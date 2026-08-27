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
using Alt.Framework.Extensions;
using Alt.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class DigitalFormVerificationBL : ExternalBLBase, ICrmOutgoing<ApiDigitalFormVerification>
    {
        int errorDescriptionLength = 1000;
        string digitalFormVerificationsSearchRangeParameterName = "DigitalFormVerificationsSearchRange";

        public DigitalFormVerificationBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public Guid CreateDigitalFormVerification(ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
            return digitalFormVerificationDAL.Create(apiDigitalFormVerification);
        }

        public void UpdateDigitalFormVerification(ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
            digitalFormVerificationDAL.Update(apiDigitalFormVerification);
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiDigitalFormVerification> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (this.ApiConfiguration != null)
            {
                ApiConfigurationCode apiConfigurationCode = (ApiConfigurationCode)this.ApiConfiguration.Code;
                switch (apiConfigurationCode)
                {
                    case ApiConfigurationCode.OpenPortfolioInShenhav:
                        {
                            actionResult = this.HandleOpenPortfolioInShenhav(apiContext);
                            break;
                        }
                    default:
                        break;
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }
            return actionResult;
        }

        public ActionResult HandleDocumentSearchForDigitalFormVerifications(ApiScheduledOperation scheduledOperation)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(GlobalContext);
            ArchiveDocumentSearchDAL archiveDocumentSearchDAL = new ArchiveDocumentSearchDAL(GlobalContext);

            int daysRange = digitalFormVerificationDAL.CacheManager.GetGlobalParameter<int>(digitalFormVerificationsSearchRangeParameterName);
            List<ApiDigitalFormVerification> digitalFormVerifications = digitalFormVerificationDAL.GetMultipleByDateLastXDays("createdon", daysRange, new[] { "alt_digitalformverificationid" });

            List<ApiArchiveDocumentSearch> documentSearchCollectionToCreate = new List<ApiArchiveDocumentSearch>();
            List<ApiArchiveDocumentSearch> documentSearchCollectionToUpdate = new List<ApiArchiveDocumentSearch>();

            this.HandleBuildUpdateAndCreateCollctions(archiveDocumentSearchDAL, digitalFormVerifications, documentSearchCollectionToCreate, documentSearchCollectionToUpdate, daysRange);
            this.ExecuteArchiveDocumentSearchRequestsHandler(archiveDocumentSearchDAL, documentSearchCollectionToCreate, documentSearchCollectionToUpdate);

            return actionResult;
        }

        public ActionResult HandleMainAccountHoldersManualMailing(ApiScheduledOperation scheduledOperation, ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            int? customerOperationTemplateCode = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(customerOperationTemplateCode), out customerOperationTemplateCode)
                ? customerOperationTemplateCode : 0;

            string fetchXmlGlobalParameterName = retrievedSchedulerSetup.TryGetSettingsItemValue(nameof(fetchXmlGlobalParameterName), out fetchXmlGlobalParameterName)
                ? fetchXmlGlobalParameterName : string.Empty;

            string fetchXML = GlobalContext.CacheManager.GetGlobalParameter<string>(fetchXmlGlobalParameterName);

            List<ApiDigitalFormVerification> retrievedDigitalFormVerifications = new DigitalFormVerificationDAL(this.GlobalContext)
                .GetDigitalFormVerificationsByFetchXML(fetchXML);

            List<ApiCustomerOperationRequest> customerOperationRequestsToUpsert = new List<ApiCustomerOperationRequest>();
            foreach (ApiDigitalFormVerification digitalFormVerification in retrievedDigitalFormVerifications)
            {
                customerOperationRequestsToUpsert.Add(this.GenerateCustomerOperationRequest(digitalFormVerification, customerOperationTemplateCode.Value));
            }

            actionResult = new CustomerOperationRequestDAL(this.GlobalContext)
                .ExecuteMultipleRequestsInChunks(customerOperationRequestsToUpsert, RequestType.Upsert);

            return actionResult;
        }

        private ApiCustomerOperationRequest GenerateCustomerOperationRequest(ApiDigitalFormVerification digitalFormVerification, int customerOperationTemplateCode)
        {
            this.GlobalContext.LogEntry();

            ApiAccountHolder primaryAccountHolder = digitalFormVerification.PrimaryAccountHolderId;
            ApiCustomerOperationRequest retrievedCustomerOperationRequest = new CustomerOperationRequestDAL(this.GlobalContext)
                .GetCustomerOperationRequestByTemplateCodeAndRelated(customerOperationTemplateCode, primaryAccountHolder.Id.Value);

            ApiCustomerOperationRequest customerOperationRequest = new ApiCustomerOperationRequest()
            {
                Id = retrievedCustomerOperationRequest?.Id,
                StatusCode = (int)CustomerOperationRequestStatusCode.Send
            };
            if (retrievedCustomerOperationRequest == null)
            {
                customerOperationRequest.RelatedRecordId = primaryAccountHolder;
                customerOperationRequest.CustomerOperationTemplateCode = customerOperationTemplateCode;
            }

            return customerOperationRequest;
        }

        private void HandleBuildUpdateAndCreateCollctions(ArchiveDocumentSearchDAL archiveDocumentSearchDAL, List<ApiDigitalFormVerification> digitalFormVerifications,
            List<ApiArchiveDocumentSearch> documentSearchCollectionToCreate, List<ApiArchiveDocumentSearch> documentSearchCollectionToUpdate, int daysRange)
        {
            var archiveDocumentSearches = archiveDocumentSearchDAL.GetArchiveDocumentSearchesCreatedInTheLastXDaysHandler(daysRange);
            foreach (ApiDigitalFormVerification apiDigitalFormVerification in digitalFormVerifications)
            {
                if (archiveDocumentSearches.ContainsKey(apiDigitalFormVerification.Id.GetValueOrDefault()))
                {
                    ApiArchiveDocumentSearch apiArchiveDocumentSearchToUpdate = archiveDocumentSearches[apiDigitalFormVerification.Id.GetValueOrDefault()];
                    apiArchiveDocumentSearchToUpdate.SearchFromArchiveStatusCode = 2;
                    documentSearchCollectionToUpdate.Add(apiArchiveDocumentSearchToUpdate);
                }
                else
                {
                    ApiArchiveDocumentSearch apiArchiveDocumentSearchToCreate = new ApiArchiveDocumentSearch
                    {
                        RegardingObject = apiDigitalFormVerification,
                        SearchFromArchiveStatusCode = 2,
                        Subject = $"חיפוש מסמכים מארכיון עבור {apiDigitalFormVerification.LogicalName} - {apiDigitalFormVerification.LogicalName}"
                    };
                    documentSearchCollectionToCreate.Add(apiArchiveDocumentSearchToCreate);
                }
            }
        }

        private ActionResult ExecuteArchiveDocumentSearchRequestsHandler(ArchiveDocumentSearchDAL archiveDocumentSearchDAL, List<ApiArchiveDocumentSearch> documentSearchCollectionToCreate
            , List<ApiArchiveDocumentSearch> documentSearchCollectionToUpdate)
        {
            ActionResult finalActionResult = new ActionResult();
            ActionResult createArchiveDocumentSearchActionResult = new ActionResult();
            ActionResult updateArchiveDocumentSearchActionResult = new ActionResult();

            createArchiveDocumentSearchActionResult = archiveDocumentSearchDAL.ExecuteMultipleRequestsInChunks(documentSearchCollectionToCreate, RequestType.Create);
            updateArchiveDocumentSearchActionResult = archiveDocumentSearchDAL.ExecuteMultipleRequestsInChunks(documentSearchCollectionToUpdate, RequestType.Update);

            if (!createArchiveDocumentSearchActionResult.IsSuccess)
            {
                finalActionResult.IsSuccess = false;
                finalActionResult.ReturnObject = createArchiveDocumentSearchActionResult.ReturnObject;
            }
            if (!updateArchiveDocumentSearchActionResult.IsSuccess)
            {
                finalActionResult.IsSuccess = false;
                finalActionResult.ReturnObject = !createArchiveDocumentSearchActionResult.IsSuccess
                    ? $"{createArchiveDocumentSearchActionResult.ReturnObject},{Environment.NewLine}{updateArchiveDocumentSearchActionResult.ReturnObject}"
                        : updateArchiveDocumentSearchActionResult.ReturnObject;
            }

            return finalActionResult;
        }

        private ActionResult HandleOpenPortfolioInShenhav(ApiContext<ApiDigitalFormVerification> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ApiDigitalFormVerification apiDigitalFormVerification = this.GetJoiningFormDetails(apiContext.Target.Id.Value);
            if (apiDigitalFormVerification.TransferToShenhavStatusCode != null
                && (TransferStatusCode)apiDigitalFormVerification.TransferToShenhavStatusCode.Value == TransferStatusCode.Sending)
            {
                ESBDigitalFormVerificationDAL esbDigitalFormVerificationDal = new ESBDigitalFormVerificationDAL(this.GlobalContext, this.ApiConfiguration);
                actionResult = esbDigitalFormVerificationDal.CreatePortfolioInSheinav(apiDigitalFormVerification);

                this.HandleOpenPortfolioInShenhavResponse(actionResult, apiDigitalFormVerification);
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForSendToExternalService, new[] { ((TransferStatusCode)apiDigitalFormVerification.TransferToShenhavStatusCode).ToString() });
            }
            return actionResult;
        }

        private ApiDigitalFormVerification GetJoiningFormDetails(Guid id)
        {
            this.GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
            ApiDigitalFormVerification apiDigitalFormVerification = digitalFormVerificationDal.GetDigitalFormVerificationDetails(id);

            AuthorizationManagementDAL authorizationManagementDal = new AuthorizationManagementDAL(this.GlobalContext);
            apiDigitalFormVerification.AuthorizationManagements = authorizationManagementDal.GetByAttribute("alt_digitalformverificationid", id, null);

            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            var accountHolders = accountHolderDal.GetActiveAccountHoldersByDigitalFormVerification(id);
            if (accountHolders != null && accountHolders.Count > 0)
            {
                apiDigitalFormVerification.AccountHolders = new List<ApiAccountHolder>();
                foreach (var accountHolder in accountHolders)
                {
                    KycDAL kycDal = new KycDAL(this.GlobalContext);
                    accountHolder.KYC = kycDal.GetKycDetailsByAccountHolder(accountHolder.Id.Value);

                    apiDigitalFormVerification.AccountHolders.Add(accountHolder);
                }
            }
            return apiDigitalFormVerification;
        }

        private void HandleOpenPortfolioInShenhavResponse(ActionResult actionResult, ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            ApiDigitalFormVerification apiDigitalFormVerificationToUpdate = new ApiDigitalFormVerification() { Id = apiDigitalFormVerification.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;
            string errorMessage;
            if (actionResult.IsSuccess)
            {
                var joiningFormResponse = JsonSerializer.Deserialize<ESBResponse<ESBJoiningFormResponse>>(actionResult.ReturnObject.ToString());
                errorMessage = this.TrimErrorDescription(joiningFormResponse.ErrorMessage);
                resultStatus = joiningFormResponse.ResultStatusCode;
                if (resultStatus == null)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { joiningFormResponse.ErrorCode?.ToString() });
                }
                else if (resultStatus == ESBResultStatusCode.Success)
                {
                    string accountNumber = this.ApiConfiguration.DebugMode.Value ? StringUtils.GetUniqueKey(10) : joiningFormResponse.ResponseData.AccountNumber;
                    apiDigitalFormVerificationToUpdate.PortfolioId = this.CreatePortfolio(accountNumber, apiDigitalFormVerification);
                }
                else
                {
                    actionResult.SetToFailedActionResult(errorMessage);
                }
            }
            else
            {
                errorMessage = this.TrimErrorDescription(actionResult.Error?.Message);
            }
            apiDigitalFormVerificationToUpdate.TransferToShenhavStatusCode = actionResult.IsSuccess && resultStatus != ESBResultStatusCode.Error ?
                (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;
            apiDigitalFormVerificationToUpdate.TransferToShenhavErrorDescription = errorMessage;

            this.UpdateDigitalFormVerification(apiDigitalFormVerificationToUpdate);
        }

        private ApiPortfolio CreatePortfolio(string accountNumber, ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
            ApiPortfolio apiPortfolio = new ApiPortfolio
            {
                Owner = apiDigitalFormVerification.Owner,
                ShenhavAccountNumber = accountNumber,
                JoiningProcessNumber = apiDigitalFormVerification.DigitalFormNumber,
                Name = this.GeneratePortfolioName(apiDigitalFormVerification, accountNumber)
            };
            apiPortfolio.Id = portfolioDAL.Create(apiPortfolio);

            return apiPortfolio;
        }

        private string GeneratePortfolioName(ApiDigitalFormVerification apiDigitalFormVerification, string accountNumber)
        {
            this.GlobalContext.LogEntry();
            List<string> nameParts = new List<string>() { accountNumber };
            nameParts.AddRange(apiDigitalFormVerification.AccountHolders
                .Where(a => a.AccountHolderTypeCode == (int)AccountHolderTypeCode.Owner)
                .Select(a => a.Name).ToList());
            return string.Join(" - ", nameParts);
        }

        private string TrimErrorDescription(string description)
        {
            return description?.Length > errorDescriptionLength ?
                        description.SubstringByLength(errorDescriptionLength) : description;
        }
    }
}
