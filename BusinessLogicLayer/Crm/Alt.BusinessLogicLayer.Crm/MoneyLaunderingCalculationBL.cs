using Alt.DataAccessLayer.Crm;
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
    public class MoneyLaunderingCalculationBL : CrmBaseBL
    {
        private readonly string[] sourceFieldsInKYCForMoneyLaunderingCalculation = {
            alt_KYC.Fields.alt_EmploymentScoreInt,
            alt_KYC.Fields.alt_EmploymentCategoryScoreInt,
            alt_KYC.Fields.alt_BankingServiceDenialScoreInt,
            alt_KYC.Fields.alt_MonthlyIncomeScoreInt,
            alt_KYC.Fields.alt_ScorePublicFigureInt,
            alt_KYC.Fields.alt_FundsSourceScoreInt,
            alt_KYC.Fields.alt_TotalWithdrawalOrTransferForecastScoreInt,
            alt_KYC.Fields.alt_TotalWithdrawalTransferForecastScoreInt,
            alt_KYC.Fields.alt_FundsDepositFrequencyForecastScoreInt,
            alt_KYC.Fields.alt_TotalDepositForecastPerYearScoreInt,
            alt_KYC.Fields.alt_TradeRelationRiskScoreInt
        };
        private readonly string[] sourceFieldsInKYCForMoneyLaunderingCalculationLevel = {
            alt_KYC.Fields.alt_TradeRelationRiskLevelCode,
            alt_KYC.Fields.alt_EmploymentCategoryLevelCode,
            alt_KYC.Fields.alt_PublicPersonLevelCode,
            alt_KYC.Fields.alt_FundsSourceLevelCode,
            alt_KYC.Fields.alt_FundsDepositFrequencyForecastLevelCode,
        };

        public MoneyLaunderingCalculationBL(GlobalContext globalContext) : base(globalContext) { }

        public void PopulateFieldsForMoneyLaunderingCalcultion(alt_MoneyLaunderingCalculation targetMoneyLaunderingCalculation)
        {
            this.GlobalContext.LogEntry();
            alt_KYC relatedKYC = GetRelatedKYCFieldsForMoneyLaunderingCalculation(targetMoneyLaunderingCalculation.alt_KYCId, sourceFieldsInKYCForMoneyLaunderingCalculation, sourceFieldsInKYCForMoneyLaunderingCalculationLevel);
            PopulateFieldsDirectly(targetMoneyLaunderingCalculation, relatedKYC);
            HandleFieldsForMoneyLaunderingScoreCalculations(targetMoneyLaunderingCalculation, relatedKYC);
        }

        private void PopulateFieldsDirectly(alt_MoneyLaunderingCalculation targetMoneyLaunderingCalculation, alt_KYC relatedKYC)
        {
            targetMoneyLaunderingCalculation.alt_CalculationSystemUserId = targetMoneyLaunderingCalculation.ModifiedBy;
            targetMoneyLaunderingCalculation.alt_CalculationExecutionDate = DateTime.UtcNow;
            targetMoneyLaunderingCalculation.alt_DigitalFormVerificationId = relatedKYC.alt_DigitalFormVerificationId;
        }

        private void HandleFieldsForMoneyLaunderingScoreCalculations(alt_MoneyLaunderingCalculation targetMoneyLaunderingCalculation, alt_KYC relatedKYC)
        {
            this.GlobalContext.LogEntry();
            if (targetMoneyLaunderingCalculation.AttributeHasValue<EntityReference>(alt_KYC.Fields.alt_KYCId))
            {
                targetMoneyLaunderingCalculation.alt_MoneyLaunderingScoreInt = CalculateMoneyLaunderingScore(relatedKYC);
                int? calculateMoneyLaunderingCalculatedScore = CalculateMoneyLaunderingCalculatedScore(relatedKYC);
                if (calculateMoneyLaunderingCalculatedScore != null)
                {
                    targetMoneyLaunderingCalculation.alt_MoneyLaunderinglevelCode = new OptionSetValue((int)calculateMoneyLaunderingCalculatedScore);
                }
                targetMoneyLaunderingCalculation.alt_CalculetedMoneyLaunderingLevelCode = new OptionSetValue(
                    CalculateMoneyLaunderingDistinctScore(targetMoneyLaunderingCalculation.alt_MoneyLaunderingScoreInt, targetMoneyLaunderingCalculation.alt_MoneyLaunderinglevelCode));
            }
        }

        private int? CalculateMoneyLaunderingScore(alt_KYC relatedKYC)
        {
            this.GlobalContext.LogEntry();
            return sourceFieldsInKYCForMoneyLaunderingCalculation
                    .Where(sourceField => relatedKYC.GetAttributeValue<int?>(sourceField) != null)
                    .Sum(sourceField => relatedKYC.GetAttributeValue<int>(sourceField));
        }

        private int? CalculateMoneyLaunderingCalculatedScore(alt_KYC relatedKYC)
        {
            this.GlobalContext.LogEntry();
            int? calculationMoneyLaunderinglevel = 0;

            foreach (string sourceField in sourceFieldsInKYCForMoneyLaunderingCalculationLevel)
            {
                if (calculationMoneyLaunderinglevel == (int)CalculetedMoneyLaunderingLevelCode.High)
                {
                    break;
                }
                else if (relatedKYC.GetAttributeValue<OptionSetValue>(sourceField) != null)
                {
                    calculationMoneyLaunderinglevel = (calculationMoneyLaunderinglevel < relatedKYC.GetAttributeValue<OptionSetValue>(sourceField).Value)
                        ? relatedKYC.GetAttributeValue<OptionSetValue>(sourceField).Value
                        : calculationMoneyLaunderinglevel;
                }
            }

            return calculationMoneyLaunderinglevel == 0 ? null : calculationMoneyLaunderinglevel;
        }

        private int CalculateMoneyLaunderingDistinctScore(int? alt_MoneyLaunderingScoreInt, OptionSetValue alt_MoneyLaunderinglevelCode)
        {
            this.GlobalContext.LogEntry();
            int calculetedMoneyLaunderingLevelCode = (int)CalculetedMoneyLaunderingLevelCode.Low;
            if (alt_MoneyLaunderingScoreInt != null)
            {
                Dictionary<string, int> moneyLaunderingScore = JsonSerializer.Deserialize<Dictionary<string, int>>(GlobalContext.CacheManager.GetGlobalParameter<string>("MoneyLaunderingScore"));
                calculetedMoneyLaunderingLevelCode = alt_MoneyLaunderingScoreInt > moneyLaunderingScore["High"]
                    ? (int)CalculetedMoneyLaunderingLevelCode.High
                    : alt_MoneyLaunderingScoreInt > moneyLaunderingScore["Medium"]
                        ? (int)CalculetedMoneyLaunderingLevelCode.Medium
                        : (int)CalculetedMoneyLaunderingLevelCode.Low;
            }
            if (alt_MoneyLaunderinglevelCode != null)
            {
                calculetedMoneyLaunderingLevelCode = calculetedMoneyLaunderingLevelCode > alt_MoneyLaunderinglevelCode.Value
                    ? calculetedMoneyLaunderingLevelCode
                    : alt_MoneyLaunderinglevelCode.Value;
            }
            return calculetedMoneyLaunderingLevelCode;
        }

        private alt_KYC GetRelatedKYCFieldsForMoneyLaunderingCalculation(EntityReference alt_KYCId, string[] sourceFieldsForMoneyLaunderingCalculation, string[] sourceFieldsInKYCForMoneyLaunderingCalculationLevel)
        {
            this.GlobalContext.LogEntry();

            string[] fieldsForRetrieve = sourceFieldsForMoneyLaunderingCalculation
                .Concat(sourceFieldsInKYCForMoneyLaunderingCalculationLevel)
                .Concat(new string[] { alt_KYC.Fields.alt_DigitalFormVerificationId }).ToArray();
            KYCDAL kYCDAL = new KYCDAL(this.GlobalContext);
            alt_KYC relatedKYC = kYCDAL.Get(alt_KYCId.Id, fieldsForRetrieve);
            return relatedKYC;
        }
    }
}
