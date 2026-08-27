using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class AccountHolderBL : CrmBaseBL
    {
        const string completionDigitalJoiningProcess = "CompletionDigitalJoiningProcess";

        public AccountHolderBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetAccountHolderName(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder = null)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<string>(alt_AccountHolder.Fields.alt_FirstName)
                || targetAccountHolder.AttributeHasValue<string>(alt_AccountHolder.Fields.alt_LastName))
            {
                alt_AccountHolder mergedAccountHolder = preAccountHolder == null ?
                    targetAccountHolder : targetAccountHolder.Merge(preAccountHolder);
                List<string> nameParts = new List<string>();
                if (mergedAccountHolder.AttributeHasValue<string>(alt_AccountHolder.Fields.alt_FirstName))
                {
                    nameParts.Add(mergedAccountHolder.alt_FirstName);
                }
                if (mergedAccountHolder.AttributeHasValue<string>(alt_AccountHolder.Fields.alt_LastName))
                {
                    nameParts.Add(mergedAccountHolder.alt_LastName);
                }
                targetAccountHolder.alt_Name = string.Join(" ", nameParts);
            }
        }

        public void HandleBeneficiarySpouseAccountHolderOnBeneficiaryStateChange(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.StateCode)
                && targetAccountHolder.StateCode != preAccountHolder.StateCode)
            {
                var mergedeAccountHolder = targetAccountHolder.Merge(preAccountHolder);
                if (mergedeAccountHolder.alt_DigitalFormVerificationId != null
                    && mergedeAccountHolder.alt_AccountHolderTypeCode != null
                    && mergedeAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Beneficiary)
                {
                    AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                    var accountHolders = accountHolderDal.GetAllAccountHolderByDigitalFormVerificationId
                        (
                            mergedeAccountHolder.alt_DigitalFormVerificationId.Id,
                            new string[]
                            {
                                alt_AccountHolder.Fields.alt_BeneficiarySpouseAccountHolderId,
                                alt_AccountHolder.Fields.alt_AccountHolderTypeCode,
                                alt_AccountHolder.Fields.CreatedOn,
                                alt_AccountHolder.Fields.StateCode
                            }
                        );
                    var ownerAccountHolders = accountHolders
                        .Where(a => a.StateCode == alt_AccountHolderState.Active
                                && a.alt_AccountHolderTypeCode != null
                                && a.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner);

                    if (ownerAccountHolders?.Count() > 0)
                    {
                        var latestActiveBeneficiariary = accountHolders
                            .Where(a => a.StateCode == alt_AccountHolderState.Active
                                    && a.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Beneficiary)
                            .OrderByDescending(a => a.CreatedOn)
                            .FirstOrDefault();

                        if (targetAccountHolder.StateCode == alt_AccountHolderState.Active)
                        {
                            var ownersWithoutSpousBenefitiary = ownerAccountHolders.Where(a => a.alt_BeneficiarySpouseAccountHolderId == null);
                            if (ownerAccountHolders.Count() == 1
                                && ownersWithoutSpousBenefitiary.Count() == 1
                                && latestActiveBeneficiariary != null)
                            {
                                accountHolderDal.Update(new alt_AccountHolder
                                {
                                    Id = ownersWithoutSpousBenefitiary.First().Id,
                                    alt_BeneficiarySpouseAccountHolderId = latestActiveBeneficiariary.ToEntityReference()
                                });
                            }
                        }
                        else
                        {
                            var ownersWithTargetSpouseBeneficiary = ownerAccountHolders.Where(a => a.alt_BeneficiarySpouseAccountHolderId != null
                             && a.alt_BeneficiarySpouseAccountHolderId.Id == targetAccountHolder.Id);
                            if (ownersWithTargetSpouseBeneficiary.Count() > 0)
                            {
                                foreach (var accountHolder in ownersWithTargetSpouseBeneficiary)
                                {
                                    var accountHolderToUpdate = new alt_AccountHolder
                                    {
                                        Id = accountHolder.Id,
                                        alt_BeneficiarySpouseAccountHolderId = ownersWithTargetSpouseBeneficiary.Count() == 1
                                            && latestActiveBeneficiariary != null ?
                                        latestActiveBeneficiariary.ToEntityReference() : null
                                    };
                                    accountHolderDal.Update(accountHolderToUpdate);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void HanldeAccountHolderStateCode(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.StateCode)
                && targetAccountHolder.StateCode == alt_AccountHolderState.Active
                && preAccountHolder.StateCode == alt_AccountHolderState.Inactive)
            {
                var mergedeAccountHolder = targetAccountHolder.Merge(preAccountHolder);
                if (mergedeAccountHolder.alt_PortfolioId == null
                      && targetAccountHolder.StatusCode.Value == (int)AccountHolderStatusCode.Active)
                {
                    throw new InvalidPluginExecutionException("לתשומת לבך, לצורך הפעלת הרשומה הנוכחית יש לבחור ב'בהקמה'.");
                }
                else if (mergedeAccountHolder.alt_PortfolioId != null
                    && targetAccountHolder.StatusCode.Value == (int)AccountHolderStatusCode.InProcessing)
                {
                    throw new InvalidPluginExecutionException("לתשומת לבך, לצורך הפעלת הרשומה הנוכחית יש לבחור ב'פעיל'.");

                }
            }
        }

        public void RelateBeneficiaryToOwnerAccountByDigitalFormVerification(alt_AccountHolder targetAccountHolder)
        {
            GlobalContext.LogEntry();
            if (targetAccountHolder.alt_CreationMethodCode?.Value == (int)CreationMethodCode.Manual
                && targetAccountHolder.alt_AccountHolderTypeCode?.Value == (int)AccountHolderTypeCode.Beneficiary
                && targetAccountHolder.alt_DigitalFormVerificationId != null)
            {
                AccountHolderDAL accountHolderDAL = new AccountHolderDAL(GlobalContext);
                List<alt_AccountHolder> retrievedAccountHolders = accountHolderDAL.GetAccountHolderByTypeAccountHolderAndDigitalFormVerificationId(targetAccountHolder.alt_DigitalFormVerificationId.Id, new int[] { (int)AccountHolderTypeCode.Owner }, new string[] { alt_AccountHolder.Fields.alt_AccountHolderId });
                if (retrievedAccountHolders?.Count == 1)
                {
                    alt_AccountHolder accountHolderToUpdate = new alt_AccountHolder()
                    {
                        Id = retrievedAccountHolders.First().Id,
                        alt_BeneficiarySpouseAccountHolderId = targetAccountHolder.ToEntityReference()
                    };
                    accountHolderDAL.Update(accountHolderToUpdate);
                }
            }
        }

        public void SetDefaultOwner(alt_AccountHolder targetAccountHolder)
        {
            GlobalContext.LogEntry();
            TeamDAL teamDAL = new TeamDAL(GlobalContext);
            targetAccountHolder.OwnerId = teamDAL.GetTeamByCodeWithCache().ToEntityReference();
        }

        public void HandleIdentificationNumberComparison(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder = null)
        {
            this.GlobalContext.LogEntry();

            bool isInvalidIdentificationOnCreate = preAccountHolder == null
                && targetAccountHolder.alt_AccountHolderTypeCode != null
                && targetAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner
                && (!targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_OnlineIdentificationNumber)
                    || targetAccountHolder.alt_OnlineIdentificationNumber == null);

            if (isInvalidIdentificationOnCreate)
            {
                targetAccountHolder.alt_IdentificationNumberInitialComparisonCode = new OptionSetValue((int)ComparisonCodes.NotIdentical);
                targetAccountHolder.alt_IdentificationNumberControlComparisonCode = targetAccountHolder.alt_IdentificationNumberInitialComparisonCode;
            }
            else if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_OnlineIdentificationNumber))
            {
                SetComparisonCodes(targetAccountHolder, preAccountHolder);
            }
        }

        private void SetComparisonCodes(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();
            alt_AccountHolder mergedAccountHolder = preAccountHolder != null ? targetAccountHolder.Merge(preAccountHolder) : targetAccountHolder;

            bool isValidForIdentificationComparison = mergedAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_AccountHolderTypeCode)
                    && mergedAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner
                    && mergedAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.StatusCode)
                    && mergedAccountHolder.StatusCode.Value == (int)AccountHolderStatusCode.InProcessing;

            if (isValidForIdentificationComparison)
            {
                CommonBL commonBL = new CommonBL(GlobalContext);
                bool isEqual = commonBL.IsIdentificationNumbersEqual(mergedAccountHolder.alt_OnlineIdentificationNumber, mergedAccountHolder.alt_IdentificationNumber);

                OptionSetValue comparisonResult = isEqual ?
                     new OptionSetValue((int)ComparisonCodes.Identical)
                     : new OptionSetValue((int)ComparisonCodes.NotIdentical);

                targetAccountHolder.alt_IdentificationNumberControlComparisonCode = comparisonResult;
                if (preAccountHolder == null)
                {
                    targetAccountHolder.alt_IdentificationNumberInitialComparisonCode = comparisonResult;
                }
            }
        }

        public void HandleShouldSendTradeInterfaceBit(alt_AccountHolder targetAccountHolder)
        {
            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_AccountHolderTypeCode)
                  && targetAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner
                  && targetAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_DigitalFormVerificationId))
            {
                targetAccountHolder.alt_ShouldSendTradeInterfaceBit = true;
            }
        }

        public void SetStateCodeByStatusCode(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.StatusCode))
            {
                alt_AccountHolderState state = alt_AccountHolderState.Active;
                AccountHolderStatusCode accountHolderStatusCode = (AccountHolderStatusCode)targetAccountHolder.StatusCode.Value;
                switch (accountHolderStatusCode)
                {
                    case AccountHolderStatusCode.Inactive:
                    case AccountHolderStatusCode.Canceled:
                        {
                            state = alt_AccountHolderState.Inactive;
                            break;
                        }
                    default:
                        break;
                }
                if (preAccountHolder.StateCode != state)
                {
                    targetAccountHolder.StateCode = state;
                }
            }
        }

        public void UpdateSpouseAccountHolder(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_SpouseAccountHolderId))
            {
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                accountHolderDal.Update(new alt_AccountHolder()
                {
                    Id = targetAccountHolder.alt_SpouseAccountHolderId.Id,
                    alt_SpouseAccountHolderId = targetAccountHolder.ToEntityReference()
                });
            }
        }

        public void HandleBeneficiarySigningDeclarationCode(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (!targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_BeneficiarySigningDeclarationCode)
                && targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_AccountHolderTypeCode)
                && targetAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
            {
                targetAccountHolder.alt_BeneficiarySigningDeclarationCode = new OptionSetValue((int)BeneficiarySigningDeclarationCode.Other);
            }
        }

        public void HandleCheckTerrorOrganizationCode(alt_AccountHolder targetAccountHolder, int depth)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_CheckTerrorOrganizationCode) && depth == 1)
            {
                targetAccountHolder.alt_CheckTerrorOrganizationSystemUserId = new EntityReference(SystemUser.EntityLogicalName, GlobalContext.InitiatingUserId);
            }
        }

        public void HandleBeneficiaryDeclarationControlCode(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_BeneficiaryDeclarationControlCode))
            {
                targetAccountHolder.alt_BeneficiaryDeclarationSystemUserId = new EntityReference(SystemUser.EntityLogicalName, GlobalContext.InitiatingUserId);
            }
        }

        public void CancelKYCOnAccountHolderInactive(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.StateCode)
                && targetAccountHolder.StateCode == alt_AccountHolderState.Inactive)
            {
                KYCDAL kycDal = new KYCDAL(this.GlobalContext);
                List<alt_KYC> kycs = kycDal.GetAllActiveKYCByAccountHolderId(targetAccountHolder.Id, new[] { alt_KYC.Fields.Id });
                foreach (alt_KYC kycRecord in kycs)
                {
                    alt_KYC kyc = new alt_KYC()
                    {
                        alt_KYCId = kycRecord.Id,
                        StateCode = alt_KYCState.Inactive,
                        StatusCode = new OptionSetValue((int)KYCStatusCode.Canceled),
                    };
                    kycDal.Update(kyc);
                }
            }
        }

        public void UpdateDigitalFormVerification(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();
            alt_AccountHolder mergedAccountHolder = targetAccountHolder.Merge(preAccountHolder);
            if (mergedAccountHolder.alt_DigitalFormVerificationId != null)
            {
                alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification() { };

                bool isNeedToUpdate = this.HandleMovingNextControlStep(digitalFormVerificationToUpdate, targetAccountHolder, mergedAccountHolder);
                this.HandleMainAccountHolder(digitalFormVerificationToUpdate, targetAccountHolder);
              
                //if (isCheckUpdateDigitalFormVerification)
                if (isNeedToUpdate || digitalFormVerificationToUpdate.Attributes.Any())
                {
                    digitalFormVerificationToUpdate.Id = mergedAccountHolder.alt_DigitalFormVerificationId.Id;
                    DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                    if (IsUpdateDigitalFormVerification(digitalFormVerificationToUpdate, digitalFormVerificationDal))
                    {
                        digitalFormVerificationDal.Update(digitalFormVerificationToUpdate);
                    }
                }
            }
        }

        private void HandleMainAccountHolder(alt_DigitalFormVerification digitalFormVerificationToUpdate, alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();

            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_MainAccountHolderBit)
                && targetAccountHolder.alt_MainAccountHolderBit.Value)
            {
                digitalFormVerificationToUpdate.Attributes.Add(alt_DigitalFormVerification.Fields.alt_PrimaryAccountHolderId, targetAccountHolder.ToEntityReference());
            }
        }

        private bool HandleMovingNextControlStep(alt_DigitalFormVerification digitalFormVerificationToUpdate, alt_AccountHolder targetAccountHolder, alt_AccountHolder mergedAccountHolder)
        {
            this.GlobalContext.LogEntry();
            bool needToUpdate = false;


            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_CheckTerrorOrganizationCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_DigitalVisualRecognitionCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_PerformAdditionalVerificationCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_BeneficiaryDeclarationControlCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_PostalCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_ManualControlVerificationIDAppliedCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_ManualControlVerificationIDDescription)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.StateCode))
            {
                needToUpdate = true;
                SetFieldsForMovingNextControlStepTab(digitalFormVerificationToUpdate, mergedAccountHolder);
            }

            if (mergedAccountHolder.alt_AccountHolderTypeCode != null
                && mergedAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
            {
                // bool isManagerVerificationRequired = ManagerVerificationRequired(targetAccountHolder);
                if (ManagerVerificationRequired(targetAccountHolder))
                {
                    needToUpdate = true;
                    digitalFormVerificationToUpdate.alt_ManagerVerificationRequiredCode = new OptionSetValue((int)ManagerVerificationRequiredCode.Yes);
                }
            }

            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_CreationMethodCode)
                     && targetAccountHolder.alt_CreationMethodCode.Value != (int)CreationMethodCode.Interface
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.StateCode))
            {
                digitalFormVerificationToUpdate.alt_ManagerVerificationRequiredCode = new OptionSetValue((int)ManagerVerificationRequiredCode.Yes);
                needToUpdate = true;
            }

            return needToUpdate;
        }

        public void SetAlternateKey(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder = null)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_PortfolioId)
                && !targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_Code))
            {
                alt_AccountHolder mergedAccountHolder = preAccountHolder != null ?
                    targetAccountHolder.Merge(preAccountHolder) : targetAccountHolder;
                string alternateKey = null;

                if (mergedAccountHolder.alt_PortfolioId != null
                    && !string.IsNullOrWhiteSpace(mergedAccountHolder.alt_IdentificationNumber))
                {
                    CommonDAL commonDal = new CommonDAL(this.GlobalContext, alt_Portfolio.EntityLogicalName);

                    string shenhavAccountNumber = (commonDal.Get(mergedAccountHolder.alt_PortfolioId.Id, new string[] { alt_Portfolio.Fields.alt_ShenhavAccountNumber })
                        .ToEntity<alt_Portfolio>()).alt_ShenhavAccountNumber;
                    string internalIdentityNumber = mergedAccountHolder.alt_IdentificationNumber.GetPadedLeftZeroString();
                    alternateKey = $"{shenhavAccountNumber}-{internalIdentityNumber}";
                }
                targetAccountHolder.alt_Code = alternateKey;
            }
        }

        public void SetCustomerByIdentificationNumber(alt_AccountHolder targetAccountHolder)
        {
            if (targetAccountHolder.AttributeHasValue<string>(alt_AccountHolder.Fields.alt_IdentificationNumber)
                && targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_IdentificationTypeCode)
                && !targetAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_CustomerId))
            {
                EntityReference customerId = null;
                Entity retrievedCustomer;
                if (targetAccountHolder.alt_IdentificationTypeCode.Value == (int)IdentificationTypeCode.GovernmentId
                    || targetAccountHolder.alt_IdentificationTypeCode.Value == (int)IdentificationTypeCode.Passport
                    || targetAccountHolder.alt_IdentificationTypeCode.Value == (int)IdentificationTypeCode.DrivingLicense)
                {
                    ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                    retrievedCustomer = contactDal.GetByGovernmentId(targetAccountHolder.alt_IdentificationNumber);
                    if (retrievedCustomer == null)
                    {
                        Guid contactId = contactDal.Create(new Contact
                        {
                            GovernmentId = targetAccountHolder.alt_IdentificationNumber,
                            FirstName = targetAccountHolder.alt_FirstName,
                            LastName = targetAccountHolder.alt_LastName
                        });
                        customerId = new EntityReference(Contact.EntityLogicalName, contactId);
                    }
                }
                else
                {
                    AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                    retrievedCustomer = accountDal.GetByAccountNumber(targetAccountHolder.alt_IdentificationNumber);
                    if (retrievedCustomer == null)
                    {
                        Guid accountId = accountDal.Create(new Account
                        {
                            AccountNumber = targetAccountHolder.alt_IdentificationNumber,
                            Name = targetAccountHolder.alt_FirstName
                        });
                        customerId = new EntityReference(Account.EntityLogicalName, accountId);
                    }
                }
                targetAccountHolder.alt_CustomerId = customerId ?? retrievedCustomer?.ToEntityReference();
            }
        }

        private void CheckAllAccountHolder(Guid digitalFormVerificationId, alt_DigitalFormVerification digitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);

            List<alt_AccountHolder> retrievedAccountHolders = accountHolderDal.GetAllAccountHolderByDigitalFormVerificationId(digitalFormVerificationId);
            if (retrievedAccountHolders.Count > 0)
            {
                SetFieldsForMovingNextControlStepByAccountHolderType(digitalFormVerification, retrievedAccountHolders);
            }
        }

        private bool IsUpdateDigitalFormVerification(alt_DigitalFormVerification digitalFormVerification, DigitalFormVerificationDAL digitalFormVerificationDal)
        {
            this.GlobalContext.LogEntry();
            string[] columns = new[]{
                    alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode,
                    alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersForStageJoiningBit,
                    alt_DigitalFormVerification.Fields.alt_BeneficiaryDeclarationControlExistsBit,
                    alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersStageManagementBit
                };
            alt_DigitalFormVerification digitalFormVerificationRetrieve = digitalFormVerificationDal.Get(digitalFormVerification.Id, columns);

            return (digitalFormVerification.AttributeHasValue<OptionSetValue>(alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode)
                    && digitalFormVerificationRetrieve.alt_ManagerVerificationRequiredCode.Value != digitalFormVerification.alt_ManagerVerificationRequiredCode.Value)
                || digitalFormVerificationRetrieve.alt_VerifiedAccountHoldersForStageJoiningBit != digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit
                || digitalFormVerificationRetrieve.alt_BeneficiaryDeclarationControlExistsBit != digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit
                || digitalFormVerificationRetrieve.alt_VerifiedAccountHoldersStageManagementBit != digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit;
        }

        private void SetFieldsForMovingNextControlStepTab(alt_DigitalFormVerification digitalFormVerification, alt_AccountHolder mergedAccountHolder)
        {
            this.GlobalContext.LogEntry();
            digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit = true;
            digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit = true;
            digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit = true;

            List<alt_AccountHolder> accountHolders = new List<alt_AccountHolder>();
            if (mergedAccountHolder.StateCode == alt_AccountHolderState.Active)
            {
                accountHolders.Add(mergedAccountHolder);
            }
            SetFieldsForMovingNextControlStepByAccountHolderType(digitalFormVerification, accountHolders);

            if (digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit == true
                 || digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit == true
                 || digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit == true)
            {
                CheckAllAccountHolder(mergedAccountHolder.alt_DigitalFormVerificationId.Id, digitalFormVerification);
            }
        }

        private void CheckOwnerTypeAccountHolder(alt_DigitalFormVerification digitalFormVerification, alt_AccountHolder accountHolder)
        {
            this.GlobalContext.LogEntry();
            if (digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit.Value
                && !VerifiedAccountHoldersForStageJoining(accountHolder))
            {
                digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit = false;
            }
            if (digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit.Value
                && accountHolder.alt_BeneficiaryDeclarationControlCode == null)
            {
                digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit = false;
            }
            if (digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit.Value
                && !VerifiedAccountHoldersStageManagement(accountHolder))
            {
                digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit = false;
            }
        }

        private bool VerifiedAccountHoldersForStageJoining(alt_AccountHolder accountHolder)
        {
            this.GlobalContext.LogEntry();
            return accountHolder.alt_DigitalVisualRecognitionCode != null
                && (accountHolder.alt_DigitalVisualRecognitionCode.Value == (int)DigitalVisualRecognitionCode.Valid
                    || accountHolder.alt_PerformAdditionalVerificationCode != null
                && accountHolder.alt_PostalCode != null);
        }

        private bool VerifiedAccountHoldersStageManagement(alt_AccountHolder accountHolder)
        {
            this.GlobalContext.LogEntry();
            return accountHolder.alt_DigitalVisualRecognitionCode != null
                && (accountHolder.alt_DigitalVisualRecognitionCode.Value == (int)DigitalVisualRecognitionCode.Valid
                    || accountHolder.alt_PerformAdditionalVerificationCode != null);
        }

        private void SetFieldsForMovingNextControlStepByAccountHolderType(alt_DigitalFormVerification digitalFormVerification, List<alt_AccountHolder> accountHolders)
        {
            this.GlobalContext.LogEntry();
            foreach (alt_AccountHolder accountHolder in accountHolders)
            {
                CheckAllTypeAccountHolder(digitalFormVerification, accountHolder);

                if (accountHolder.alt_AccountHolderTypeCode != null)
                {
                    if (accountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
                    {
                        CheckOwnerTypeAccountHolder(digitalFormVerification, accountHolder);
                    }
                    if (accountHolder.alt_AccountHolderTypeCode.Value != (int)AccountHolderTypeCode.Beneficiary)
                    {
                        CheckUnequalBeneficiaryTypeAccountHolder(digitalFormVerification, accountHolder);
                    }
                }
            }
        }

        private void CheckUnequalBeneficiaryTypeAccountHolder(alt_DigitalFormVerification digitalFormVerification, alt_AccountHolder accountHolder)
        {
            this.GlobalContext.LogEntry();
            if (digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit.Value
                && (accountHolder.alt_ManualControlVerificationIDAppliedCode == null
                    || accountHolder.alt_ManualControlVerificationIDDescription == null))
            {
                digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit = false;
            }
        }

        private void CheckAllTypeAccountHolder(alt_DigitalFormVerification digitalFormVerification, alt_AccountHolder accountHolder)
        {
            this.GlobalContext.LogEntry();
            if (accountHolder.alt_CheckTerrorOrganizationCode == null)
            {
                digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit = false;
                digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit = false;
            }
            else if (accountHolder.alt_CheckTerrorOrganizationCode.Value != (int)CheckTerrorOrganizationCode.Valid)
            {
                digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit = false;
            }
        }

        private bool ManagerVerificationRequired(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.StateCode == alt_AccountHolderState.Inactive)
            {
                return false;
            }
            else
            {
                return (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_CheckTerrorOrganizationCode)
                               && targetAccountHolder.alt_CheckTerrorOrganizationCode.Value == (int)CheckTerrorOrganizationCode.Invalid)
                           || (targetAccountHolder.AttributeHasValue<bool>(alt_AccountHolder.Fields.alt_BeneficiaryDeclarationRequiredBit)
                               && targetAccountHolder.alt_BeneficiaryDeclarationRequiredBit.Value)
                           || (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_BeneficiaryDeclarationControlCode)
                               && targetAccountHolder.alt_BeneficiaryDeclarationControlCode.Value != (int)BeneficiaryDeclarationControlCode.Valid);
            }
        }

        public void HandleApprovalsRound(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder = null)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_CheckTerrorOrganizationCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_PerformVerificationCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_DigitalVisualRecognitionCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_PerformAdditionalVerificationCode)
                || targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_BeneficiaryDeclarationControlCode))
            {
                alt_AccountHolder mergedAccountHolder = preAccountHolder == null ?
                    targetAccountHolder : targetAccountHolder.Merge(preAccountHolder);
                if (mergedAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_DigitalFormVerificationId))
                {
                    alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification();
                    DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                    var digitalFormVerification = digitalFormVerificationDal.GetDigitalFormVerificationDetails(mergedAccountHolder.alt_DigitalFormVerificationId.Id);

                    if (mergedAccountHolder.alt_AccountHolderTypeCode != null
                        && mergedAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
                    {
                        this.SetManagerVerificationRequired(targetAccountHolder, digitalFormVerification, digitalFormVerificationToUpdate);
                        this.SetBeneficiaryDeclarationControlExistsBit(mergedAccountHolder, digitalFormVerification, digitalFormVerificationToUpdate);
                    }
                    this.SetVerifiedAccountHoldersStageManagementBit(mergedAccountHolder, digitalFormVerification, digitalFormVerificationToUpdate);
                    this.SetVerifiedAccountHoldersForStageJoiningBit(mergedAccountHolder, digitalFormVerification, digitalFormVerificationToUpdate);

                    if (digitalFormVerificationToUpdate.Attributes.Count > 0)
                    {
                        digitalFormVerificationToUpdate.Id = digitalFormVerification.Id;
                        digitalFormVerificationDal.Update(digitalFormVerificationToUpdate);
                    }
                }
            }
        }

        private void SetVerifiedAccountHoldersStageManagementBit(alt_AccountHolder accountHolder, alt_DigitalFormVerification digitalFormVerification, alt_DigitalFormVerification digitalFormVerificationToUpdate)
        {
            this.GlobalContext.LogEntry();

            if (digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit.Value)
            {
                bool isVerified;
                if (accountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
                {
                    isVerified = (accountHolder.alt_DigitalVisualRecognitionCode != null
                                    && accountHolder.alt_DigitalVisualRecognitionCode.Value == (int)DigitalVisualRecognitionCode.Valid)
                                || accountHolder.alt_PerformAdditionalVerificationCode != null
                                || (accountHolder.alt_CheckTerrorOrganizationCode != null
                                    && accountHolder.alt_CheckTerrorOrganizationCode.Value == (int)CheckTerrorOrganizationCode.Valid);
                }
                else
                {
                    isVerified = accountHolder.alt_CheckTerrorOrganizationCode != null
                        || accountHolder.alt_CheckTerrorOrganizationCode.Value == (int)CheckTerrorOrganizationCode.Valid;
                }
                if (isVerified != digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit.Value)
                {
                    digitalFormVerificationToUpdate.Attributes.Add(alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersStageManagementBit, isVerified);
                    digitalFormVerification.alt_VerifiedAccountHoldersStageManagementBit = isVerified;
                }
            }
        }

        private void SetBeneficiaryDeclarationControlExistsBit(alt_AccountHolder accountHolder, alt_DigitalFormVerification digitalFormVerification, alt_DigitalFormVerification digitalFormVerificationToUpdate)
        {
            this.GlobalContext.LogEntry();

            bool isExist = accountHolder.alt_BeneficiaryDeclarationControlCode == null;
            if (isExist != digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit)
            {
                digitalFormVerification.alt_BeneficiaryDeclarationControlExistsBit = isExist;
                digitalFormVerificationToUpdate.Attributes.Add(alt_DigitalFormVerification.Fields.alt_BeneficiaryDeclarationControlExistsBit, isExist);
            }
        }

        private void SetVerifiedAccountHoldersForStageJoiningBit(alt_AccountHolder accountHolder, alt_DigitalFormVerification digitalFormVerification, alt_DigitalFormVerification digitalFormVerificationToUpdate)
        {
            this.GlobalContext.LogEntry();

            bool isVerified;
            if (accountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
            {
                isVerified = (accountHolder.alt_CheckTerrorOrganizationCode != null
                        && accountHolder.alt_PerformVerificationCode != null)
                        || accountHolder.alt_PerformVerificationCode.Value != (int)PerformVerificationCode.Digital
                        || (accountHolder.alt_PerformVerificationCode.Value == (int)PerformVerificationCode.Digital
                            && accountHolder.alt_DigitalVisualRecognitionCode != null
                            && accountHolder.alt_DigitalVisualRecognitionCode.Value == (int)DigitalVisualRecognitionCode.Valid);
            }
            else
            {
                isVerified = accountHolder.alt_CheckTerrorOrganizationCode != null;
            }
            if (isVerified != digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit)
            {
                digitalFormVerification.alt_VerifiedAccountHoldersForStageJoiningBit = isVerified;
                digitalFormVerificationToUpdate.Attributes.Add(alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersForStageJoiningBit, isVerified);
            }
        }

        private void SetManagerVerificationRequired(alt_AccountHolder targetAccountHolder, alt_DigitalFormVerification digitalFormVerification, alt_DigitalFormVerification digitalFormVerificationToUpdate)
        {
            this.GlobalContext.LogEntry();

            bool managerRequired = ((targetAccountHolder.alt_CheckTerrorOrganizationCode != null
                    && targetAccountHolder.alt_CheckTerrorOrganizationCode.Value == (int)CheckTerrorOrganizationCode.Invalid)
                || (targetAccountHolder.alt_BeneficiaryDeclarationRequiredBit != null
                    && targetAccountHolder.alt_BeneficiaryDeclarationRequiredBit.Value)
                || (targetAccountHolder.alt_BeneficiaryDeclarationControlCode != null
                    && targetAccountHolder.alt_BeneficiaryDeclarationControlCode.Value != (int)BeneficiaryDeclarationControlCode.Valid));
            if (managerRequired
                && (digitalFormVerification.alt_ManagerVerificationRequiredCode == null
                    || digitalFormVerification.alt_ManagerVerificationRequiredCode.Value != (int)ManagerVerificationRequiredCode.Yes))
            {
                digitalFormVerification.alt_ManagerVerificationRequiredCode = new OptionSetValue((int)ManagerVerificationRequiredCode.Yes);
                digitalFormVerificationToUpdate.Attributes.Add(alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode, new OptionSetValue((int)ManagerVerificationRequiredCode.Yes));
            }
        }

        public void HandleAutomaticMailing(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();

            if (targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.StatusCode))
            {
                var mergedAccountHolder = targetAccountHolder.Equals(preAccountHolder) ?
                    targetAccountHolder : targetAccountHolder.Merge(preAccountHolder);

                if (mergedAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.alt_AccountHolderTypeCode)
                    && mergedAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner
                    && mergedAccountHolder.alt_MainAccountHolderBit.HasValue
                    && mergedAccountHolder.alt_MainAccountHolderBit.Value)
                {
                    AccountHolderStatusCode accountHolderStatusCode = (AccountHolderStatusCode)targetAccountHolder.StatusCode.Value;
                    string mailingProcessName = null;
                    switch (accountHolderStatusCode)
                    {
                        case AccountHolderStatusCode.InProcessing:
                            {
                                this.CreateCustomerOperationRequest(mergedAccountHolder, CustomerOperationTemplateCode.CompletionDigitalJoiningProcess);
                                break;
                            }
                        case AccountHolderStatusCode.Active:
                        case AccountHolderStatusCode.Inactive:
                        case AccountHolderStatusCode.Canceled:
                        default:
                            {
                                break;
                            }
                    }
                    this.SendSmsAndEmail(mergedAccountHolder, mergedAccountHolder.alt_DigitalFormVerificationId, mailingProcessName);
                }
            }
        }

        private void SendSmsAndEmail(alt_AccountHolder mergedAccountHolder, EntityReference regardingObjectId, string mailingProcessName)
        {
            this.GlobalContext.LogEntry();
            if (!string.IsNullOrWhiteSpace(mailingProcessName))
            {
                Recipient recipient = new Recipient()
                {
                    CustomerId = mergedAccountHolder.alt_CustomerId,
                    MobilePhone = mergedAccountHolder.alt_MobilePhone,
                    Email = mergedAccountHolder.alt_Email
                };

                CommonBL commonBL = new CommonBL(this.GlobalContext);
                commonBL.ExecuteTradeAutomaticMailing(regardingObjectId, recipient, mergedAccountHolder.ToEntityReference(), mailingProcessName);
            }
        }

        public void HandleCustomerOperationRequests(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();
            if (targetAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_PortfolioId)
                && targetAccountHolder.AttributeHasValue<OptionSetValue>(alt_AccountHolder.Fields.StatusCode)
                && targetAccountHolder.StatusCode.Value == (int)AccountHolderStatusCode.Active)
            {
                var mergedAccountHolder = targetAccountHolder.Equals(preAccountHolder) ?
                    targetAccountHolder : targetAccountHolder.Merge(preAccountHolder);
                if (mergedAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_DigitalFormVerificationId)
                    && mergedAccountHolder.alt_AccountHolderTypeCode != null
                    && mergedAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner)
                {
                    this.CreateCustomerOperationRequest(mergedAccountHolder, CustomerOperationTemplateCode.SendJoiningBenefit);

                    if (mergedAccountHolder.alt_MainAccountHolderBit != null && mergedAccountHolder.alt_MainAccountHolderBit.Value)
                    {
                        this.CreateCustomerOperationRequest(mergedAccountHolder, CustomerOperationTemplateCode.SendCustomerAgreement);

                        if (mergedAccountHolder.alt_ShouldSendTradeInterfaceBit != null && mergedAccountHolder.alt_ShouldSendTradeInterfaceBit.Value)
                        {
                            this.CreateCustomerOperationRequest(mergedAccountHolder, CustomerOperationTemplateCode.OpenTradeOneUser);
                        }
                    }
                }
            }
        }

        public void HandleCustomerOperationRequests(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();

            if (targetAccountHolder.AttributeHasValue<EntityReference>(alt_AccountHolder.Fields.alt_DigitalFormVerificationId)
                && targetAccountHolder.alt_AccountHolderTypeCode != null
                && targetAccountHolder.alt_AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner
                && targetAccountHolder.alt_MainAccountHolderBit != null
                && targetAccountHolder.alt_MainAccountHolderBit.Value
                && this.IsAligible(targetAccountHolder))
            {

                this.CreateCustomerOperationRequest(targetAccountHolder, CustomerOperationTemplateCode.CheckEligibilityBenefit);
            }
        }

        internal bool IsAligible(alt_AccountHolder targetAccountHolder)
        {
            this.GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
            var retrievedDigitalFormVerification = digitalFormVerificationDal.GetActiveByAttribute(
                alt_DigitalFormVerification.Fields.alt_DigitalFormVerificationId,
                targetAccountHolder.alt_DigitalFormVerificationId.Id,
                new string[] { alt_DigitalFormVerification.Fields.alt_ReferralSourceId })?
                .FirstOrDefault();
            if (retrievedDigitalFormVerification?.alt_ReferralSourceId != null)
            {
                ReferralSourceDAL referralSourceDal = new ReferralSourceDAL(this.GlobalContext);
                return referralSourceDal.IsReferralSourceMivtza(retrievedDigitalFormVerification.alt_ReferralSourceId);
            }
            else
            {
                return false;
            }
        }

        private void CreateCustomerOperationRequest(alt_AccountHolder mergedAccountHolder, CustomerOperationTemplateCode customerOperationTemplateCode)
        {
            this.GlobalContext.LogEntry();
            try
            {
                this.GlobalContext.OrganizationService.Create(new alt_CustomerOperationRequest
                {
                    alt_RelatedRecordId = mergedAccountHolder.ToEntityReference(),
                    alt_CustomerOperationTemplateCodeInt = (int)customerOperationTemplateCode,
                    OwnerId = mergedAccountHolder.OwnerId
                });
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Warning($"Create Customer Operation Request {customerOperationTemplateCode} Faild.{Environment.NewLine}{ex.ToString()}");
            }
        }

        public void HandleTradeOneUserNameUpdateFromShenhav(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();

            if (targetAccountHolder.Contains(alt_AccountHolder.Fields.alt_UserNameTrade)
                && string.IsNullOrWhiteSpace(targetAccountHolder.alt_UserNameTrade))
            {
                targetAccountHolder.alt_UserNameTrade = preAccountHolder.alt_UserNameTrade;
            }
        }

        public void HandleTradingCourseMailing(alt_AccountHolder targetAccountHolder, alt_AccountHolder preAccountHolder)
        {
            this.GlobalContext.LogEntry();

            if (targetAccountHolder.alt_SentCustomerAgreementBit != null
                && targetAccountHolder.alt_SentCustomerAgreementBit.Value)
            {
                var mergedAccountHolder = targetAccountHolder.Merge(preAccountHolder);
                this.SendSmsAndEmail(mergedAccountHolder, mergedAccountHolder.alt_PortfolioId, "TradingCourse");
            }
        }
    }
}