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

namespace Alt.BusinessLogicLayer.Crm
{
    public class KYCBL : CrmBaseBL
    {
        public KYCBL(GlobalContext globalContext) : base(globalContext)
        {

        }

        public void SetKYCName(alt_KYC targetKYC, alt_KYC preKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_DigitalFormVerificationId)
                || targetKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_AccountHolderId)
                || targetKYC.AttributeHasValue<string>(alt_KYC.Fields.alt_Name))
            {
                alt_KYC mergedKYC = targetKYC.Merge(preKYC);
                List<string> nameParts = new List<string>();
                if (mergedKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_DigitalFormVerificationId))
                {
                    DigitalFormVerificationDAL digitalFormVerificationtDal = new DigitalFormVerificationDAL(this.GlobalContext);
                    nameParts.Add(digitalFormVerificationtDal.Get(mergedKYC.alt_DigitalFormVerificationId.Id, new string[] { alt_DigitalFormVerification.Fields.alt_DigitalFormNumber }).alt_DigitalFormNumber);
                }
                if (targetKYC.AttributeHasValue<string>(alt_KYC.Fields.alt_Name))
                {
                    nameParts.Add(targetKYC.alt_Name);
                }
                else if (mergedKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_AccountHolderId))
                {
                    AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                    nameParts.Add(accountHolderDal.GetPrimeryAttributeValue(mergedKYC.alt_AccountHolderId, alt_AccountHolder.Fields.alt_Name));
                }
                string name = string.Join(" - ", nameParts);
                if (targetKYC.alt_Name != name)
                {
                    targetKYC.alt_Name = name;
                }
            }
        }

        public void SetDefaultValues(alt_KYC targetKYC)
        {
            GlobalContext.LogEntry();
            TeamDAL teamDAL = new TeamDAL(GlobalContext);
            targetKYC.OwnerId = teamDAL.GetTeamByCodeWithCache().ToEntityReference();
        }

        public void HandleEmploymentCategoryOccupationId(alt_KYC targetKYC)
        {
            if (targetKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_EmploymentCategoryOccupationId))
            {
                int otheOccupationCode = GlobalContext.CacheManager.GetGlobalParameter<int>("OtheOccupationCode");
                OccupationDAL occupationDal = new OccupationDAL(this.GlobalContext);
                alt_Occupation occupation = occupationDal.Get(targetKYC.alt_EmploymentCategoryOccupationId.Id, new[] { alt_Occupation.Fields.alt_CodeInt });
                if (occupation.alt_CodeInt.Value == otheOccupationCode)
                {
                    this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.Other);
                }
            }
        }

        private void AddValueToManualHandlingReasonsCode(alt_KYC targetKYC, int value)
        {
            this.GlobalContext.LogEntry();
            OptionSetValueCollection newValuesManualHandlingReasons = targetKYC.AttributeHasValue<OptionSetValueCollection>(alt_KYC.Fields.alt_ManualHandlingReasonsCode) ?
                targetKYC.alt_ManualHandlingReasonsCode : new OptionSetValueCollection();

            if (!newValuesManualHandlingReasons?.Any(v => v.Value == value) ?? false)
            {
                newValuesManualHandlingReasons.Add(new OptionSetValue(value));
                targetKYC.alt_ManualHandlingReasonsCode = newValuesManualHandlingReasons;
            }
        }

        public void HandleMonthlyIncomeLevelNIS(alt_KYC targetKYC, alt_KYC preKYC = null)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.AttributeHasValue<OptionSetValue>(alt_KYC.Fields.alt_MonthlyIncomeLevelNISCode)
                || targetKYC.AttributeHasValue<OptionSetValue>(alt_KYC.Fields.alt_TotalDepositForecastPerYearCode))
            {
                var mergedKYC = preKYC == null ? targetKYC : targetKYC.Merge(preKYC);
                if (mergedKYC.AttributeHasValue<OptionSetValue>(alt_KYC.Fields.alt_MonthlyIncomeLevelNISCode)
                     && mergedKYC.alt_MonthlyIncomeLevelNISCode.Value == (int)MonthlyIncomeLevelNISCode.UpToSixThousandNIS
                     && mergedKYC.AttributeHasValue<OptionSetValue>(alt_KYC.Fields.alt_TotalDepositForecastPerYearCode)
                     && mergedKYC.alt_TotalDepositForecastPerYearCode.Value != (int)TotalDepositForecastPerYearCode.UpToFiftyThousand)
                {
                    this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.NoCompatibilityBetweenIncomeLevelAndDeposit);
                }
            }
        }

        public void HandleEmploymentTypeCode(alt_KYC targetKYC, alt_KYC preKYC = null)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.Contains(alt_KYC.Fields.alt_EmploymentTypeCode))
            {
                var mergedKYC = preKYC == null ? targetKYC : targetKYC.Merge(preKYC);
                int[] fundsSourceTrigger;
                switch (targetKYC.alt_EmploymentTypeCode.Value)
                {
                    case (int)EmploymentTypeCode.Else:
                        {
                            this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.Other);
                        }
                        break;
                    case (int)EmploymentTypeCode.Employee:
                        {
                            fundsSourceTrigger = new int[] { (int)FundsSourceCode.IncomeSourceBusiness, (int)FundsSourceCode.Dividend };
                            if (mergedKYC.AttributeHasValue<OptionSetValueCollection>(alt_KYC.Fields.alt_FundsSourceCode)
                                && mergedKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceTrigger.Any(v => v == fundsSourceItem.Value)))
                            {
                                this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.NoCompatibilityBetweenEmploymentTypeAndFundsSource);
                            }
                        }
                        break;
                    case (int)EmploymentTypeCode.Student:
                    case (int)EmploymentTypeCode.Soldier:
                        {
                            fundsSourceTrigger = new int[] { (int)FundsSourceCode.IncomeSourceBusiness, (int)FundsSourceCode.Dividend };
                            if (mergedKYC.AttributeHasValue<OptionSetValueCollection>(alt_KYC.Fields.alt_FundsSourceCode)
                                && mergedKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceTrigger.Any(v => v == fundsSourceItem.Value)))
                            {
                                this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.NoCompatibilityBetweenEmploymentTypeAndFundsSource);
                            }
                        }
                        break;
                    case (int)EmploymentTypeCode.Unemployed:
                        {
                            fundsSourceTrigger = new int[] { (int)FundsSourceCode.Salary, (int)FundsSourceCode.IncomeSourceBusiness, (int)FundsSourceCode.Dividend };
                            if (mergedKYC.AttributeHasValue<OptionSetValueCollection>(alt_KYC.Fields.alt_FundsSourceCode)
                                && mergedKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceTrigger.Any(v => v == fundsSourceItem.Value)))
                            {
                                this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.NoCompatibilityBetweenEmploymentTypeAndFundsSource);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void HandleBankServiceDenialUpdate(alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.AttributeHasValue<bool>(alt_KYC.Fields.alt_BankServiceDenialBit)
                && targetKYC.alt_BankServiceDenialBit.Value == true)
            {
                this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.DenialBankService);
            }
        }

        public void HandleAdditionalAccountExistsatAltshulerUpdate(alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.AttributeHasValue<bool>(alt_KYC.Fields.alt_AdditionalAccountExistsAtAltshulerBit)
                && targetKYC.alt_AdditionalAccountExistsAtAltshulerBit.Value == true)
            {
                this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.AdditionalAccountExists);
            }
        }

        public void HandleFundsSourceUpdate(alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.AttributeHasValue<OptionSetValueCollection>(alt_KYC.Fields.alt_FundsSourceCode))
            {
                int[] fundsSourceTrigger = { (int)FundsSourceCode.Inheritance, (int)FundsSourceCode.Gift, (int)FundsSourceCode.ForeignTerritory, (int)FundsSourceCode.Else };
                if (targetKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceTrigger.Any(v => v == fundsSourceItem.Value)))
                {
                    this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.FundsSource);
                }
                if (targetKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceItem.Value == (int)FundsSourceCode.Else))
                {
                    this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.Other);
                }
            }
        }

        public void HandleManualHandlingReasonsCode(alt_KYC targetKYC, alt_KYC preKYC)
        {
            this.GlobalContext.LogEntry();

            if (preKYC.alt_ManualHandlingReasonsCode != null
                && preKYC.alt_ManualHandlingReasonsCode.Contains(new OptionSetValue((int)ManualHandlingReasonsCode.TriggeredByRepresentative)))
            {
                this.AddValueToManualHandlingReasonsCode(targetKYC, (int)ManualHandlingReasonsCode.TriggeredByRepresentative);
            }
            alt_KYC mergedKYC = targetKYC.Merge(preKYC);
            if ((targetKYC.alt_ManualHandlingRequiredBit != null && targetKYC.alt_ManualHandlingRequiredBit.Value)
                || mergedKYC.alt_ManualHandlingReasonsCode != null)
            {
                targetKYC.alt_ManualHandlingRequiredBit = true;
            }
            else
            {
                targetKYC.alt_ManualHandlingRequiredBit = false;
            }
        }

        public void SetFildsScoreTheCalculatorSection(alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            string jsonScoreDefinition = GetJsonScoreDefinition();

            if (!string.IsNullOrEmpty(jsonScoreDefinition))
            {
                MoneyLaunderingRiskCalculator moneyLaunderingRiskCalculator = JsonSerializer.Deserialize<MoneyLaunderingRiskCalculator>(jsonScoreDefinition);

                foreach (AttributesMoneyLaunderingCalculator attributesMoneyLaunderingCalculator in moneyLaunderingRiskCalculator.MoneyLaunderingCalculator.attributesMoneyLaunderingCalculator)
                {
                    int[] currentValue = this.GetCurrentValuePerType(attributesMoneyLaunderingCalculator, targetKYC);
                    if (currentValue != null)
                    {
                        int? scoreDestination = GetValueScoreDestination(currentValue, attributesMoneyLaunderingCalculator.dataSource.sourceValues);
                        if (scoreDestination != null)
                        {
                            this.SetScoreInKYCPerType(attributesMoneyLaunderingCalculator, scoreDestination, targetKYC);
                        }
                    }
                }
                SetScoreInKYCEntityReference(targetKYC);
            }
        }

        public void SetRelatedPortfolioCustomerId(alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.Contains(alt_KYC.Fields.alt_RelatedPortfolioIdentityNumber))
            {
                if (!string.IsNullOrWhiteSpace(targetKYC.alt_RelatedPortfolioIdentityNumber))
                {
                    ContactDAL contactDAL = new ContactDAL(this.GlobalContext);
                    Contact retrievedContact = contactDAL.GetByGovernmentId(targetKYC.alt_RelatedPortfolioIdentityNumber);
                    if (retrievedContact != null)
                    {
                        targetKYC.alt_RelatedPortfolioCustomerId = retrievedContact.ToEntityReference();
                    }
                    else
                    {
                        AccountDAL accountDAL = new AccountDAL(GlobalContext);
                        Account retrievedAccount = accountDAL.GetByAccountNumber(targetKYC.alt_RelatedPortfolioIdentityNumber);
                        if (retrievedAccount != null)
                        {
                            targetKYC.alt_RelatedPortfolioCustomerId = retrievedAccount.ToEntityReference();
                        }
                        else
                        {
                            targetKYC.alt_RelatedPortfolioCustomerId = null;
                        }
                    }
                }
                else
                {
                    targetKYC.alt_RelatedPortfolioCustomerId = null;
                }
            }
        }

        private int? GetValueScoreDestination(int[] currentValue, Sourcevalue[] sourceValues)
        {
            int? valueScoreDestination = null;
            this.GlobalContext.LogEntry();
            if (currentValue.Length > 1)
            {
                valueScoreDestination = sourceValues.Where(sv => sv.currentValue.HasValue
                && currentValue.Contains(sv.currentValue.Value) && sv.scoreDestination.HasValue).Max(sv => sv.scoreDestination);
            }
            else
            {
                valueScoreDestination = sourceValues.FirstOrDefault(item => item.currentValue == currentValue[0])?.scoreDestination;
            }
            return valueScoreDestination;
        }

        private void SetScoreInKYCPerType(AttributesMoneyLaunderingCalculator attributeMoneyLaunderingCalculator, int? scoreDestination, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            switch (attributeMoneyLaunderingCalculator.fieldTypeDestination)
            {
                case CrmPropertyType.Int:
                    this.SetScoreInKYCInt(scoreDestination, attributeMoneyLaunderingCalculator.fieldNameDestination, targetKYC);
                    break;
                case CrmPropertyType.OptionSet:
                    this.SetScoreInKYCOptionSet(scoreDestination, attributeMoneyLaunderingCalculator.fieldNameDestination, targetKYC);
                    break;
                default:
                    break;
            }
        }

        private int[] GetCurrentValuePerType(AttributesMoneyLaunderingCalculator attributeMoneyLaunderingCalculator, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            int[] currentValuePerType = null;
            switch (attributeMoneyLaunderingCalculator.dataSource.fieldTypeSource)
            {
                case CrmPropertyType.Int:
                    currentValuePerType = this.GetCurrentValueInt(attributeMoneyLaunderingCalculator.dataSource, targetKYC);
                    break;
                case CrmPropertyType.Bool:
                    currentValuePerType = this.GetCurrentValueBool(attributeMoneyLaunderingCalculator.dataSource, targetKYC);
                    break;
                case CrmPropertyType.OptionSet:
                    currentValuePerType = this.GetCurrentValueOptionSet(attributeMoneyLaunderingCalculator.dataSource, targetKYC);
                    break;
                case CrmPropertyType.OptionSetCollection:
                    currentValuePerType = this.GetCurrentValueOptionSetCollection(attributeMoneyLaunderingCalculator.dataSource, targetKYC);
                    break;
                case CrmPropertyType.EntityReference:
                default:
                    break;
            }
            return currentValuePerType;
        }

        private void SetScoreInKYCEntityReference(alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_EmploymentCategoryOccupationId))
            {
                OccupationDAL occupationDal = new OccupationDAL(this.GlobalContext);
                alt_Occupation occupation = occupationDal.Get(targetKYC.alt_EmploymentCategoryOccupationId.Id, new[] { alt_Occupation.Fields.alt_AccountHolderScoreInt, alt_Occupation.Fields.alt_AccountHolderRiskLevelCode });
                if (occupation.AttributeHasValue<int>(alt_Occupation.Fields.alt_AccountHolderScoreInt))
                {
                    targetKYC.alt_EmploymentCategoryScoreInt = occupation.alt_AccountHolderScoreInt;
                }
                if (occupation.AttributeHasValue<OptionSetValue>(alt_Occupation.Fields.alt_AccountHolderRiskLevelCode))
                {
                    targetKYC.alt_EmploymentCategoryLevelCode = occupation.alt_AccountHolderRiskLevelCode;
                }
            }
        }

        private void SetScoreInKYCInt(int? scoreDestination, string fieldNameDestination, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            targetKYC[fieldNameDestination] = (int)scoreDestination;
        }

        private void SetScoreInKYCOptionSet(int? scoreDestination, string fieldNameDestination, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            targetKYC[fieldNameDestination] = new OptionSetValue((int)scoreDestination);
        }

        private int[] GetCurrentValueOptionSetCollection(DataSource dataSource, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            int[] currentValueOptionSetCollection = null;
            if (targetKYC.AttributeHasValue<OptionSetValueCollection>(dataSource.fieldNameSource))
            {
                OptionSetValueCollection currentValuesOptionSetCollection = targetKYC.GetAttributeValue<OptionSetValueCollection>(dataSource.fieldNameSource);
                currentValueOptionSetCollection = currentValuesOptionSetCollection.Select(optionSetValue => optionSetValue.Value).ToArray();
            }
            return currentValueOptionSetCollection;
        }

        private int[] GetCurrentValueBool(DataSource dataSource, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            int[] currentValueBool = null;
            if (targetKYC.AttributeHasValue<bool>(dataSource.fieldNameSource))
            {
                currentValueBool = new int[] { Convert.ToInt32(targetKYC.GetAttributeValue<bool>(dataSource.fieldNameSource)) };
            }
            return currentValueBool;

        }

        private int[] GetCurrentValueOptionSet(DataSource dataSource, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            int[] currentValueOptionSet = null;
            if (targetKYC.AttributeHasValue<OptionSetValue>(dataSource.fieldNameSource))
            {
                currentValueOptionSet = new int[] { targetKYC.GetAttributeValue<OptionSetValue>(dataSource.fieldNameSource).Value };
            }
            return currentValueOptionSet;

        }

        private int[] GetCurrentValueInt(DataSource dataSource, alt_KYC targetKYC)
        {
            this.GlobalContext.LogEntry();
            int[] currentValueInt = null;
            if (targetKYC.AttributeHasValue<int>(dataSource.fieldNameSource))
            {
                currentValueInt = new int[] { targetKYC.GetAttributeValue<int>(dataSource.fieldNameSource) };
            }
            return currentValueInt;
        }

        private string GetJsonScoreDefinition()
        {
            this.GlobalContext.LogEntry();
            return GlobalContext.CacheManager.GetGlobalParameter<string>("MoneyLaunderingCalculatorKYCEntity");
        }

        public void HandlelScoresSectionInternalBit(alt_KYC targetKYC, alt_KYC preKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetKYC.Contains(alt_KYC.Fields.alt_ScoresSectionInternalBit))
            {
                bool isCreated = targetKYC.Equals(preKYC);
                alt_KYC mergedKYC = isCreated ? targetKYC : targetKYC.Merge(preKYC);

                if ((isCreated && targetKYC.Contains(alt_KYC.Fields.alt_ManualHandlingRequiredBit) && !targetKYC.alt_ManualHandlingRequiredBit.Value)
                    || !isCreated
                    )
                {
                    CreateRecordMoneyLaundering(mergedKYC, isCreated);
                    UpdateOrCreateAuthorizationManagement(mergedKYC);
                }
            }
        }

        private void CreateRecordMoneyLaundering(alt_KYC mergedKYC, bool isCreated)
        {
            this.GlobalContext.LogEntry();
            alt_MoneyLaunderingCalculation moneyLaunderingCalculationToCreate = new alt_MoneyLaunderingCalculation()
            {
                alt_KYCId = mergedKYC.ToEntityReference(),
                OwnerId = isCreated ? mergedKYC.OwnerId : mergedKYC.ModifiedBy,
            };
            MoneyLaunderingCalculationDAL moneyLaunderingCalculationDAL = new MoneyLaunderingCalculationDAL(this.GlobalContext);
            moneyLaunderingCalculationDAL.Create(moneyLaunderingCalculationToCreate);
        }

        public void UpdateOrCreateAuthorizationManagement(alt_KYC mergedKYC)
        {
            this.GlobalContext.LogEntry();
            int? CalculetedMoneyLaunderingLevelMax = this.GetCalculetedMoneyLaunderingLevelMaxForDigitalFormVerification(mergedKYC.alt_DigitalFormVerificationId.Id);
            //Get last createdon if Exsist AuthorizationManagement by DigitalFormVerificationId
            AuthorizationManagementDAL authorizationManagementDAL = new AuthorizationManagementDAL(this.GlobalContext);
            alt_AuthorizationManagement lastAuthorizationManagement = authorizationManagementDAL.GetLastCreatedOnAuthorizationManagementByDigitalFormVerificationId(mergedKYC.alt_DigitalFormVerificationId.Id);
            alt_AuthorizationManagement authorizationManagementToUpdate = new alt_AuthorizationManagement()
            {
                alt_SignerNameSystemUserId = mergedKYC.ModifiedBy,
            };

            if (CalculetedMoneyLaunderingLevelMax != null)
            {
                authorizationManagementToUpdate.alt_CapitalRiskLevelAccountCode = new OptionSetValue((int)CalculetedMoneyLaunderingLevelMax);
            }
            if (lastAuthorizationManagement == null)
            {
                TeamDAL teamDAL = new TeamDAL(this.GlobalContext);
                int teamCode = JsonSerializer.Deserialize<Dictionary<string, int>>(GlobalContext.CacheManager.GetGlobalParameter<string>("TeamsCodes"))["OperationalControl"];
                EntityReference teamId = new EntityReference(Team.EntityLogicalName, teamDAL.GetFirstOrDefaultByAttribute(Team.Fields.alt_TeamCodeInt, teamCode, new string[] { Team.Fields.Id }).Id);
                authorizationManagementToUpdate.alt_ControlStageTeamId = teamId;
                authorizationManagementToUpdate.OwnerId = teamId;
                authorizationManagementToUpdate.alt_DigitalFormVerificationId = new EntityReference(alt_DigitalFormVerification.EntityLogicalName, mergedKYC.alt_DigitalFormVerificationId.Id);
                authorizationManagementToUpdate.alt_SignatureDate = DateTime.UtcNow;

                Guid authorizationManagementId = authorizationManagementDAL.Create(authorizationManagementToUpdate);

                alt_AuthorizationManagement authorizationManagementUpdate = new alt_AuthorizationManagement()
                {
                    Id = authorizationManagementId,
                    StateCode = alt_AuthorizationManagementState.Inactive,
                    StatusCode = new OptionSetValue((int)AuthorizationManagementStatusCode.Inactive)
                };
                authorizationManagementDAL.Update(authorizationManagementUpdate);
            }
            else
            {
                authorizationManagementToUpdate.alt_AuthorizationManagementId = lastAuthorizationManagement.alt_AuthorizationManagementId;
                authorizationManagementDAL.Update(authorizationManagementToUpdate);
            }
        }

        private int? GetCalculetedMoneyLaunderingLevelMaxForDigitalFormVerification(Guid digitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();
            int? moneyLaunderingLevelMax = null;
            //1. Get all kyc linked accountHolder by DigitalFormVerification
            KYCDAL KYCDal = new KYCDAL(this.GlobalContext);
            List<alt_KYC> recordsKYC = KYCDal.GetActiveAccountHolderKYCsByAccountHolderTypeAndDigitalFormVerificationId(digitalFormVerificationId, new int[] { (int)AccountHolderTypeCode.Owner }, new[] { alt_KYC.Fields.CreatedOn, alt_KYC.Fields.alt_AccountHolderId });
            if (recordsKYC.Count > 0)
            {
                IEnumerable<alt_KYC> groupKYCByAccountHolder = recordsKYC.GroupBy(record => record.alt_AccountHolderId.Id).Select(group => group.OrderByDescending(record => record.CreatedOn).FirstOrDefault());
                List<Guid> guidsKYCId = groupKYCByAccountHolder.Select(record => record.alt_KYCId.Value).ToList();

                //2. Get all MoneyLaunderingCalculation by kyc laster createOn
                MoneyLaunderingCalculationDAL moneyLaunderingCalculationDAL = new MoneyLaunderingCalculationDAL(this.GlobalContext);
                List<alt_MoneyLaunderingCalculation> recordsMoneyLaunderingCalculation = moneyLaunderingCalculationDAL.GetAllKYCLinkedAccountHolderByDigitalFormVerificationId(guidsKYCId);
                IEnumerable<alt_MoneyLaunderingCalculation> groupMoneyLaunderingCalculationByKYC = recordsMoneyLaunderingCalculation.GroupBy(record => record.alt_KYCId.Id).Select(group => group.OrderByDescending(record => record.CreatedOn).FirstOrDefault());
                moneyLaunderingLevelMax = groupMoneyLaunderingCalculationByKYC.Max(record => record.AttributeHasValue<OptionSetValue>(alt_MoneyLaunderingCalculation.Fields.alt_CalculetedMoneyLaunderingLevelCode) ? record.alt_CalculetedMoneyLaunderingLevelCode.Value : (int?)null);
            }
            return moneyLaunderingLevelMax;
        }

        public void UpdateDigitalFormVerification(alt_KYC targetKYC, alt_KYC preKYC)
        {
            this.GlobalContext.LogEntry();

            alt_KYC mergedKYC = targetKYC.Merge(preKYC);

            DigitalFormVerificationDAL DigitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
            alt_DigitalFormVerification digitalFormVerification = new alt_DigitalFormVerification() { alt_DigitalFormVerificationId = mergedKYC.alt_DigitalFormVerificationId.Id };
            digitalFormVerification.alt_VerifiedKYCForStageJoiningControlBit = IsAllRequiredFieldsContainValue(mergedKYC) ? IsAllRequiredFieldsContainValueForAllKYC(mergedKYC.alt_DigitalFormVerificationId.Id) : false;

            if (IsUpdateDigitalFormVerification(digitalFormVerification, DigitalFormVerificationDAL))
            {
                DigitalFormVerificationDAL.Update(digitalFormVerification);
            }
        }

        private bool IsUpdateDigitalFormVerification(alt_DigitalFormVerification digitalFormVerification, DigitalFormVerificationDAL digitalFormVerificationDAL)
        {
            this.GlobalContext.LogEntry();
            string[] columns = new[] { alt_DigitalFormVerification.Fields.alt_VerifiedKYCForStageJoiningControlBit };
            alt_DigitalFormVerification digitalFormVerificationRetrieve = digitalFormVerificationDAL.Get(Guid.Parse(digitalFormVerification.alt_DigitalFormVerificationId.ToString()), columns);
            return digitalFormVerificationRetrieve.alt_VerifiedKYCForStageJoiningControlBit != digitalFormVerification.alt_VerifiedKYCForStageJoiningControlBit ? true : false;
        }

        private bool IsAllRequiredFieldsContainValueForAllKYC(Guid digitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();
            KYCDAL KYCDal = new KYCDAL(this.GlobalContext);
            List<alt_KYC> recordsKYC = KYCDal.GetActiveAccountHolderKYCsByAccountHolderTypeAndDigitalFormVerificationId(digitalFormVerificationId, new int[] { (int)AccountHolderTypeCode.Owner });
            return recordsKYC.Count > 1 && recordsKYC.Any(KYC => !IsAllRequiredFieldsContainValue(KYC)) ? false : true;
        }

        private bool IsAllRequiredFieldsContainValue(alt_KYC mergedKYC)
        {
            this.GlobalContext.LogEntry();
            return IsAllRequiredQuestionsForAccountOwner(mergedKYC)
                && IsAllRequiredAccountLevelQuestions(mergedKYC);
        }

        private bool IsAllRequiredAccountLevelQuestions(alt_KYC mergedKYC)
        {
            this.GlobalContext.LogEntry();
            int[] fundsSourceRequired = { (int)FundsSourceCode.Inheritance, (int)FundsSourceCode.Gift, (int)FundsSourceCode.Else };
            return (mergedKYC.alt_FundsSourceCode == null
                    || ((mergedKYC.alt_FundsSourcePrivate != null
                            || !mergedKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceRequired.Any(v => v == fundsSourceItem.Value)))
                        && (mergedKYC.alt_FundsSourceFinancial != null
                            || !mergedKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceItem.Value == (int)FundsSourceCode.ForeignTerritory))
                        && (mergedKYC.alt_FundsSourceScoreInt != null
                            || !mergedKYC.alt_FundsSourceCode.Any(fundsSourceItem => fundsSourceItem.Value == (int)FundsSourceCode.Else))));
        }

        private bool IsAllRequiredQuestionsForAccountOwner(alt_KYC mergedKYC)
        {
            this.GlobalContext.LogEntry();
            bool isAllRequiredAccountOwner = true;
            if ((mergedKYC.alt_EmploymentTypeCode != null
                    && ((mergedKYC.alt_EmploymentTypeCode.Value == ((int)EmploymentTypeCode.Independent)
                            || mergedKYC.alt_EmploymentTypeCode.Value == ((int)EmploymentTypeCode.CompanyOwner))
                        && mergedKYC.alt_BusinessName == null))
                || (mergedKYC.alt_TradeRelationRiskTerritoryBit.Value
                    && (mergedKYC.alt_TradeRelationDesc == null
                        || !mergedKYC.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_TradeRelationRiskCountryId)))
                || (mergedKYC.alt_PublicPersonBit.Value
                    && (mergedKYC.alt_PublicPersonRole == null
                        || mergedKYC.alt_RelationToPublicPerson == null
                        || mergedKYC.alt_FinancialResourceSource == null)))
            {
                isAllRequiredAccountOwner = false;
            }

            if (isAllRequiredAccountOwner && mergedKYC.alt_EmploymentCategoryOccupationId != null && mergedKYC.alt_EmploymentCategoryDesc == null)
            {
                this.GlobalContext.LogEntry();
                int otheOccupationCode = GlobalContext.CacheManager.GetGlobalParameter<int>("OtheOccupationCode");
                OccupationDAL occupationDal = new OccupationDAL(this.GlobalContext);
                alt_Occupation occupation = occupationDal.Get(mergedKYC.alt_EmploymentCategoryOccupationId.Id, new[] { alt_Occupation.Fields.alt_CodeInt });
                if (occupation.alt_CodeInt.Value == otheOccupationCode)
                {
                    isAllRequiredAccountOwner = false;
                }
            }
            return isAllRequiredAccountOwner;
        }
    }
}