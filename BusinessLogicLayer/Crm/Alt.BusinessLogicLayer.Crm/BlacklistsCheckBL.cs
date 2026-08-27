using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class BlacklistsCheckBL : CrmBaseBL
    {
        public BlacklistsCheckBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetName(alt_BlacklistsCheck targetBlacklistsCheck)
        {
            GlobalContext.LogEntry();
            if (!string.IsNullOrWhiteSpace(targetBlacklistsCheck.alt_FirstName)
                || !string.IsNullOrWhiteSpace(targetBlacklistsCheck.alt_LastName))
            {
                targetBlacklistsCheck.alt_Name = $"{targetBlacklistsCheck.alt_FirstName} {targetBlacklistsCheck.alt_LastName}";
            }
        }

        public void SetDefaultValues(alt_BlacklistsCheck targetBlacklistsCheck)
        {
            GlobalContext.LogEntry();
            //  SetDefaultOwner(targetBlacklistsCheck);
            SetDefaultCountry(targetBlacklistsCheck);
        }

        public void HandleStatusCode(alt_BlacklistsCheck targetBlacklistsCheck)
        {
            if (targetBlacklistsCheck.AttributeHasValue<OptionSetValue>(alt_BlacklistsCheck.Fields.StatusCode)
                && targetBlacklistsCheck.StatusCode.Value == (int)BlacklistsCheckStatusCode.Send)
            {
                targetBlacklistsCheck.StatusCode = new OptionSetValue((int)BlacklistsCheckStatusCode.Sending);
            }
        }

        private void SetDefaultCountry(alt_BlacklistsCheck targetBlacklistsCheck)
        {
            GlobalContext.LogEntry();
            CountryDAL countryDAL = new CountryDAL(GlobalContext);
            targetBlacklistsCheck.alt_CountryId = countryDAL.GetCountryByCodeWithCache().ToEntityReference();
        }

        public void SetDefaultOwner(Entity targetEntity)
        {
            GlobalContext.LogEntry();
            TeamDAL teamDAL = new TeamDAL(GlobalContext);
            targetEntity["ownerid"] = teamDAL.GetTeamByCodeWithCache().ToEntityReference();
        }

        public void SendAppNotificationOnReceivedResponse(alt_BlacklistsCheck targetBlacklistsCheck, alt_BlacklistsCheck preBlacklistsCheck)
        {
            GlobalContext.LogEntry();

            if (targetBlacklistsCheck.Contains(alt_BlacklistsCheck.Fields.StateCode)
                && targetBlacklistsCheck.AttributeHasValue<OptionSetValue>(alt_BlacklistsCheck.Fields.StatusCode)
                && targetBlacklistsCheck.StatusCode.Value == (int)BlacklistsCheckStatusCode.ReceivedResponse)
            {
                var mergedBlacklistsCheck = targetBlacklistsCheck.Merge(preBlacklistsCheck);
                if (mergedBlacklistsCheck.OwnerId.LogicalName == SystemUser.EntityLogicalName)
                {
                    AppNotificationSettings appNotificationSettings = JsonSerializer
                 .Deserialize<AppNotificationSettings>(GlobalContext.CacheManager.GetGlobalParameter<string>("BlacklistsCheckAppNotificationSettings"));
                    appNotificationSettings.Actions = this.GenerateAppNotificationActions(mergedBlacklistsCheck);

                    if (targetBlacklistsCheck.alt_AppearsInBlacklistsCode != null)
                    {
                        appNotificationSettings.IconType = targetBlacklistsCheck.alt_AppearsInBlacklistsCode.Value == (int)AppearsInBlacklistsCode.NotAppears ?
                            AppNotificationIconTypeCode.Success : AppNotificationIconTypeCode.Failure;
                    }
                    else
                    {
                        appNotificationSettings.IconType = AppNotificationIconTypeCode.Info;
                    }
                    CommonDAL commonDal = new CommonDAL(this.GlobalContext, null);
                    appNotificationSettings.Body = commonDal.GetParsedMessage(appNotificationSettings.Body, targetBlacklistsCheck.ToEntityReference());
                    commonDal.SendAppNotification(mergedBlacklistsCheck.OwnerId, appNotificationSettings);
                }
            }
        }

        private Entity GenerateAppNotificationActions(alt_BlacklistsCheck mergedBlacklistsCheck)
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
                                                  ["title"] = "רשימות שחורות",
                                                  ["width"] = 400
                                               }
                                            },
                                            ["navigationTarget"] = new Entity
                                            {
                                                Attributes =
                                                {
                                                    ["pageType"] = "entityrecord",
                                                    ["entityName"] = alt_BlacklistsCheck.EntityLogicalName,
                                                    ["entityId"] = mergedBlacklistsCheck.Id
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

        public void HandleUpdateAccountHolder(alt_BlacklistsCheck targetBlacklistsCheck, alt_BlacklistsCheck preBlacklistsCheck)
        {
            GlobalContext.LogEntry();

            if (preBlacklistsCheck.AttributeHasValue<EntityReference>(alt_BlacklistsCheck.Fields.alt_AaccountHolderId))
            {
                AccountHolderDAL accountHolderDAL = new AccountHolderDAL(this.GlobalContext);
                var checkTerrorOrganizationCode = targetBlacklistsCheck.alt_AppearsInBlacklistsCode.Value == (int)AppearsInBlacklistsCode.NotAppears
                    ? CheckTerrorOrganizationCode.Valid : CheckTerrorOrganizationCode.Invalid;

                alt_AccountHolder accountHolderToUpdate = new alt_AccountHolder()
                {
                    Id = preBlacklistsCheck.alt_AaccountHolderId.Id,
                    alt_CheckTerrorOrganizationCode = new OptionSetValue((int)checkTerrorOrganizationCode),
                    alt_CheckTerrorOrganizationSystemUserId = preBlacklistsCheck.CreatedBy
                };

                accountHolderDAL.Update(accountHolderToUpdate);
            }
        }
    }

}