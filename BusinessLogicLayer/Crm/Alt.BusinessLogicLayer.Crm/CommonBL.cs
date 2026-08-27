using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.JsonConverters;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class CommonBL : CrmBaseBL
    {
        const string tradeMailingSettingsGlobalParameterName = "TradeAutomaticMailingProcessesSettings";
        public CommonBL(GlobalContext globalContext) : base(globalContext) { }


        public bool ExecuteTradeAutomaticMailing(EntityReference regardingObjectId, Recipient recipient, EntityReference parserEntryPointstring, string mailingProcessName)
        {
            this.GlobalContext.LogEntry();
            bool isSucceededSendSms = false;
            bool isSucceededSendEmail = false;
            string globalParameterValue = this.GlobalContext.CacheManager.GetGlobalParameter<string>(tradeMailingSettingsGlobalParameterName);
            var settings = JsonSerializer.Deserialize<AutomaticMailingSettings>(globalParameterValue);
            var processSettings = settings?.MailingProcessesSettings?.Where(s => s.ProcessName == mailingProcessName).FirstOrDefault();
            if (processSettings != null
                && (processSettings.EmailTemplateCode != null
                    || processSettings.SmsTemplateCode != null))
            {
                string parserEntryPoint = parserEntryPointstring != null ? JsonSerializer.Serialize(parserEntryPointstring) : null;

                if (processSettings.SmsTemplateCode != null
                    && !string.IsNullOrWhiteSpace(recipient.MobilePhone)
                    && this.SendSms(regardingObjectId, recipient, processSettings.SmsTemplateCode, null, parserEntryPoint))
                {
                    isSucceededSendSms = true;
                }
                if (processSettings.EmailTemplateCode != null
                    && !string.IsNullOrWhiteSpace(recipient.Email)
                    && SendEmail(regardingObjectId, recipient, processSettings.EmailTemplateCode.Value, parserEntryPoint))
                {
                    isSucceededSendEmail = true;
                }
            }
            return isSucceededSendSms || isSucceededSendEmail;
        }

        private bool SendEmail(EntityReference regardingObjectId, Recipient recipient, int emailTemplateCode, string parserEntryPoint)
        {
            this.GlobalContext.LogEntry();
            bool isSentEmail = true;

            try
            {
                EmailBL emailBl = new EmailBL(this.GlobalContext);
                emailBl.CreateEmail(regardingObjectId, recipient, emailTemplateCode, parserEntryPoint);
            }
            catch (Exception ex)
            {
                isSentEmail = false;
                this.GlobalContext.Log.Error(ex.ToString());
            }
            return isSentEmail;
        }

        private bool SendSms(EntityReference regardingObjectId, Recipient recipient, int? smsTemplateCode, EntityReference smsTemplateId, string parserEntryPoint)
        {
            this.GlobalContext.LogEntry();
            bool isSentSms = true;
            try
            {
                SmsBL smsBl = new SmsBL(this.GlobalContext);
                smsBl.CreateSms(regardingObjectId, recipient, smsTemplateCode, smsTemplateId, parserEntryPoint);
            }
            catch (Exception ex)
            {
                isSentSms = false;
                this.GlobalContext.Log.Error(ex.ToString());
            }
            return isSentSms;
        }

        internal bool IsIdentificationNumbersEqual(string onlineIdentificationNumber, string identificationNumber)
        {
            GlobalContext.LogEntry();
            return onlineIdentificationNumber?.GetPadedLeftZeroString() == identificationNumber?.GetPadedLeftZeroString();
        }

        internal void SendAppNotificationForDuplicateLeadOrOpportunity(Entity entity)
        {
            GlobalContext.LogEntry();
            string appNotificationSettingParam = GlobalContext.CacheManager.GetGlobalParameter<string>("LeadOpportunityDuplicateDetectionAppNotificationSettings");
            AppNotificationSettings appNotificationSettings = JsonSerializer.Deserialize<AppNotificationSettings>(appNotificationSettingParam,
                new JsonSerializerOptions
                {
                    Converters = { new EntityReferenceJsonConverter() }
                });

            SetNotificationSettingsByEntityLogicalName(appNotificationSettings, entity);

            CommonDAL commonDal = new CommonDAL(this.GlobalContext, null);
            appNotificationSettings.Body = commonDal.GetParsedMessage(appNotificationSettings.Body, entity.ToEntityReference());
            
            List<EntityReference> usersToNotifyList = GetUsersToNotifyByEntityReferences(appNotificationSettings.Recipients);
            
            if (appNotificationSettings.SendToOwner)
            {
                EntityReference entityOwner = (EntityReference)entity[Lead.Fields.OwnerId];
                EntityReference entityOwnerToNotify = GetUsersToNotifyByEntityReferences(new List<EntityReference> { entityOwner }).FirstOrDefault();
                if (entityOwnerToNotify != null && !usersToNotifyList.Any(u => u.Id == entityOwnerToNotify.Id)) 
                { 
                    usersToNotifyList.Add(entityOwnerToNotify); 
                }
            }

            foreach (EntityReference recipient in usersToNotifyList)
            {
                commonDal.SendAppNotification(recipient, appNotificationSettings);
            }
        }

        private void SetNotificationSettingsByEntityLogicalName(AppNotificationSettings appNotificationSettings, Entity entity)
        {
            GlobalContext.LogEntry();
            string entitydisplayName = entity.LogicalName == Opportunity.EntityLogicalName ? "הזדמנות" : "הפניה";
            //appNotificationSettings.Title = appNotificationSettings.Title.Replace("EntityDisplayName", entitydisplayName);
            appNotificationSettings.Body = appNotificationSettings.Body.Replace("EntityDisplayName", entitydisplayName);
            if (entity.LogicalName == Opportunity.EntityLogicalName)
            {
                appNotificationSettings.Body = appNotificationSettings.Body.Replace(Lead.Fields.MobilePhone, Opportunity.Fields.alt_MobilePhone);
                appNotificationSettings.Body = appNotificationSettings.Body.Replace(Lead.Fields.LeadSourceCode,
                    $"{Opportunity.Fields.OriginatingLeadId}>{Lead.EntityLogicalName}.{Lead.Fields.LeadSourceCode}");
            }

            appNotificationSettings.Actions = GenerateSidePaneAppNotificationActions(entity, entitydisplayName);
        }

        private List<EntityReference> GetUsersToNotifyByEntityReferences(List<EntityReference> recipientReferences)
        {
            GlobalContext.LogEntry();
            List<EntityReference> usersToNotifyList = new List<EntityReference>();

            if (recipientReferences?.Count > 0)
            {
                TeamDAL teamDAL = new TeamDAL(GlobalContext);
                foreach (EntityReference recipient in recipientReferences)
                {
                    EntityReference userToNotify = recipient;
                    if (recipient.LogicalName == Team.EntityLogicalName)
                    {
                        if (recipient.KeyAttributes.TryGetValue(Team.Fields.alt_TeamCodeInt, out object teamCode))
                        {
                            int teamCodeInt = int.Parse(teamCode.ToString());
                            userToNotify = teamDAL.GetFirstOrDefaultByAttribute(Team.Fields.alt_TeamCodeInt, teamCodeInt,
                                                new string[] { Team.Fields.AdministratorId }).AdministratorId;
                        }
                        else
                        {
                            userToNotify = teamDAL.Get(recipient.Id, 
                                new string[] { Team.Fields.alt_TeamCodeInt, Team.Fields.AdministratorId }).AdministratorId;
                        }
                    }

                    if (!usersToNotifyList.Any(u => u.Id == userToNotify.Id))
                    {
                        usersToNotifyList.Add(userToNotify);
                    }
                }
            }

            return usersToNotifyList;
        }

        private Entity GenerateSidePaneAppNotificationActions(Entity entity, string entitydisplayName)
        {
            GlobalContext.LogEntry();

            Entity actions = new Entity()
            {
                Attributes =
                {
                    ["actions"] = new EntityCollection()
                    {
                       Entities =
                       {
                          new Entity()
                          {
                             Attributes =
                             {
                                ["title"] = "ראה את הרשומה",
                                ["data"] = new Entity()
                                {
                                   Attributes =
                                   {
                                      ["type"] = "sidepane",
                                      ["paneOptions"] = new Entity
                                      {
                                         Attributes =
                                         {
                                            ["title"] = entitydisplayName,
                                            ["width"] = 400
                                         }
                                      },
                                      ["navigationTarget"] = new Entity
                                      {
                                          Attributes =
                                          {
                                              ["pageType"] = "entityrecord",
                                              ["entityName"] = entity.LogicalName,
                                              ["entityId"] = entity.Id
                                          }
                                      }
                                   }
                                }
                             }
                          }
                       }
                    }
                }
            };
            return actions;
        }
    }
}