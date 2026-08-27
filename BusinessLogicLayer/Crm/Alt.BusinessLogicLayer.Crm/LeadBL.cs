using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class LeadBL : CrmBaseBL
    {
        bool isCreateAccount = false;
        bool isCreateContact = false;

        string DefaultTreatmentStatusCodeGlobalParameterName = "DefaultTreatmentStatusCode";
        string defaultOwnerTeamCodeGlobalParameterName = "DefaultOwnerTeamCode";
        string assignedToRepresentitiveTreatmentStatusCodeGlobalParameterName = "AssignedToRepresentitiveTreatmentStatusCode";

        public LeadBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleAssignToMe(Lead targetLead, Guid initiatingUserId)
        {
            GlobalContext.LogEntry();

            if (targetLead.AttributeHasValue<bool>(Lead.Fields.alt_AssignToMeBit)
                && targetLead.alt_AssignToMeBit == true)
            {
                targetLead.alt_AssignToMeBit = false;
                var assignRequest = new AssignRequest
                {
                    Target = targetLead.ToEntityReference(),
                    Assignee = new EntityReference(SystemUser.EntityLogicalName, initiatingUserId)
                };

                this.GlobalContext.OrganizationService.Execute(assignRequest);
            }
        }

        public void HandleSystemAsyncCreateLead(Lead targetLead)
        {
            GlobalContext.LogEntry();

            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            var isDisqualifyDuplicatedLeads = this.GlobalContext.CacheManager.GetGlobalParameter<bool>("DisqualifyDuplicatedLeads");
            if (isDisqualifyDuplicatedLeads
                && leadDal.IsNeedToDisqualify(targetLead))
            {
                leadDal.DisqualifyLead(targetLead, LeadStatusCode.DoubleLead);
            }
            else
            {
                this.HandleRepresentativeRewardCreate(targetLead);
                this.HandleDuplicateLeadCheck(targetLead);
            }
        }

        public void HandleTreatmentStatusByAssignee(Lead targetLead, Lead preLead)
        {
            this.GlobalContext.LogEntry();

            var defaultOwnerTeamCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>(defaultOwnerTeamCodeGlobalParameterName);
            var assignedToRepresentitiveTreatmentStatusCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>(assignedToRepresentitiveTreatmentStatusCodeGlobalParameterName);

            if (preLead.AttributeHasValue<EntityReference>(Lead.Fields.OwnerId)
                && preLead.OwnerId.LogicalName == Team.EntityLogicalName
                && targetLead.AttributeHasValue<EntityReference>(Lead.Fields.OwnerId)
                && targetLead.OwnerId.LogicalName == SystemUser.EntityLogicalName)
            {
                TeamDAL teamDAL = new TeamDAL(this.GlobalContext);
                Team preOwner = teamDAL.Get(preLead.OwnerId.Id, new string[] { Team.Fields.alt_TeamCodeInt });

                this.GlobalContext.Log.Info("pre owner: " + preOwner.Id);
                this.GlobalContext.Log.Info("pre owner code: " + preOwner.alt_TeamCodeInt);
                this.GlobalContext.Log.Info("1: " + (preOwner != null));
                this.GlobalContext.Log.Info("2: " + (preOwner.alt_TeamCodeInt == defaultOwnerTeamCode));

                if (preOwner != null && preOwner.alt_TeamCodeInt == defaultOwnerTeamCode)
                {
                    this.GlobalContext.LogEntry();

                    TreatmentStatusDAL treatmentStatusDAL = new TreatmentStatusDAL(this.GlobalContext);
                    alt_TreatmentStatus retrievedTreatmentStatus = treatmentStatusDAL.GetByCode(assignedToRepresentitiveTreatmentStatusCode);
                    targetLead.alt_TreatmentStatusId = retrievedTreatmentStatus.ToEntityReference();
                }
            }
        }

        public void HandleJoiningProcessSummary(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            var defaultOwnerTeamCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>("DefaultOwnerTeamCode");
            if (targetLead.AttributeHasValue<string>(Lead.Fields.alt_LeadIdentityNumber))
            {
                alt_JoiningProcessSummary joiningProcessSummaryToCreeate = new alt_JoiningProcessSummary
                {
                    alt_Name = "תהליך הצטרפות - " + targetLead.alt_LeadIdentityNumber,
                    alt_LeadId = targetLead.ToEntityReference(),
                    alt_JoiningProcessIdentifier = targetLead.alt_LeadIdentityNumber,
                    OwnerId = new EntityReference(Team.EntityLogicalName, "alt_teamcodeint", defaultOwnerTeamCode)
                };
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_JoiningProcessSummary.EntityLogicalName);
                commonDAL.Create(joiningProcessSummaryToCreeate);
            }
        }


        public void HandleRepresentativeRewardCreate(Lead targetLead, Lead preLead = null)
        {
            this.GlobalContext.LogEntry();

            var mergedLead = preLead == null ? targetLead : targetLead.Merge(preLead);
            if ((targetLead.OwnerId != null
                    && targetLead.OwnerId.LogicalName == SystemUser.EntityLogicalName)
                || (targetLead.Contains(Opportunity.Fields.StateCode)
                    && targetLead.StateCode == LeadState.Qualified
                    && mergedLead.OwnerId.LogicalName == SystemUser.EntityLogicalName))
            {
                RepresentativeRewardBL representativeRewardBL = new RepresentativeRewardBL(this.GlobalContext);
                representativeRewardBL.CreateRepresentativeReward(mergedLead.ToEntity<Entity>());
            }
        }

        public void SetDefaultTreatmentStatus(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            if (!targetLead.Contains(alt_TreatmentStatus.Fields.alt_TreatmentStatusId))
            {
                int code = this.GlobalContext.CacheManager.GetGlobalParameter<int>(DefaultTreatmentStatusCodeGlobalParameterName);
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_TreatmentStatus.EntityLogicalName);
                var treatmentStatus = commonDAL.GetActiveByAttribute(alt_TreatmentStatus.Fields.alt_CodeInt, code, new string[] { alt_TreatmentStatus.Fields.alt_TreatmentStatusId }).FirstOrDefault();
                targetLead.alt_TreatmentStatusId = treatmentStatus.ToEntityReference();
            }
        }

        public void SetRefferingCustomerCalculatedAccountNumber(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.AttributeHasValue<string>(Lead.Fields.alt_RefferingCustomerAccountNumber)
                && long.TryParse(targetLead.alt_RefferingCustomerAccountNumber, out long result)
                && result != 0)
            {
                targetLead.alt_RefferingCustomerCalculatedAccountNumber = (result / 2).ToString();
            }
        }

        public void HandleIdentityNumber(Lead targetLead, Lead preLead = null)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.Contains(Lead.Fields.alt_IdentityNumber))
            {
                Lead mergedLead = preLead == null || targetLead.Equals(preLead) ?
                    targetLead : targetLead.Merge(preLead);
                IdentityTypeCode identityTypeCode = (IdentityTypeCode)mergedLead.alt_IdentityTypeCode.Value;
                if (!string.IsNullOrWhiteSpace(targetLead.alt_IdentityNumber))
                {
                    switch (identityTypeCode)
                    {
                        case IdentityTypeCode.GovernmentId:
                            {

                                ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                                targetLead.ParentContactId = contactDal.GetByGovernmentId(targetLead.alt_IdentityNumber)?.ToEntityReference();
                                targetLead.ParentAccountId = null;
                                break;
                            }

                        case IdentityTypeCode.AccountNumber:
                            {
                                AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                                targetLead.ParentAccountId = accountDal.GetByAccountNumber(targetLead.alt_IdentityNumber)?.ToEntityReference();
                                targetLead.ParentContactId = null;
                                break;
                            }

                        default:
                            break;
                    }
                }
                else
                {
                    targetLead.ParentContactId = null;
                    targetLead.ParentAccountId = null;
                }
            }
        }

        public void HandleDuplicateLeadCheck(Lead targetLead, Lead preLead = null)
        {
            GlobalContext.LogEntry();
            if (targetLead.AttributeHasValue<string>(Lead.Fields.MobilePhone))
            {
                Lead mergedLead = preLead != null ? targetLead.Merge(preLead) : targetLead;
                Entity leadToSearch = new Entity(Lead.EntityLogicalName);
                leadToSearch[Lead.Fields.MobilePhone] = mergedLead.MobilePhone;

                CommonDAL commonDAL = new CommonDAL(GlobalContext, string.Empty);
                //bool isDuplicate = commonDAL.ExecuteDuplicateDetectionRequest(leadToSearch).DuplicateCollection.Entities.Count > 1;
                DataCollection<Entity> duplicateLeads = commonDAL.ExecuteDuplicateDetectionRequest(leadToSearch).DuplicateCollection.Entities;
                if (duplicateLeads.Count > 1)
                {
                    CommonBL commonBL = new CommonBL(GlobalContext);
                    foreach (var duplicateLead in duplicateLeads)
                    {
                        if (duplicateLead.Id != mergedLead.Id)
                        {
                            commonBL.SendAppNotificationForDuplicateLeadOrOpportunity(duplicateLead);
                        }
                    }
                }

                Opportunity opportunityToCheck = new Opportunity()
                {
                    OriginatingLeadId = targetLead.ToEntityReference(),
                    alt_MobilePhone = mergedLead.MobilePhone
                };

                OpportunityBL opportunityBL = new OpportunityBL(GlobalContext);
                opportunityBL.HandleDuplicateOpportunityCheck(opportunityToCheck);
            }
        }

        public void SetReferralSource(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.AttributeHasValue<string>(Lead.Fields.alt_MarketingSource))
            {
                SetReferralSourceByMarktingSource(targetLead);
            }
            else
            {
                SetReferralSourceByLeadSource(targetLead);
            }
        }

        private void SetReferralSourceByLeadSource(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.LeadSourceCode?.Value == (int)LeadSourceCode.MiniSite
                 || targetLead.LeadSourceCode?.Value == (int)LeadSourceCode.HouseMarketingSite)
            {
                int withoutCooperationReferralSourceCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>("WithoutCooperationReferralSource");
                ReferralSourceDAL referralSourceDAL = new ReferralSourceDAL(this.GlobalContext);
                alt_ReferralSource withoutCooperationReferralSource = referralSourceDAL.GetFirstOrDefaultByAttribute(
                    alt_ReferralSource.Fields.alt_CodeInt,
                    withoutCooperationReferralSourceCode,
                    new string[] { alt_ReferralSource.Fields.alt_CodeInt });

                targetLead.alt_ReferralSourceId = withoutCooperationReferralSource.ToEntityReference();
            }
        }

        private void SetReferralSourceByMarktingSource(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            CommonDAL commonDal = new CommonDAL(this.GlobalContext, alt_ReferralSource.EntityLogicalName);
            var retrievedReferralSource = commonDal.GetActiveByAttribute(alt_ReferralSource.Fields.alt_MarketingSource, targetLead.alt_MarketingSource.Trim(), new string[] { alt_ReferralSource.Fields.Id })
                .FirstOrDefault();
            if (retrievedReferralSource != null)
            {
                targetLead.alt_ReferralSourceId = retrievedReferralSource.ToEntityReference();
            }
        }

        public void HandlePreQualifyLead(ParameterCollection inputParameters, IPluginExecutionContext parentContext)
        {
            this.GlobalContext.LogEntry();
            var qualifiedLead = inputParameters["LeadId"] as EntityReference;

            if (qualifiedLead != null)
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                var retrievedLead = leadDal.Get(qualifiedLead.Id, new[]
                {
                    Lead.Fields.alt_IdentityNumber,
                    Lead.Fields.ParentContactId,
                    Lead.Fields.ParentAccountId,
                    Lead.Fields.alt_IdentityTypeCode,
                    Lead.Fields.LeadSourceCode
                });
                if (this.IsValidForQualifyLead(retrievedLead, parentContext))
                {
                    if (!retrievedLead.AttributeHasValue<string>(Lead.Fields.alt_IdentityNumber))
                    {
                        throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.LeadWithoutCustomerIdentityError, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.LeadWithoutCustomerIdentityError));
                    }
                    else if (retrievedLead.ParentContactId == null && retrievedLead.ParentAccountId == null)
                    {
                        this.HandleParentCustomer(retrievedLead);
                    }
                    this.GlobalContext.Log.Info($"CreateAccount: ({isCreateAccount})");
                    this.GlobalContext.Log.Info($"CreateContact: ({isCreateContact})");
                    inputParameters["CreateAccount"] = isCreateAccount;
                    inputParameters["CreateContact"] = isCreateContact;
                }
                else
                {
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.InvalidTraidLeadForQualify, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidTraidLeadForQualify));
                }
            }
        }

        public void HandleLeadStateCode(Lead targetLead)
        {
            this.GlobalContext.LogEntry();
            if (targetLead.Contains(Lead.Fields.StatusCode)
                && !targetLead.Contains(Lead.Fields.StateCode))
            {
                LeadStatusCode leadStatusCode = (LeadStatusCode)targetLead.StatusCode.Value;
                switch (leadStatusCode)
                {
                    case LeadStatusCode.New:
                    case LeadStatusCode.InProgress:
                        {
                            targetLead.StateCode = LeadState.Open;
                            break;
                        }
                    case LeadStatusCode.Other:
                    case LeadStatusCode.NoResponseMultipleAttempts:
                    case LeadStatusCode.RequestedBenefitsNotApproved:
                    case LeadStatusCode.CanceledAutomaticallyInOutSystem:
                    case LeadStatusCode.DoubleLead:
                    case LeadStatusCode.RepresentativeInitiativeAffiliateAccount:
                    case LeadStatusCode.RepresentativeInitiativeCorporateAccount:
                    case LeadStatusCode.NotInterestedInOpeningAnAccount:
                    case LeadStatusCode.DoesNotMeetMinimumDeposit:
                    case LeadStatusCode.ExistingCustomerTransferredToCustomerRelations:
                    case LeadStatusCode.InterestedInManagedPortfolio:
                    case LeadStatusCode.UnderAgeEighteen:
                    case LeadStatusCode.InformationProvidedAndCurrentlyNotRelevant:
                    case LeadStatusCode.OpenedByMistake:
                    case LeadStatusCode.OpenedAccountWithAnotherStockExchangeMember:
                    case LeadStatusCode.UnitedStateResident:
                    case LeadStatusCode.Foreigner:
                    case LeadStatusCode.Disqualified:
                    case LeadStatusCode.RepresentativeInitiativeUnder18:
                    case LeadStatusCode.RepresentativeInitiativeForeignCountryTaxResidency:
                    case LeadStatusCode.RepresentativeInitiativeKosherPhone:
                    case LeadStatusCode.RepresentativeInitiativeUSPerson:
                    case LeadStatusCode.RepresentativeInitiativeNoValidIdentification:
                    case LeadStatusCode.RepresentativeInitiativeInvalidPhoneNumber:
                    case LeadStatusCode.RepresentativeInitiativeNotGettingAlongWithTheProcess:
                    case LeadStatusCode.CorporationAccount:
                    case LeadStatusCode.AffiliateAccount:
                        {
                            targetLead.StateCode = LeadState.Disqualified;
                            break;
                        }

                    default:
                        break;
                }
            }
        }

        public void HandleDisqualifyLead(Lead targetLead, IPluginExecutionContext parentContext)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.Contains(Lead.Fields.StatusCode)
                && targetLead.Contains(Lead.Fields.StateCode)
                && targetLead.StateCode == LeadState.Disqualified
                && !IsValidForDisqualifyLead(targetLead, parentContext))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.InvalidTraidLeadForDisqualify, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidTraidLeadForDisqualify));

            }
        }

        public void HandleUpdateDigitalForm(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.Contains(Lead.Fields.alt_IdentityNumber)
                && !string.IsNullOrWhiteSpace(targetLead.alt_IdentityNumber))
            {
                DigitalFormDAL digitalFormdDal = new DigitalFormDAL(this.GlobalContext);
                var relatedActiveDigitalForm = digitalFormdDal.GetByAttribute(alt_DigitalForm.Fields.RegardingObjectId, targetLead.Id, new string[] { alt_DigitalForm.Fields.alt_CustomerIdentityNumber })?.FirstOrDefault();
                if (relatedActiveDigitalForm != null && relatedActiveDigitalForm.alt_CustomerIdentityNumber != targetLead.alt_IdentityNumber)
                {
                    digitalFormdDal.Update(new alt_DigitalForm()
                    {
                        Id = relatedActiveDigitalForm.Id,
                        alt_CustomerIdentityNumber = targetLead.alt_IdentityNumber
                    });
                }
            }
        }

        private bool IsValidForQualifyLead(Lead lead, IPluginExecutionContext parentContext)
        {
            this.GlobalContext.LogEntry();
            bool isValid = true;

            if (this.LeadHasActiveTradeDigitalForm(lead) && parentContext == null)
            {
                isValid = false;
            }

            return isValid;
        }

        private bool IsValidForDisqualifyLead(Lead lead, IPluginExecutionContext parentContext)
        {
            this.GlobalContext.LogEntry();
            bool isValid = true;

            if (this.LeadHasActiveTradeDigitalForm(lead))
            {
                if (parentContext == null
                    || (parentContext.InputParameters.Contains("Target")
                        && ((Entity)parentContext.InputParameters["Target"]).LogicalName == Lead.EntityLogicalName))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool LeadHasActiveTradeDigitalForm(Lead lead)
        {
            this.GlobalContext.LogEntry();

            DigitalFormDAL digitalFormdDal = new DigitalFormDAL(this.GlobalContext);
            var relatedDigitalForms = digitalFormdDal.GetDigitalFormsByRegardingObject(lead.Id);

            List<alt_DigitalForm> activeTradeJoiningDigitalForm = relatedDigitalForms?.Where(d => d.StateCode.Value != alt_DigitalFormState.Canceled
                                                 && (!string.IsNullOrWhiteSpace(d.alt_DigitalFormLink)
                                                 || (d.alt_TransferToOutSystemStatusCode != null
                                                    && d.alt_TransferToOutSystemStatusCode.Value == (int)TransferStatusCode.Sending))
                                                 && d.alt_DigitalFormTypeCode != null
                                                 && d.alt_DigitalFormTypeCode.Value == (int)DigitalFormTypeCode.TradeJoining)?.ToList();

            return activeTradeJoiningDigitalForm != null && activeTradeJoiningDigitalForm.Count > 0;
        }

        public void HandleParentCustomer(Lead targetLead)
        {
            this.GlobalContext.LogEntry();
            Lead leadToUpdate = new Lead { Id = targetLead.Id };
            IdentityTypeCode identityTypeCode = (IdentityTypeCode)targetLead.alt_IdentityTypeCode.Value;
            switch (identityTypeCode)
            {
                case IdentityTypeCode.GovernmentId:
                    {
                        ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                        Contact retrievedContact = contactDal.GetByGovernmentId(targetLead.alt_IdentityNumber);
                        isCreateContact = retrievedContact == null;
                        leadToUpdate.ParentContactId = retrievedContact?.ToEntityReference();
                        break;
                    }

                case IdentityTypeCode.AccountNumber:
                    {
                        AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                        Account retrievedAccount = accountDal.GetByAccountNumber(targetLead.alt_IdentityNumber);
                        isCreateAccount = retrievedAccount == null;
                        leadToUpdate.ParentAccountId = retrievedAccount?.ToEntityReference();
                        break;
                    }

                default:
                    break;
            }
            if (leadToUpdate.ParentAccountId != null || leadToUpdate.ParentContactId != null)
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                leadDal.Update(leadToUpdate);
            }
        }

        public void HandleLeadSourceCode(Lead targetLead, Lead preLead)
        {
            this.GlobalContext.LogEntry();
            if (targetLead.LeadSourceCode != null
                && targetLead.LeadSourceCode.Value == (int)LeadSourceCode.DigitalForm)
            {
                targetLead.LeadSourceCode = preLead.LeadSourceCode;
            }
        }

        public void HandleClosedOnDate(Lead targetLead, Lead preLead)
        {
            if (preLead.alt_ClosedOnDate == null
                && targetLead.Contains(Lead.Fields.StateCode)
                && targetLead.StateCode != LeadState.Open)
            {
                targetLead.alt_ClosedOnDate = DateTime.UtcNow;
            }
        }

        public void HandleCloseRelatedActivitiesOnLeadClosed(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            if (targetLead.Contains(Lead.Fields.StateCode)
                && (targetLead.StateCode == LeadState.Qualified
                    || targetLead.StateCode == LeadState.Disqualified))
            {
                ActivityBL activityBL = new ActivityBL(this.GlobalContext);
                activityBL.CloseActivitiesOnRegardingObjectStateCode(targetLead, (int)targetLead.StateCode.Value);
            }
        }
    }
}
