using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Alt.DataModel.Crm.Core.Errors;

namespace Alt.BusinessLogicLayer.Crm
{
    public class DigitalFormVerificationBL : CrmBaseBL
    {
        const string fundsTransferActionMotivationProcessName = "FundsTransferActionMotivation";
        public DigitalFormVerificationBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetLeadValuesByLinkedDigitalForm(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_DigitalFormId))
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                Lead retrievedLead = leadDal.GetFirstOrDefautlLeadByDigitalForm(targetDigitalFormVerification.alt_DigitalFormId.Id);

                if (retrievedLead != null)
                {
                    targetDigitalFormVerification.alt_OpportunityId = retrievedLead.QualifyingOpportunityId;
                    targetDigitalFormVerification.alt_DigitalFormNumber = retrievedLead.alt_LeadIdentityNumber;
                    targetDigitalFormVerification.alt_LeadSourceCode = retrievedLead.LeadSourceCode;
                    targetDigitalFormVerification.alt_UTMMarketingSource = retrievedLead.alt_MarketingSource;

                    if (retrievedLead.alt_ReferralSourceId != null)
                    {
                        targetDigitalFormVerification.alt_ReferralSourceId = retrievedLead.alt_ReferralSourceId;
                        this.HandleLoyaltyProgram(targetDigitalFormVerification);
                    }

                }
                else
                {
                    this.GlobalContext.Log.Warning("Digital Form Regarding Lead not Found.");
                }
            }
        }

        private void HandleLoyaltyProgram(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            ReferralSourceDAL referralSourceDal = new ReferralSourceDAL(this.GlobalContext);
            if (!referralSourceDal.IsReferralSourceMivtza(targetDigitalFormVerification.alt_ReferralSourceId))
            {
                CommonDAL commonDal = new CommonDAL(this.GlobalContext, alt_ReferralSource.EntityLogicalName);
                var retrievedRefferalSource = commonDal.Get(targetDigitalFormVerification.alt_ReferralSourceId.Id, new string[] { alt_ReferralSource.Fields.alt_CodeInt });

                int? loyalityProgramCode = null;
                int? refferalSourceCode = retrievedRefferalSource.GetAttributeValue<int?>(alt_ReferralSource.Fields.alt_CodeInt);

                var mappingSettings = GlobalContext.CacheManager.GetGlobalParameter<Dictionary<string, int>>("ReferralSourceIdToLoyaltyProgramIdMappingSettings");
                if (mappingSettings.ContainsKey(refferalSourceCode.Value.ToString())
                    && mappingSettings.TryGetValue(refferalSourceCode.Value.ToString(), out int result))
                {
                    loyalityProgramCode = result;
                }
                //else
                //{
                //    loyalityProgramCode = GlobalContext.CacheManager.GetGlobalParameter<int?>("NotInClubMembershipLoyaltyProgramCode");
                //}

                this.SetLoyalityProgram(targetDigitalFormVerification, loyalityProgramCode);
            }
        }

        private void SetLoyalityProgram(alt_DigitalFormVerification targetDigitalFormVerification, int? loyalityProgramCode)
        {
            this.GlobalContext.LogEntry();

            if (loyalityProgramCode != null)
            {
                var retrievedLoyalityProgram = new CommonDAL(GlobalContext, alt_LoyaltyProgram.EntityLogicalName)
                             .GetFirstOrDefaultByAttribute(alt_LoyaltyProgram.Fields.alt_CodeInt, loyalityProgramCode.Value, new string[] { alt_LoyaltyProgram.Fields.alt_CommissionClientTypeId });
                if (retrievedLoyalityProgram != null)
                {
                    targetDigitalFormVerification.alt_LoyaltyProgramId = retrievedLoyalityProgram.ToEntityReference();
                    EntityReference commissionClientType = retrievedLoyalityProgram.GetAttributeValue<EntityReference>(alt_LoyaltyProgram.Fields.alt_CommissionClientTypeId);

                    if (commissionClientType != null)
                    {
                        targetDigitalFormVerification.alt_CommissionClientTypeId = commissionClientType;
                    }
                }
            }
        }

        public void SetEncouragingDepositSystemUserByRelatedOpportunityOwner(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_OpportunityId))
            {
                OpportunityDAL opportunityDAL = new OpportunityDAL(GlobalContext);
                EntityReference opportunityOwner = opportunityDAL.Get(targetDigitalFormVerification.alt_OpportunityId.Id, new string[] { Opportunity.Fields.OwnerId }).OwnerId;
                if (opportunityOwner.LogicalName == SystemUser.EntityLogicalName)
                {
                    targetDigitalFormVerification.alt_EncouragingDepositSystemUserId = opportunityOwner;
                }
            }
            else if (!string.IsNullOrWhiteSpace(targetDigitalFormVerification.alt_DigitalFormNumber))
            {
                this.GlobalContext.Log.Info($"retrieve owner from Opportunity by targetDigitalFormVerification.alt_DigitalFormNumber :{targetDigitalFormVerification.alt_DigitalFormNumber}");
                OpportunityDAL opportunityDAL = new OpportunityDAL(GlobalContext);
                EntityReference ownerToUpdate = opportunityDAL.GetActiveByAttribute<string>(
                                                Opportunity.Fields.alt_OpportunityIdentityNumber,
                                                targetDigitalFormVerification.alt_DigitalFormNumber,
                                                new string[] { Opportunity.Fields.OwnerId })?.FirstOrDefault()?.OwnerId;

                if (ownerToUpdate == null)
                {
                    this.GlobalContext.Log.Info("retrieve owner from Lead");
                    LeadDAL leadDAL = new LeadDAL(GlobalContext);
                    ownerToUpdate = leadDAL.GetActiveByAttribute<string>(
                        Lead.Fields.alt_LeadIdentityNumber,
                        targetDigitalFormVerification.alt_DigitalFormNumber,
                        new string[] { Lead.Fields.OwnerId })?.FirstOrDefault()?.OwnerId;
                }

                string ownerToUpdateLogMessage = ownerToUpdate != null ? $"{{LogicalName:{ownerToUpdate.LogicalName}, Id: {ownerToUpdate.Id}}}" : "empty";
                this.GlobalContext.Log.Info($"ownerToUpdate result = {ownerToUpdateLogMessage}");

                if (ownerToUpdate != null && ownerToUpdate.LogicalName == SystemUser.EntityLogicalName)
                {
                    targetDigitalFormVerification.alt_EncouragingDepositSystemUserId = ownerToUpdate;
                }
            }
        }

        public void SetCommissionClientType(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_LoyaltyProgramId)
                && !targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_CommissionClientTypeId))
            {
                CommonDAL commonDal = new CommonDAL(this.GlobalContext, alt_LoyaltyProgram.EntityLogicalName);
                var retrievedLoyaltyProgram = commonDal.Get(targetDigitalFormVerification.alt_LoyaltyProgramId.Id,
                    new string[] { alt_LoyaltyProgram.Fields.alt_CommissionClientTypeId });

                targetDigitalFormVerification.alt_CommissionClientTypeId = retrievedLoyaltyProgram.GetAttributeValue<EntityReference>(alt_LoyaltyProgram.Fields.alt_CommissionClientTypeId);
            }
        }

        public void HandleLinkedPortfolioId(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_PortfolioId))
            {
                targetDigitalFormVerification.alt_FormStatusCode = new OptionSetValue((int)FormStatusCode.OpenedPortfolioInShenhav);
            }
        }

        public void SetTransferToShenhavStatusCode(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if ((targetDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_FormStatusCode)
                    && targetDigitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.SendToOpenPortfolioInShenhav)
                || (targetDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_TransferToShenhavStatusCode)
                    && targetDigitalFormVerification.alt_TransferToShenhavStatusCode.Value == (int)TransferStatusCode.Send))
            {
                targetDigitalFormVerification.alt_TransferToShenhavStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
                this.ClearTransferToShenhavErrorDescription(targetDigitalFormVerification, preDigitalFormVerification);
            }
        }

        public void SetFormStatusCode(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_InitialDepositCode)
                || targetDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_FormStatusCode)
                || targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_ControlStageTeamId))
            {
                alt_DigitalFormVerification mergedDigitalFormVerification = targetDigitalFormVerification.Merge(preDigitalFormVerification);


                ManagerControlChangeTrackingBL managerControlChangeTrackingBL =new ManagerControlChangeTrackingBL(this.GlobalContext);
                bool hasRelevantFieldsChanged = managerControlChangeTrackingBL.HasRevalntFieldsChanged( targetDigitalFormVerification, preDigitalFormVerification);
                bool hasLastManagerApprovalDate = mergedDigitalFormVerification.AttributeHasValue<DateTime>(alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate);
                bool hasRelevantChangeAfterManagerApproval = hasRelevantFieldsChanged && hasLastManagerApprovalDate;

                if (mergedDigitalFormVerification.alt_InitialDepositCode?.Value == (int)InitialDepositCode.AcceptedDepositForApproval)
                {
                    if (targetDigitalFormVerification.alt_FormStatusCode == null
                        || targetDigitalFormVerification.alt_FormStatusCode.Value != (int)FormStatusCode.Canceled)
                    {
                        targetDigitalFormVerification.alt_FormStatusCode = new OptionSetValue((int)FormStatusCode.InAuthorizationProcess);
                    }
                }
                else if (!hasRelevantChangeAfterManagerApproval
               && mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_InitialDepositCode)
               && mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_FormStatusCode)
               && mergedDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_ControlStageTeamId))
                {
                    TeamDAL teamDAL = new TeamDAL(this.GlobalContext);
                    Dictionary<string, int> teamsCodesParameter = JsonSerializer.Deserialize<Dictionary<string, int>>(GlobalContext.CacheManager.GetGlobalParameter<string>("TeamsCodes"));
                    int? controlStageTeamCode = teamDAL.Get(mergedDigitalFormVerification.alt_ControlStageTeamId.Id, new string[] { Team.Fields.alt_TeamCodeInt }).alt_TeamCodeInt;
                    if (teamsCodesParameter.FirstOrDefault(x => x.Value == controlStageTeamCode).Key == "OperationalControl")
                    {
                        if (mergedDigitalFormVerification.alt_InitialDepositCode.Value == (int)InitialDepositCode.AwaitinglDeposit
                            && mergedDigitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.InAuthorizationProcess)
                        {
                            targetDigitalFormVerification.alt_FormStatusCode = new OptionSetValue((int)FormStatusCode.AwaitingForDeposit);
                        }
                        else if (mergedDigitalFormVerification.alt_InitialDepositCode.Value != (int)InitialDepositCode.AwaitinglDeposit
                            && (mergedDigitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.AwaitingForDeposit
                            || mergedDigitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.InAuthorizationProcess))
                        {
                            targetDigitalFormVerification.alt_FormStatusCode = new OptionSetValue((int)FormStatusCode.SendToOpenPortfolioInShenhav);
                        }
                    }
                }
            }
        }

        public void SetManagerVerificationRequiredCode(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification = null)
        {
            this.GlobalContext.LogEntry();
            alt_DigitalFormVerification mergedDigitalFormVerification = preDigitalFormVerification == null ?
                targetDigitalFormVerification : targetDigitalFormVerification.Merge(preDigitalFormVerification);
            if (!mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode)
                || mergedDigitalFormVerification.alt_ManagerVerificationRequiredCode.Value == (int)ManagerVerificationRequiredCode.No)
            {
                if ((mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_CreditRequestExistsCode)
                        && mergedDigitalFormVerification.alt_CreditRequestExistsCode.Value == (int)CreditRequestExistsCode.Yes)
                    || (mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_ShortSaleRequestApprovaIExistsCode)
                        && mergedDigitalFormVerification.alt_ShortSaleRequestApprovaIExistsCode.Value == (int)ShortSaleRequestApprovaIExistsCode.Yes)
                    || !mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_OptionExerciseRequestApprovalExistsCode)
                    || mergedDigitalFormVerification.alt_OptionExerciseRequestApprovalExistsCode.Value != (int)OptionExerciseRequestApprovalExistsCode.No)
                {
                    targetDigitalFormVerification.alt_ManagerVerificationRequiredCode = new OptionSetValue((int)ManagerVerificationRequiredCode.Yes);
                }
            }
        }

        public void SetVerificationReceivedDate(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_FormStatusCode)
                && targetDigitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.AcceptedControl)
            {
                targetDigitalFormVerification.alt_VerificationReceivedDate = DateTime.UtcNow;
            }
        }

        public void HandleFormStatusChanged(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_FormStatusCode))
            {
                FormStatusCode formStatusCode = (FormStatusCode)targetDigitalFormVerification.alt_FormStatusCode.Value;

                switch (formStatusCode)
                {
                    case FormStatusCode.SendToOpenPortfolioInShenhav:
                        {
                            this.InactivateKyc(targetDigitalFormVerification.Id, KYCStatusCode.Ended);
                            this.ApproveLastAuthorizationManagement(targetDigitalFormVerification);
                            break;
                        }
                    case FormStatusCode.AwaitingForDeposit:
                        {
                            this.InactivateKyc(targetDigitalFormVerification.Id, KYCStatusCode.Ended);
                            // this.HandleAutomaticMailing(targetDigitalFormVerification, fundsTransferActionMotivationProcessName);
                            this.HandleAutomaticMailing(targetDigitalFormVerification);
                            break;
                        }
                    case FormStatusCode.Canceled:
                        {
                            this.InactivateKyc(targetDigitalFormVerification.Id, KYCStatusCode.Canceled);
                            this.CancelAccountHolders(targetDigitalFormVerification.Id);
                            this.CloseOpportunity(preDigitalFormVerification.alt_OpportunityId, OpportunityState.Lost);
                            break;
                        }
                    case FormStatusCode.OpenedPortfolioInShenhav:
                        {
                            this.HandleOpenedPortfolioInShenhavStatus(targetDigitalFormVerification, preDigitalFormVerification);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void HandleOpenedPortfolioInShenhavStatus(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_PortfolioId))
            {
                this.LinkAccountHoldersToPortfolio(targetDigitalFormVerification);
                this.LinkKYCToPortfolio(targetDigitalFormVerification);
                this.CloseOpportunity(preDigitalFormVerification.alt_OpportunityId, OpportunityState.Won);
                this.InactivateDigitalFormVerification(targetDigitalFormVerification);
            }
            else
            {
                this.GlobalContext.Log.Warning(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.OpenedInShenhavStatusWithoutPortfolio));
            }
        }

        public void LinkRepresentativeRewardsToPortfolio(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalFormVerification.alt_PortfolioId != null)
            {
                var mergedDigitalFormVerification = targetDigitalFormVerification.Merge(preDigitalFormVerification);
                RepresentativeRewardDAL representativeRewardDAL = new RepresentativeRewardDAL(this.GlobalContext);

                string digitalFormNumber = mergedDigitalFormVerification.alt_DigitalFormNumber;
                var retrievedRepresentativeRewards = representativeRewardDAL.GetActiveByAttribute(
                    alt_RepresentativeReward.Fields.alt_JoiningProcessNumber,
                    digitalFormNumber,
                    new string[] { alt_DigitalFormVerification.Fields.alt_PortfolioId });
                if (retrievedRepresentativeRewards != null && retrievedRepresentativeRewards.Count > 0)
                {
                    foreach (var representativeReward in retrievedRepresentativeRewards)
                    {
                        if (representativeReward.alt_PortfolioId == null
                            || representativeReward.alt_PortfolioId.Id != targetDigitalFormVerification.alt_PortfolioId.Id)
                        {
                            representativeRewardDAL.Update(new alt_RepresentativeReward
                            {
                                Id = representativeReward.Id,
                                alt_PortfolioId = targetDigitalFormVerification.alt_PortfolioId
                            });
                        }
                    }
                }
            }
        }

        public void HandleRepresentativeRewardCreate(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.alt_PortfolioId != null)
            {
                var mergedDigitalFormVerification = preDigitalFormVerification == null ?
                              targetDigitalFormVerification : targetDigitalFormVerification.Merge(preDigitalFormVerification);

                if (mergedDigitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_FormStatusCode)
                    && mergedDigitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.OpenedPortfolioInShenhav
                    && mergedDigitalFormVerification.alt_EncouragingDepositSystemUserId != null)
                {
                    RepresentativeRewardBL representativeRewardBl = new RepresentativeRewardBL(this.GlobalContext);
                    representativeRewardBl.CreateRepresentativeReward(mergedDigitalFormVerification.ToEntity<Entity>(), mergedDigitalFormVerification.alt_EncouragingDepositSystemUserId);
                }
            }
        }

        private void InactivateDigitalFormVerification(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_PortfolioId))
            {
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                digitalFormVerificationDal.Update(new alt_DigitalFormVerification
                {
                    Id = targetDigitalFormVerification.Id,
                    StateCode = alt_DigitalFormVerificationState.Inactive
                });
            }
        }

        private void CloseOpportunity(EntityReference opportunity, OpportunityState opportunityState)
        {
            this.GlobalContext.LogEntry();
            if (opportunity != null)
            {
                OpportunityBL opportunityBl = new OpportunityBL(this.GlobalContext);
                opportunityBl.CloseOpportunity(opportunity, opportunityState);
            }
        }

        private void LinkAccountHoldersToPortfolio(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            List<alt_AccountHolder> accountHolders = accountHolderDal.GetActiveByAttribute(alt_AccountHolder.Fields.alt_DigitalFormVerificationId, targetDigitalFormVerification.Id, new[]
            {
                alt_AccountHolder.Fields.Id,
                alt_AccountHolder.Fields.StatusCode,
                alt_AccountHolder.Fields.alt_PortfolioId
            });
            if (accountHolders.Count > 0)
            {
                foreach (alt_AccountHolder accountHolder in accountHolders)
                {
                    if (!accountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_PortfolioId)
                        && accountHolder.StatusCode.Value == (int)AccountHolderStatusCode.InProcessing)
                    {
                        alt_AccountHolder accountHolderToUpdate = new alt_AccountHolder
                        {
                            Id = accountHolder.Id,
                            StatusCode = new OptionSetValue((int)AccountHolderStatusCode.Active),
                            alt_PortfolioId = targetDigitalFormVerification.alt_PortfolioId
                        };
                        accountHolderDal.Update(accountHolderToUpdate);
                    }
                }
            }
        }


        private void LinkKYCToPortfolio(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            KYCDAL KYCDal = new KYCDAL(this.GlobalContext);
            List<alt_KYC> kycs = KYCDal.GetActiveByAttribute(alt_KYC.Fields.alt_DigitalFormVerificationId, targetDigitalFormVerification.Id, new[] { alt_KYC.Fields.Id });
            foreach (alt_KYC KYC in kycs)
            {
                alt_KYC KYCToUpdate = new alt_KYC
                {
                    Id = KYC.Id,
                    alt_PortfolioId = targetDigitalFormVerification.alt_PortfolioId
                };
                KYCDal.Update(KYCToUpdate);
            }
        }

        private void CancelAccountHolders(Guid targetDigitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();

            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            List<alt_AccountHolder> accountHolders = accountHolderDal
                .GetActiveByAttribute(alt_AccountHolder.Fields.alt_DigitalFormVerificationId, targetDigitalFormVerificationId, new[] { alt_AccountHolder.Fields.Id })
                .ToList();
            if (accountHolders.Count > 0)
            {
                foreach (alt_AccountHolder accountHolder in accountHolders)
                {
                    alt_AccountHolder accountHolderToUpdate = new alt_AccountHolder
                    {
                        Id = accountHolder.Id,
                        StatusCode = new OptionSetValue((int)AccountHolderStatusCode.Canceled)
                    };
                    accountHolderDal.Update(accountHolderToUpdate);
                }
            }
        }

        private void InactivateKyc(Guid digitalFormVerificationId, KYCStatusCode statusCode)
        {
            this.GlobalContext.LogEntry();

            KYCDAL KYCDal = new KYCDAL(this.GlobalContext);
            List<alt_KYC> retrievedKycs = KYCDal.GetActiveAccountHoldersKYCsByDigitalFormVerificationId(digitalFormVerificationId,
                new[]
                {
                    alt_KYC.Fields.Id,
                    alt_KYC.Fields.StateCode
                });
            foreach (alt_KYC kyc in retrievedKycs)
            {
                if (kyc.StateCode == 0)
                {
                    alt_KYC KYC = new alt_KYC()
                    {
                        alt_KYCId = kyc.Id,
                        StateCode = alt_KYCState.Inactive,
                        StatusCode = new OptionSetValue((int)statusCode),
                    };
                    KYCDal.Update(KYC);
                }
            }
        }

        private void ApproveLastAuthorizationManagement(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            AuthorizationManagementDAL authorizationManagementDal = new AuthorizationManagementDAL(this.GlobalContext);
            alt_AuthorizationManagement retrievedAuthorizationManagement = authorizationManagementDal.GetLastCreatedOnAuthorizationManagementByDigitalFormVerificationId(targetDigitalFormVerification.Id);
            alt_AuthorizationManagement authorizationManagement = new alt_AuthorizationManagement()
            {
                Id = retrievedAuthorizationManagement.Id,
                alt_ControlStageStatusCode = new OptionSetValue((int)ControlStageStatusCode.Approval)
            };
            authorizationManagementDal.Update(authorizationManagement);
        }

        //private void HandleAutomaticMailing(alt_DigitalFormVerification targetDigitalFormVerification, string automaticMailingProcessName)
        //{
        //    this.GlobalContext.LogEntry();

        //    AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
        //    alt_AccountHolder mainAccountHolder = accountHolderDal.GetMainAccountHolderByDigitalFormVerificationId(targetDigitalFormVerification.Id);

        //    if (mainAccountHolder != null)
        //    {
        //        Recipient recipient = new Recipient()
        //        {
        //            CustomerId = mainAccountHolder.alt_CustomerId,
        //            MobilePhone = mainAccountHolder.alt_MobilePhone,
        //            Email = mainAccountHolder.alt_Email
        //        };
        //        CommonBL commonBl = new CommonBL(this.GlobalContext);
        //        commonBl.ExecuteTradeAutomaticMailing(targetDigitalFormVerification.ToEntityReference(), recipient, mainAccountHolder.ToEntityReference(), automaticMailingProcessName);
        //    }
        //}

        private void HandleAutomaticMailing(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            alt_AccountHolder mainAccountHolder = accountHolderDal.GetMainAccountHolderByDigitalFormVerificationId(targetDigitalFormVerification.Id);

            if (mainAccountHolder != null)
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_CustomerOperationRequest.EntityLogicalName);

                alt_CustomerOperationRequest customerOperationRequestToCreate = new alt_CustomerOperationRequest()
                {
                    alt_RelatedRecordId = mainAccountHolder.ToEntityReference(),
                    alt_CustomerOperationTemplateId = new EntityReference(alt_CustomerOperationTemplate.EntityLogicalName, alt_CustomerOperationTemplate.Fields.alt_CodeInt, (int)CustomerOperationTemplateCode.FundsTransferActionMotivation),
                    StatusCode = new OptionSetValue((int)CustomerOperationRequestStatusCode.Send)
                };

                commonDAL.Create(customerOperationRequestToCreate);
            }
        }

        private void ClearTransferToShenhavErrorDescription(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (preDigitalFormVerification.alt_TransferToShenhavErrorDescription != null)
            {
                targetDigitalFormVerification.alt_TransferToShenhavErrorDescription = null;
            }
        }

        public void HandleJoiningProcessSummary(alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalFormVerification.AttributeHasValue<string>(alt_DigitalFormVerification.Fields.alt_DigitalFormNumber))
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_JoiningProcessSummary.EntityLogicalName);
                Entity retrievedJoiningProcessSummary = commonDAL.GetFirstOrDefaultByAttribute<string>(alt_JoiningProcessSummary.Fields.alt_JoiningProcessIdentifier,
                    targetDigitalFormVerification.alt_DigitalFormNumber, new string[] { alt_JoiningProcessSummary.Fields.alt_JoiningProcessSummaryId });

                if (retrievedJoiningProcessSummary != null)
                {
                    UpdateExistingJoiningProcessSummary(retrievedJoiningProcessSummary, commonDAL, targetDigitalFormVerification);
                }
                else
                {
                    CreateNewJoiningProcessSummary(commonDAL, targetDigitalFormVerification);
                }
            }
        }

        private void UpdateExistingJoiningProcessSummary(Entity retrievedJoiningProcessSummary, CommonDAL commonDAL, alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            alt_JoiningProcessSummary joiningProcessSummaryToUpdate = new alt_JoiningProcessSummary
            {
                Id = retrievedJoiningProcessSummary.Id,
                alt_DigitalFormVerificationId = targetDigitalFormVerification.ToEntityReference()
            };
            commonDAL.Update(joiningProcessSummaryToUpdate);
        }

        private void CreateNewJoiningProcessSummary(CommonDAL joiningProcessSummaryDAL, alt_DigitalFormVerification targetDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            var defaultOwnerTeamCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>("DefaultOwnerTeamCode");
            alt_JoiningProcessSummary joiningProcessSummaryToCreate = new alt_JoiningProcessSummary
            {
                alt_Name = "תהליך הצטרפות - " + targetDigitalFormVerification.alt_DigitalFormNumber,
                alt_DigitalFormVerificationId = targetDigitalFormVerification.ToEntityReference(),
                alt_JoiningProcessIdentifier = targetDigitalFormVerification.alt_DigitalFormNumber,
                OwnerId = new EntityReference(Team.EntityLogicalName, "alt_teamcodeint", defaultOwnerTeamCode)
            };

            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_OpportunityId))
            {
                joiningProcessSummaryToCreate.alt_OpportunityId = targetDigitalFormVerification.alt_OpportunityId;
                OpportunityDAL opportunityDAL = new OpportunityDAL(this.GlobalContext);
                Opportunity retrievedOpportunity = opportunityDAL.Get(targetDigitalFormVerification.alt_OpportunityId.Id,
                    new string[] { Opportunity.Fields.OriginatingLeadId });

                if (retrievedOpportunity != null && retrievedOpportunity.OriginatingLeadId != null)
                {
                    joiningProcessSummaryToCreate.alt_LeadId = retrievedOpportunity.OriginatingLeadId;
                }
            }
            joiningProcessSummaryDAL.Create(joiningProcessSummaryToCreate);
        }

        public void HandleLinkPortfolioToJoiningProcessSummary(alt_DigitalFormVerification targetDigitalFormVerification, alt_DigitalFormVerification preDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalFormVerification.AttributeHasValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_PortfolioId))
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_JoiningProcessSummary.EntityLogicalName);
                Entity retrievedJoiningProcessSummary = commonDAL.GetFirstOrDefaultByAttribute<string>(alt_JoiningProcessSummary.Fields.alt_JoiningProcessIdentifier,
                    preDigitalFormVerification.alt_DigitalFormNumber, new string[] { alt_JoiningProcessSummary.Fields.alt_JoiningProcessSummaryId });

                if (retrievedJoiningProcessSummary != null)
                {
                    alt_JoiningProcessSummary joiningProcessSummaryToUpdate = new alt_JoiningProcessSummary
                    {
                        Id = retrievedJoiningProcessSummary.Id,
                        alt_PortfolioId = targetDigitalFormVerification.alt_PortfolioId
                    };
                    commonDAL.Update(joiningProcessSummaryToUpdate);
                }
            }
        }

        public void ChangeAssigneeWhenAssignedToUser(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();

            var target = inputParameters["Target"] as EntityReference;
            var assignee = inputParameters["Assignee"] as EntityReference;

            if (target != null && assignee != null && assignee.LogicalName == SystemUser.EntityLogicalName)
            {
                DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                alt_DigitalFormVerification retrievedDigitalFormVerification =
                    digitalFormVerificationDAL.Get(target.Id, new[] { alt_DigitalFormVerification.Fields.OwnerId });

                if (retrievedDigitalFormVerification != null
                    && retrievedDigitalFormVerification.AttributeHasValue<EntityReference>(Incident.Fields.OwnerId)
                    && retrievedDigitalFormVerification.OwnerId.LogicalName == Team.EntityLogicalName)
                {
                    alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification()
                    {
                        Id = target.Id,
                        alt_ResponsibleSystemUserId = assignee
                    };

                    digitalFormVerificationDAL.Update(digitalFormVerificationToUpdate);
                    inputParameters["Assignee"] = retrievedDigitalFormVerification.OwnerId;
                }
            }
            else
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CantAssignToTeam, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CantAssignToTeam));
            }
        }
    }
}