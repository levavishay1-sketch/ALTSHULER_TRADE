using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class ArchiveDocumentSearchBL : ExternalBLBase, ICrmOutgoing<ApiArchiveDocumentSearch>
    {
        public ArchiveDocumentSearchBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiArchiveDocumentSearch> apiContext)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            if (this.ApiConfiguration != null)
            {
                switch (this.ApiConfiguration.Code.Value)
                {
                    case (int)ApiConfigurationCode.DocumentSearch:
                        {
                            actionResult = this.HandleDocumentSearch(apiContext.Target);
                            break;
                        }
                    default:
                        {
                            actionResult.SetToFailedActionResult(CustomErrorCodes.UnrecognizedApiCodeForDocument);
                            break;
                        }
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }
            return actionResult;
        }

        private ActionResult HandleDocumentSearch(ApiArchiveDocumentSearch apiArchiveDocumentSearch)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ArchiveDocumentSearchDAL archiveDocumentSearchDAL = new ArchiveDocumentSearchDAL(this.GlobalContext);
            ApiArchiveDocumentSearch retrievedApiArchiveDocumentSearch = archiveDocumentSearchDAL.GetByAttribute(
                "activityid",
                apiArchiveDocumentSearch.Id.Value,
                new string[] { "regardingobjectid", "alt_lastsearchdate" }
            ).First();

            List<ApiCustomer> customers = this.GetCustomerIdFromRegardingObject(retrievedApiArchiveDocumentSearch);
            retrievedApiArchiveDocumentSearch.ProcessCode = this.GetProcessCodeFromRegardingObject(retrievedApiArchiveDocumentSearch);
            List<ESBDocumentMetaData> esbResponses = new List<ESBDocumentMetaData>();
            ESBResponse<List<ESBDocumentMetaData>> searchResponse = new ESBResponse<List<ESBDocumentMetaData>>();

            this.GlobalContext.Log.Info($"Retrieved Customers Number: {customers?.Count}");
            foreach (var customer in customers)
            {
                retrievedApiArchiveDocumentSearch.Customer = customer;
                ESBArchiveDocumentSearchDAL eSBDocumentSearchForEntityDAL = new ESBArchiveDocumentSearchDAL(this.GlobalContext, this.ApiConfiguration);
                actionResult = eSBDocumentSearchForEntityDAL.ExecuteRequest(retrievedApiArchiveDocumentSearch);
                if (actionResult.IsSuccess)
                {
                    searchResponse = JsonUtils.Deserialize<ESBResponse<List<ESBDocumentMetaData>>>(actionResult.ReturnObject.ToString());
                    List<ESBDocumentMetaData> retrievedDocuments = searchResponse.ResponseData ?? new List<ESBDocumentMetaData>();
                    esbResponses.AddRange(retrievedDocuments);
                }
                else
                {
                    break;
                }
            }

            searchResponse.ResponseData = esbResponses;
            actionResult.ReturnObject = searchResponse;
            this.HandleSearchResult(retrievedApiArchiveDocumentSearch, customers, actionResult);
            return actionResult;
        }

        private void HandleSearchResult(ApiArchiveDocumentSearch apiArchiveDocumentSearch, List<ApiCustomer> customers, ActionResult actionResult)
        {
            this.GlobalContext.LogEntry();
            Dictionary<string, ApiCustomer> customersDictionary = customers.ToDictionary((c) => c.CustomerIdentity);
            ApiArchiveDocumentSearch archiveDocumentSearchToUpdate = new ApiArchiveDocumentSearch { Id = apiArchiveDocumentSearch.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;
            if (actionResult.IsSuccess)
            {
                ESBResponse<List<ESBDocumentMetaData>> searchResponse = actionResult.ReturnObject as ESBResponse<List<ESBDocumentMetaData>>;
                resultStatus = searchResponse.ResultStatusCode;

                DocumentDAL documentDAL = new DocumentDAL(GlobalContext);

                List<ApiDocument> apiDocumentCollectionToUpsert = new List<ApiDocument>();
                List<string> retrievedCurrentDocumentsIdentifiers = documentDAL.GetActiveByAttribute("alt_regardingid", apiArchiveDocumentSearch.RegardingObject.Id.Value, new[] { "alt_filearchiveidentifier", "alt_documentid" }).Select(t => t.FileArchiveIdentifier).ToList();//.ToDictionary((d) => { return d.FileArchiveIdentifier; });
                HashSet<string> retrievedCurrentRelatedDocumentsHashSet = new HashSet<string>(retrievedCurrentDocumentsIdentifiers);
                List<ESBDocumentMetaData> retrievedDocuments = searchResponse.ResponseData ?? new List<ESBDocumentMetaData>();
                foreach (ESBDocumentMetaData esbDocumentMetaData in retrievedDocuments)
                {
                    if (!retrievedCurrentRelatedDocumentsHashSet.Contains(esbDocumentMetaData.OpenTextID))
                        apiDocumentCollectionToUpsert.Add(new ApiDocument
                        {
                            CustomerID = customersDictionary.ContainsKey(esbDocumentMetaData.CustomerID) ? customersDictionary[esbDocumentMetaData.CustomerID] : null,
                            FileArchiveIdentifier = esbDocumentMetaData.OpenTextID,
                            Name = esbDocumentMetaData.FileName,
                            Regarding = new ApiEntity(apiArchiveDocumentSearch.RegardingObject.LogicalName) { Id = apiArchiveDocumentSearch.RegardingObject.Id },
                            MimeType = MimeMapping.GetMimeMapping(esbDocumentMetaData.FileName),
                            Publish = Convert.ToBoolean(Convert.ToInt16(esbDocumentMetaData.Publish)),
                            ProductTypeCode = Convert.ToInt16(esbDocumentMetaData.ProductCode)
                        });
                }

                actionResult = documentDAL.ExecuteMultipleRequestsInChunks(apiDocumentCollectionToUpsert, RequestType.Create);
                if (!actionResult.IsSuccess)
                {
                    List<string> errors = (List<string>)actionResult.ReturnObject;
                    this.GlobalContext.Log.Error($"Errors: {string.Join(",", errors)}");
                }
            }

            actionResult.IsSuccess = (actionResult.IsSuccess && resultStatus != null && resultStatus == ESBResultStatusCode.Success);
            archiveDocumentSearchToUpdate.SearchFromArchiveStatusCode = actionResult.IsSuccess ? (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;
            archiveDocumentSearchToUpdate.LastSearchDate = actionResult.IsSuccess ? DateTime.UtcNow : archiveDocumentSearchToUpdate.LastSearchDate;
            ArchiveDocumentSearchDAL archiveDocumentSearchDAL = new ArchiveDocumentSearchDAL(this.GlobalContext);
            archiveDocumentSearchDAL.Update(archiveDocumentSearchToUpdate);
        }

        private List<ApiCustomer> GetCustomerIdFromRegardingObject(ApiArchiveDocumentSearch apiDocumentSearchForEntity)
        {
            this.GlobalContext.LogEntry();

            string regardingLogicalName = apiDocumentSearchForEntity.RegardingObject.LogicalName;
            Guid regardingId = apiDocumentSearchForEntity.RegardingObject.Id.Value;
            List<ApiCustomer> customers = new List<ApiCustomer>();

            this.GlobalContext.Log.Info($"regarding: {regardingLogicalName} - {regardingId}");

            switch (regardingLogicalName)
            {
                case ApiIncident.EntityLogicalName:
                    {
                        IncidentDAL incidentDAL = new IncidentDAL(this.GlobalContext);
                        ApiIncident apiIncident = incidentDAL.Get(regardingId, new string[] { "alt_externalidentifier", "customerid" });
                        CustomerDAL customerDAL = new CustomerDAL(this.GlobalContext, apiIncident.Customer.LogicalName);
                        apiIncident.Customer.CustomerIdentity = customerDAL.GetCustomerArchiveIdentifier(apiIncident.Customer);
                        customers.Add(apiIncident.Customer);
                        break;
                    }
                case ApiOpportunity.EntityLogicalName:
                    {
                        OpportunityDAL opportunityDAL = new OpportunityDAL(this.GlobalContext);
                        ApiOpportunity apiOpportunity = opportunityDAL.Get(regardingId, new string[] { "alt_opportunityidentitynumber", "customerid" });
                        CustomerDAL customerDAL = new CustomerDAL(this.GlobalContext, apiOpportunity.CustomerId.LogicalName);
                        apiOpportunity.CustomerId.CustomerIdentity = customerDAL.GetCustomerArchiveIdentifier(apiOpportunity.CustomerId);
                        customers.Add(apiOpportunity.CustomerId);
                        break;
                    }
                case ApiDigitalFormVerification.EntityLogicalName:
                case ApiPortfolio.EntityLogicalName:
                    {
                        string accountHolderRelatedAttribute = regardingLogicalName == ApiDigitalFormVerification.EntityLogicalName ? "alt_digitalformverificationid" : "alt_portfolioid";
                        AccountHolderDAL accountHolderDAL = new AccountHolderDAL(this.GlobalContext);
                        customers = accountHolderDAL.GetActiveByAttribute(accountHolderRelatedAttribute,
                              apiDocumentSearchForEntity.RegardingObject.Id, new[] { "alt_customerid", "alt_accountholdertypecode" }).Where(t => t.AccountHolderTypeCode != (int)AccountHolderTypeCode.Beneficiary).Select(a => a.CustomerId).ToList();
                        CustomerDAL customerDAL = null;
                        foreach (var customer in customers)
                        {
                            customerDAL = new CustomerDAL(this.GlobalContext, customer.LogicalName);
                            customer.CustomerIdentity = customerDAL.GetCustomerArchiveIdentifier(customer);
                        }
                        break;
                    }

                default:
                    break;
            }

            return customers;
        }

        private string GetProcessCodeFromRegardingObject(ApiArchiveDocumentSearch apiDocumentSearchForEntity)
        {
            this.GlobalContext.LogEntry();

            string regardingLogicalName = apiDocumentSearchForEntity.RegardingObject.LogicalName;
            Guid regardingID = apiDocumentSearchForEntity.RegardingObject.Id.Value;
            string processCode = string.Empty;

            switch (regardingLogicalName)
            {
                case ApiIncident.EntityLogicalName:
                    {
                        IncidentDAL incidentDAL = new IncidentDAL(this.GlobalContext);
                        ApiIncident apiIncident = incidentDAL.Get(regardingID, new string[] { "alt_externalidentifier" });
                        processCode = !string.IsNullOrWhiteSpace(apiIncident.ExternalIdentifier) ? apiIncident.ExternalIdentifier : regardingID.ToString();
                        break;
                    }
                case ApiOpportunity.EntityLogicalName:
                    {
                        OpportunityDAL opportunityDAL = new OpportunityDAL(this.GlobalContext);
                        ApiOpportunity apiOpportunity = opportunityDAL.Get(regardingID, new string[] { "alt_opportunityidentitynumber" });
                        processCode = apiOpportunity.OpportunityIdentityNumber;
                        break;
                    }
                case ApiDigitalFormVerification.EntityLogicalName:
                    {
                        DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                        ApiDigitalFormVerification apiDigitalFormVerification = digitalFormVerificationDAL.Get(regardingID, new string[] { "alt_digitalformnumber" });
                        processCode = apiDigitalFormVerification.DigitalFormNumber;
                        break;
                    }
                case ApiPortfolio.EntityLogicalName:
                    {
                        PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
                        ApiPortfolio apiPortfolio = portfolioDAL.Get(regardingID, new string[] { "alt_shenhavaccountnumber" });
                        processCode = apiPortfolio.ShenhavAccountNumber;
                        break;
                    }
                default:
                    break;
            }

            return processCode;
        }
    }
}
