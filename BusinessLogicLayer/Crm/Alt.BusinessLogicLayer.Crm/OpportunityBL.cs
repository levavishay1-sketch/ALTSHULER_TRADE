using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;

namespace Alt.BusinessLogicLayer.Crm
{
    public class OpportunityBL : CrmBaseBL
    {
        public OpportunityBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetOpportunityName(Opportunity targetOpportunity, Opportunity preOpportunity)
        {
            this.GlobalContext.LogEntry();

            if (targetOpportunity.Contains(Opportunity.Fields.CustomerId)
                || targetOpportunity.Contains(Opportunity.Fields.OriginatingLeadId)
                || targetOpportunity.Contains(Opportunity.Fields.alt_MobilePhone)
                || targetOpportunity.Contains(Opportunity.Fields.alt_CompanyName))
            {
                Opportunity mergedOpportunity = targetOpportunity.Merge(preOpportunity);
                List<string> nameParts = new List<string>();
                if (mergedOpportunity.AttributeHasValue<EntityReference>(Opportunity.Fields.CustomerId))
                {
                    nameParts.Add(this.GetCustomerName(mergedOpportunity));
                }
                else if (mergedOpportunity.AttributeHasValue<string>(Opportunity.Fields.alt_CompanyName))
                {
                    nameParts.Add(mergedOpportunity.alt_CompanyName);
                }
                else if (mergedOpportunity.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId))
                {
                    LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                    string leadName = leadDal.GetPrimeryAttributeValue(mergedOpportunity.OriginatingLeadId, Lead.Fields.FullName);
                    nameParts.Add(leadName);
                }
                if (mergedOpportunity.AttributeHasValue<string>(Opportunity.Fields.alt_MobilePhone))
                {
                    nameParts.Add(mergedOpportunity.alt_MobilePhone);
                }
                targetOpportunity.Name = string.Join(" - ", nameParts);
            }
        }

        public void HandleEncouragingDepositSystemUserInRelatedDigitalFormVerification(Opportunity targetOpportunity, Opportunity preOpportunity)
        {
            this.GlobalContext.LogEntry();

            if (targetOpportunity.AttributeHasValue<EntityReference>(Opportunity.Fields.OwnerId))
            {
                DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                alt_DigitalFormVerification retrievedDigitalFormVerification = digitalFormVerificationDAL.GetFirstOrDefaultByAttribute(
                    alt_DigitalFormVerification.Fields.alt_DigitalFormNumber,
                    preOpportunity.alt_OpportunityIdentityNumber,
                    new string[] { alt_DigitalFormVerification.Fields.alt_DigitalFormVerificationId });

                if (retrievedDigitalFormVerification != null)
                {
                    EntityReference encouragingDepositSystemUserId = targetOpportunity.OwnerId.LogicalName == SystemUser.EntityLogicalName
                            ? targetOpportunity.OwnerId : null;

                    alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification()
                    {
                        Id = retrievedDigitalFormVerification.Id,
                        alt_EncouragingDepositSystemUserId = encouragingDepositSystemUserId
                    };
                    digitalFormVerificationDAL.Update(digitalFormVerificationToUpdate);
                }
            }
        }

        public void HandleJoiningProcessSummary(Opportunity targetOpportunity)
        {
            this.GlobalContext.LogEntry();

            if (targetOpportunity.AttributeHasValue<string>(Opportunity.Fields.alt_OpportunityIdentityNumber))
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_JoiningProcessSummary.EntityLogicalName);
                Entity retrievedJoiningProcessSummary = commonDAL.GetFirstOrDefaultByAttribute<string>(alt_JoiningProcessSummary.Fields.alt_JoiningProcessIdentifier,
                    targetOpportunity.alt_OpportunityIdentityNumber, new string[] { alt_JoiningProcessSummary.Fields.alt_JoiningProcessSummaryId });

                if (retrievedJoiningProcessSummary != null)
                {
                    UpdateExistingJoiningProcessSummary(retrievedJoiningProcessSummary, commonDAL, targetOpportunity);
                }
                else
                {
                    CreateNewJoiningProcessSummary(commonDAL, targetOpportunity);
                }
            }
        }

        private void UpdateExistingJoiningProcessSummary(Entity retrievedJoiningProcessSummary, CommonDAL commonDAL, Opportunity targetOpportunity)
        {
            this.GlobalContext.LogEntry();
            alt_JoiningProcessSummary joiningProcessSummaryToUpdate = new alt_JoiningProcessSummary
            {
                Id = retrievedJoiningProcessSummary.Id,
                alt_OpportunityId = targetOpportunity.ToEntityReference()
            };
            commonDAL.Update(joiningProcessSummaryToUpdate);
        }

        private void CreateNewJoiningProcessSummary(CommonDAL commonDAL, Opportunity targetOpportunity)
        {
            this.GlobalContext.LogEntry();
            var defaultOwnerTeamCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>("DefaultOwnerTeamCode");
            alt_JoiningProcessSummary joiningProcessSummaryToCreate = new alt_JoiningProcessSummary
            {
                alt_Name = "תהליך הצטרפות - " + targetOpportunity.alt_OpportunityIdentityNumber,
                alt_OpportunityId = targetOpportunity.ToEntityReference(),
                alt_JoiningProcessIdentifier = targetOpportunity.alt_OpportunityIdentityNumber,
                OwnerId = new EntityReference(Team.EntityLogicalName, "alt_teamcodeint", defaultOwnerTeamCode)
            };

            if (targetOpportunity.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId))
            {
                joiningProcessSummaryToCreate.alt_LeadId = targetOpportunity.OriginatingLeadId;
            }
            commonDAL.Create(joiningProcessSummaryToCreate);
        }

        public void HandleRepresentativeRewardCreate(Opportunity targetOpportunity, Opportunity preOpportunity = null)
        {
            GlobalContext.LogEntry();

            var mergedOpportunity = preOpportunity == null ? targetOpportunity : targetOpportunity.Merge(preOpportunity);
            if ((targetOpportunity.OwnerId != null
                    && targetOpportunity.OwnerId.LogicalName == SystemUser.EntityLogicalName)
                || (targetOpportunity.Contains(Opportunity.Fields.StateCode)
                    && targetOpportunity.StateCode == OpportunityState.Won
                    && mergedOpportunity.OwnerId.LogicalName == SystemUser.EntityLogicalName))
            {
                RepresentativeRewardBL representativeRewardBL = new RepresentativeRewardBL(this.GlobalContext);
                representativeRewardBL.CreateRepresentativeReward(mergedOpportunity.ToEntity<Entity>());
            }
        }

        public void HandleDuplicateOpportunityCheck(Opportunity targetOpportunity, Opportunity preOpportunity = null)
        {
            GlobalContext.LogEntry();
            if (targetOpportunity.AttributeHasValue<string>(Opportunity.Fields.alt_MobilePhone))
            {
                Opportunity mergedOpportunity = preOpportunity != null ? targetOpportunity.Merge(preOpportunity) : targetOpportunity;

                Entity opportunityToSearch = new Entity(Lead.EntityLogicalName);
                opportunityToSearch[Lead.Fields.MobilePhone] = mergedOpportunity.alt_MobilePhone;

                CommonDAL commonDAL = new CommonDAL(GlobalContext, string.Empty);
                RetrieveDuplicatesResponse duplicateResponse = commonDAL.ExecuteDuplicateDetectionRequest(opportunityToSearch, Opportunity.EntityLogicalName);
                DataCollection<Entity> duplicateEntities = duplicateResponse.DuplicateCollection.Entities;

                bool opportunityWithSameLeadExists = duplicateEntities.Any(o =>
                        o.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId)
                            && ((EntityReference)o[Opportunity.Fields.OriginatingLeadId]).Id == mergedOpportunity.OriginatingLeadId.Id);

                GlobalContext.Log.Info($"Opportunities With Same MobilePhone Exists :{opportunityWithSameLeadExists}");

                bool isMobileDuplicate = duplicateEntities.Count > 0;
                bool isEmptyLead = !mergedOpportunity.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId);

                bool isDifferentLead = !isEmptyLead && !duplicateEntities.Any(o =>
                        o.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId)
                            && ((EntityReference)o[Opportunity.Fields.OriginatingLeadId]).Id == mergedOpportunity.OriginatingLeadId.Id);

                List<Entity> duplicateOpportunities = duplicateEntities.Where(o =>
                                                !o.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId)
                                                || o.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId)
                                                && ((EntityReference)o[Opportunity.Fields.OriginatingLeadId]).Id != mergedOpportunity.OriginatingLeadId.Id)
                                                .ToList();

                if (isMobileDuplicate && duplicateOpportunities.Count > 0)
                {
                    CommonBL commonBL = new CommonBL(GlobalContext);
                    foreach (var duplicaterOpportunity in duplicateOpportunities)
                    {
                        commonBL.SendAppNotificationForDuplicateLeadOrOpportunity(duplicaterOpportunity);
                    }
                }

                //if (isMobileDuplicate && (isEmptyLead || isDifferentLead))
                //{
                //    CommonBL commonBL = new CommonBL(GlobalContext);
                //    commonBL.SendAppNotificationForDuplicateLeadOrOpportunity(duplicateEntities.First());
                //}
            }
        }

        public void ResetOportunityOperation(Opportunity targetOpportunity, Opportunity preOpportunity)
        {
            this.GlobalContext.LogEntry();

            if (targetOpportunity.Contains(Opportunity.Fields.StateCode)
                && targetOpportunity.StateCode == OpportunityState.Open
                && preOpportunity.StateCode != OpportunityState.Open
                && preOpportunity.alt_OpportunityOperationCode != null)
            {
                targetOpportunity.alt_OpportunityOperationCode = null;
            }
        }

        public void HandleCloseOpportunity(Opportunity targetOpportunity, Opportunity postOpportunity)
        {
            this.GlobalContext.LogEntry();
            if (targetOpportunity.AttributeHasValue<OptionSetValue>(Opportunity.Fields.alt_OpportunityOperationCode)
                && postOpportunity.OriginatingLeadId != null)
            {
                this.GlobalContext.LogEntry();
                OpportunityOperationCode opportunityOperationCode = (OpportunityOperationCode)targetOpportunity.alt_OpportunityOperationCode.Value;
                if (OpportunityHasActiveTradeDigitalForm(postOpportunity.OriginatingLeadId.Id))
                {
                    int errorCode = opportunityOperationCode == OpportunityOperationCode.Win ?
                        CustomErrorCodes.InvalidCloseAsWonOpportunity : CustomErrorCodes.InvalidCloseAsLostOpportunity;
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, errorCode, CustomErrorCodes.GetErrorMessage(errorCode));
                }
                else
                {
                    OpportunityDAL opportunityDal = new OpportunityDAL(this.GlobalContext);
                    switch (opportunityOperationCode)
                    {
                        case OpportunityOperationCode.Win:
                            {
                                targetOpportunity.StatusCode = new OptionSetValue((int)OpportunityStatusCode.Winning);
                                opportunityDal.CloseOpportunityAsWon(targetOpportunity);
                                break;
                            }
                        case OpportunityOperationCode.Lost:
                            {
                                targetOpportunity.StatusCode = new OptionSetValue((int)OpportunityStatusCode.Canceld);
                                opportunityDal.CloseOpportunityAsLost(targetOpportunity);
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        public void SetOwner(Opportunity targetOpportunity)
        {
            this.GlobalContext.LogEntry();
            if (targetOpportunity.AttributeHasValue<EntityReference>(Opportunity.Fields.OriginatingLeadId))
            {
                targetOpportunity.OwnerId = new LeadDAL(GlobalContext).Get(targetOpportunity.OriginatingLeadId.Id, new string[] { Lead.Fields.OwnerId }).OwnerId;
            }
            //TODO: is this relevant?
            else if (!OpportunityHasActiveTradeDigitalForm(targetOpportunity.OriginatingLeadId.Id)
                && (targetOpportunity.OwnerId == null
                || targetOpportunity.OwnerId.Id != GlobalContext.UserId))
            {
                targetOpportunity.OwnerId = new EntityReference(SystemUser.EntityLogicalName, GlobalContext.UserId);
            }
        }

        public void CloseOpportunity(EntityReference opportunity, OpportunityState opportunityState)
        {
            this.GlobalContext.LogEntry();
            if (opportunity != null)
            {
                OpportunityDAL opportunityDal = new OpportunityDAL(this.GlobalContext);
                var retrievedOpportunity = opportunityDal.Get(opportunity.Id, new string[] { Opportunity.Fields.StateCode });
                if (retrievedOpportunity.StateCode == OpportunityState.Open)
                {
                    switch (opportunityState)
                    {
                        case OpportunityState.Won:
                            {
                                opportunityDal.CloseOpportunityAsWon(opportunity, new OptionSetValue((int)OpportunityStatusCode.Winning));
                                break;
                            }

                        case OpportunityState.Lost:
                            {
                                opportunityDal.CloseOpportunityAsLost(opportunity, new OptionSetValue((int)OpportunityStatusCode.Canceld));
                                break;
                            }

                        default:
                            break;
                    }

                }
                else
                {
                    this.GlobalContext.Log.Warning(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.OpportunityAlreadyClosedError));
                }
            }
        }

        private bool OpportunityHasActiveTradeDigitalForm(Guid regardingId)
        {
            this.GlobalContext.LogEntry();
            bool activeDigitalJoining = false;

            var digitalFormdDal = new DigitalFormDAL(this.GlobalContext);
            List<alt_DigitalForm> relatedDigitalForms = digitalFormdDal.GetDigitalFormsByRegardingObject(regardingId);

            List<alt_DigitalForm> activeTradeJoiningDigitalForm = relatedDigitalForms
                ?.Where(d => d.StateCode.Value != alt_DigitalFormState.Canceled
                        && !string.IsNullOrWhiteSpace(d.alt_DigitalFormLink)
                        && d.alt_DigitalFormTypeCode != null
                        && d.alt_DigitalFormTypeCode.Value == (int)DigitalFormTypeCode.TradeJoining
                        )?.ToList();
            activeDigitalJoining = activeTradeJoiningDigitalForm != null && activeTradeJoiningDigitalForm.Count > 0;
            return activeDigitalJoining;
        }

        private string GetCustomerName(Opportunity mergedOpportunity)
        {
            this.GlobalContext.LogEntry();
            string customerName;
            EntityReference customerEntityReference = mergedOpportunity.CustomerId;

            if (customerEntityReference.LogicalName == Contact.EntityLogicalName)
            {
                ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                customerName = contactDal.GetPrimeryAttributeValue(customerEntityReference, Contact.Fields.FullName);
            }
            else
            {
                AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                customerName = accountDal.GetPrimeryAttributeValue(customerEntityReference, Account.Fields.Name);
            }
            return customerName;
        }

        public void HandleCloseRelatedActivitiesOnOpportunityClosed(Opportunity targetOpportunity)
        {
            this.GlobalContext.LogEntry();

            if (targetOpportunity.Contains(Opportunity.Fields.StateCode)
                && (targetOpportunity.StateCode == OpportunityState.Won
                    || targetOpportunity.StateCode == OpportunityState.Lost))
            {
                ActivityBL activityBL = new ActivityBL(this.GlobalContext);
                activityBL.CloseActivitiesOnRegardingObjectStateCode(targetOpportunity, (int)targetOpportunity.StateCode.Value);
            }
        }

        public void LinkOpportunityToDigitalFormVerification(Opportunity targetOpportunity)
        {
            this.GlobalContext.LogEntry();

            if (targetOpportunity.AttributeHasValue<string>(Opportunity.Fields.alt_OpportunityIdentityNumber))
            {
                DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);

                alt_DigitalFormVerification retrievedDigitalFormVerification =
                    digitalFormVerificationDAL.GetByDigitalFormNumberWithNoOpportunity(targetOpportunity.alt_OpportunityIdentityNumber);

                if (retrievedDigitalFormVerification != null)
                {
                    alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification
                    {
                        Id = retrievedDigitalFormVerification.Id,
                        alt_OpportunityId = targetOpportunity.ToEntityReference()
                    };
                    digitalFormVerificationDAL.Update(digitalFormVerificationToUpdate);
                }
            }
        }
    }
}
