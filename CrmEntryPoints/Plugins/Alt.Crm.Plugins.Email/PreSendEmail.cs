using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.Crm.Plugins.Email
{
    /// <summary>
    /// This plugin just for test environment !!!
    /// </summary>
    public class PreSendEmail : PluginBase
    {
        public PreSendEmail(string unsecure, string secure) : base(typeof(PreSendEmail))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var globalContext = localContext.ToGlobal();
            globalContext.LogEntry();

            if (IsUseWhiteList(globalContext))
            {
                var inputParameters = localContext.PluginExecutionContext.InputParameters;
                if (inputParameters.Contains("EmailId"))
                {
                    var emailId = (Guid)inputParameters["EmailId"];
                    this.CheckEmailAdresses(emailId, globalContext);
                }
            }
        }

        private void CheckEmailAdresses(Guid emailId, GlobalContext globalContext)
        {
            globalContext.LogEntry();

            Entity email = globalContext.OrganizationService.Retrieve(DataModel.Crm.Entities.Email.EntityLogicalName, emailId, new ColumnSet(DataModel.Crm.Entities.Email.Fields.To, DataModel.Crm.Entities.Email.Fields.Cc));
            var to = email.GetAttributeValue<EntityCollection>(DataModel.Crm.Entities.Email.Fields.To);
            var cc = email.GetAttributeValue<EntityCollection>(DataModel.Crm.Entities.Email.Fields.Cc);

            List<string> toEmailAdresses = GetEmailAdresses(to?.Entities, globalContext);
            List<string> ccEmailAdresses = GetEmailAdresses(cc?.Entities, globalContext);
            toEmailAdresses.AddRange(ccEmailAdresses);

            if (!this.IsAllInWhiteList(toEmailAdresses, globalContext))
            {
                this.CancelEmail(emailId, globalContext);
                throw new InvalidPluginExecutionException("Email canceled. Not Found in WhiteList Error.");
            }
        }

        private void CancelEmail(Guid emailId, GlobalContext globalContext)
        {
            globalContext.LogEntry();

            Entity emailToUpdate = new Entity(DataModel.Crm.Entities.Email.EntityLogicalName, emailId);
            emailToUpdate[DataModel.Crm.Entities.Email.Fields.StatusCode] = new OptionSetValue((int)EmailStatusCode.Canceled);
            emailToUpdate[DataModel.Crm.Entities.Email.Fields.StateCode] = new OptionSetValue(2);
            globalContext.OrganizationService.Update(emailToUpdate);
        }

        private bool IsAllInWhiteList(List<string> toEmailAdresses, GlobalContext globalContext)
        {
            globalContext.LogEntry();

            var whiteList = this.GetWhiteList(globalContext);
            if (whiteList != null && whiteList.Count > 0)
            {
                foreach (var emailAddress in toEmailAdresses)
                {
                    if (!whiteList.Contains(emailAddress))
                    {
                        globalContext.Log.Warning($"Cannot send email. Emailaddress {emailAddress} is not in WhiteList.");
                        return false;
                    }
                }
            }
            else
            {
                globalContext.Log.Warning("WhiteList not found or empty.");
                return false;
            }
            return true;
        }

        private List<string> GetEmailAdresses(DataCollection<Entity> entities, GlobalContext globalContext)
        {
            globalContext.LogEntry();

            List<string> emailAddresses = new List<string>();
            string emailAddressAttributeName = "emailaddress1";

            if (entities != null && entities.Count > 0)
            {
                foreach (var entity in entities)
                {
                    string addressUsed = entity.GetAttributeValue<string>(ActivityParty.Fields.AddressUsed);
                    if (!string.IsNullOrWhiteSpace(addressUsed))
                    {
                        emailAddresses.Add(addressUsed);
                    }
                    else
                    {
                        EntityReference partyId = entity.GetAttributeValue<EntityReference>(ActivityParty.Fields.PartyId);
                        if (partyId != null)
                        {
                            Entity retrievedEntity = globalContext.OrganizationService.Retrieve(partyId.LogicalName, partyId.Id, new ColumnSet(emailAddressAttributeName));
                            string emailaddress = retrievedEntity.GetAttributeValue<string>(emailAddressAttributeName);
                            if (!string.IsNullOrWhiteSpace(emailaddress))
                            {
                                emailAddresses.Add(emailaddress);
                            }
                        }
                    }
                }
            }
            return emailAddresses;
        }

        private List<string> GetWhiteList(GlobalContext globalContext)
        {
            globalContext.LogEntry();
            List<string> whiteList = null;

            string globalParameterValue = globalContext.CacheManager.GetGlobalParameter<string>("WhiteList");

            if (!string.IsNullOrWhiteSpace(globalParameterValue))
            {
                whiteList = globalParameterValue.Split(',').ToList();
            }

            return whiteList != null ? whiteList.Select(r => r.Trim()).ToList() : whiteList;
        }

        private bool IsUseWhiteList(GlobalContext globalContext)
        {
            globalContext.LogEntry();
            var result = globalContext.CacheManager.GetEnvironmentVariable("alt_UseWhiteList");
            return result.ToString() == "yes";
        }
    }
}
