using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.Utils;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class DepositBL : CrmBaseBL
    {
        private string bankNumberConfigurationGlobalParameter = "BankNumberConfiguration";
        private string minimumDepositRequiredGlobalParameter = "MinimumDepositRequired";

        public DepositBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleRelatedEntities(alt_Deposit targetDeposit)
        {
            this.GlobalContext.LogEntry();

            if (targetDeposit.AttributeHasValue<string>(alt_Deposit.Fields.alt_OpposingAccountNumber)
                && targetDeposit.AttributeHasValue<string>(alt_Deposit.Fields.alt_OpposingBankNumber)
                && targetDeposit.AttributeHasValue<string>(alt_Deposit.Fields.alt_OpposingBranchNumber))
            {
                BankNumberConfiguration bankNumberConfiguration =
                    JsonUtils.Deserialize<BankNumberConfiguration>(GlobalContext.CacheManager.GetGlobalParameter<string>(bankNumberConfigurationGlobalParameter));

                string trimmedOpposingAccountNumber = StringUtils.SafeTrimStartingZeros(targetDeposit.alt_OpposingAccountNumber);
                string trimmedOpposingBranchNumber = StringUtils.SafeTrimStartingZeros(targetDeposit.alt_OpposingBranchNumber);
                string trimmedOpposingBankNumber = StringUtils.SafeTrimStartingZeros(targetDeposit.alt_OpposingBankNumber);
                string trimmedBankNumber = StringUtils.SafeTrimStartingZeros(targetDeposit.alt_BankNumber);
                targetDeposit.alt_CRMBankNumber = GetMappedBankNumber(trimmedBankNumber, bankNumberConfiguration);
                targetDeposit.alt_CRMOppositeBankNumber = GetMappedBankNumber(trimmedOpposingBankNumber, bankNumberConfiguration);

                DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                List<alt_DigitalFormVerification> retrievedDigitalFormVerifications = digitalFormVerificationDAL.GetDigitalFormVerificationsByBankDetails(
                    targetDeposit.alt_CRMOppositeBankNumber,
                    trimmedOpposingBranchNumber
                );
                List<alt_DigitalFormVerification> matchingDigitalFormVerifications = retrievedDigitalFormVerifications
                    .Where(e =>
                        IsMatchingDigitalFormVerification(
                            e,
                            trimmedOpposingAccountNumber
                        )
                    )
                    .ToList();

                PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
                List<alt_Portfolio> retrievedPortfolios = portfolioDAL.GetPortfoliosByBankDetails(
                    targetDeposit.alt_CRMOppositeBankNumber,
                    trimmedOpposingBranchNumber
                );
                List<alt_Portfolio> matchingPortfolios = retrievedPortfolios
                    .Where(e =>
                        IsMatchingPortfolio(
                            e,
                            trimmedOpposingAccountNumber
                        )
                    )
                    .ToList();

                this.HandleMatchDigitalFormVerification(targetDeposit, matchingDigitalFormVerifications);
                this.HandleMatchPortfolio(targetDeposit, matchingPortfolios);

                if (IsDigitalFormVerificationValidForShenhavAutomaticAccountCreate(targetDeposit, matchingDigitalFormVerifications))
                {
                    targetDeposit.alt_AutomaticLaunchedShenhavPortfolioBit = true;
                    targetDeposit.alt_AutomaticLaunchShenhavPortfolioDate = DateTime.Now.Date;
                }
            }
        }

        public void HandleRelatedDigitalFromVerificationUpdate(alt_Deposit targetDeposit)
        {
            this.GlobalContext.LogEntry();

            if (targetDeposit.AttributeHasValue<bool>(alt_Deposit.Fields.alt_AutomaticLaunchedShenhavPortfolioBit)
                && targetDeposit.alt_AutomaticLaunchedShenhavPortfolioBit.Value)
            {
                this.UpdateRelatedDigitalFormVerification(targetDeposit);
            }
        }

        private void HandleMatchDigitalFormVerification(alt_Deposit targetDeposit, List<alt_DigitalFormVerification> matchingDigitalFormVerifications)
        {
            this.GlobalContext.LogEntry();

            if (matchingDigitalFormVerifications == null || matchingDigitalFormVerifications.Count == 0)
            {
                targetDeposit.alt_MatchForDigitalFormVerificationCode = new OptionSetValue((int)MatchForDigitalFormVerificationCode.No);
            }
            else
            {
                alt_DigitalFormVerification lastCreatedDigitalFormVerification = matchingDigitalFormVerifications.FirstOrDefault();

                targetDeposit.alt_MatchForDigitalFormVerificationCode = matchingDigitalFormVerifications.Count == 1
                    ? targetDeposit.alt_MatchForDigitalFormVerificationCode = new OptionSetValue((int)MatchForDigitalFormVerificationCode.Yes)
                    : targetDeposit.alt_MatchForDigitalFormVerificationCode = new OptionSetValue((int)MatchForDigitalFormVerificationCode.MoreThanOne);

                targetDeposit.alt_DigitalFormVerificationId = lastCreatedDigitalFormVerification.ToEntityReference();
                targetDeposit.alt_DigitalFormNumber = lastCreatedDigitalFormVerification.alt_DigitalFormNumber;
                targetDeposit.alt_DigitalFormVerificationStatusCode = lastCreatedDigitalFormVerification.alt_FormStatusCode;

                targetDeposit.alt_MainAccountHolder =
                    lastCreatedDigitalFormVerification.GetAliasedAttributeValue<string>(DigitalFormVerificationDAL.mainAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_Name);

                targetDeposit.alt_MainAccountHolderIdentificationNumber =
                    lastCreatedDigitalFormVerification.GetAliasedAttributeValue<string>(DigitalFormVerificationDAL.mainAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_IdentificationNumber);

                targetDeposit.alt_BeneficiaryAccountHolder =
                    lastCreatedDigitalFormVerification.GetAliasedAttributeValue<string>(DigitalFormVerificationDAL.beneficiaryAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_Name);

                if (matchingDigitalFormVerifications.Count > 1)
                {
                    alt_DigitalFormVerification firstCreatedDigitalFormVerification = matchingDigitalFormVerifications.Last();
                    targetDeposit.alt_FirstCreatedDigitalFormVerificationId = firstCreatedDigitalFormVerification.ToEntityReference();
                    targetDeposit.alt_FirstCreatedDigitalFormNumber = firstCreatedDigitalFormVerification.alt_DigitalFormNumber;
                }
            }
        }

        private void HandleMatchPortfolio(alt_Deposit targetDeposit, List<alt_Portfolio> matchingPortfolios)
        {
            this.GlobalContext.LogEntry();

            if (matchingPortfolios == null || matchingPortfolios.Count == 0)
            {
                targetDeposit.alt_MatchForPortfolioCode = new OptionSetValue((int)MatchForDigitalFormVerificationCode.No);
            }
            else
            {
                alt_Portfolio lastCreatedPortfolio = matchingPortfolios.FirstOrDefault();

                targetDeposit.alt_MatchForPortfolioCode = matchingPortfolios.Count == 1
                    ? targetDeposit.alt_MatchForPortfolioCode = new OptionSetValue((int)MatchForPortfolioCode.Yes)
                    : targetDeposit.alt_MatchForPortfolioCode = new OptionSetValue((int)MatchForPortfolioCode.MoreThanOne);

                targetDeposit.alt_PortfolioId = lastCreatedPortfolio.ToEntityReference();
                targetDeposit.alt_JoiningProcessNumber = lastCreatedPortfolio.alt_JoiningProcessNumber;
                targetDeposit.alt_ShenhavAccountNumber = lastCreatedPortfolio.alt_ShenhavAccountNumber;
                targetDeposit.alt_ShenhavStatusCode = lastCreatedPortfolio.alt_ShenhavStatusCode;

                targetDeposit.alt_MainAccountHolder =
                    lastCreatedPortfolio.GetAliasedAttributeValue<string>(PortfolioDAL.mainAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_Name);

                targetDeposit.alt_MainAccountHolderIdentificationNumber =
                    lastCreatedPortfolio.GetAliasedAttributeValue<string>(PortfolioDAL.mainAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_IdentificationNumber);

                targetDeposit.alt_BeneficiaryAccountHolder =
                    lastCreatedPortfolio.GetAliasedAttributeValue<string>(PortfolioDAL.beneficiaryAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_Name);

                if (matchingPortfolios.Count > 1)
                {
                    alt_Portfolio firstCreatedPortolio = matchingPortfolios.Last();
                    targetDeposit.alt_FirstCreatedPortfolioId = firstCreatedPortolio.ToEntityReference();
                    targetDeposit.alt_FirstCreatedShenhavAccountNumber = firstCreatedPortolio.alt_ShenhavAccountNumber;
                    targetDeposit.alt_FirstCreatedPortfolioShenhavStatusCode = firstCreatedPortolio.alt_ShenhavStatusCode;
                }
            }
        }

        private string GetMappedBankNumber(string bankNumber, BankNumberConfiguration mapper)
        {
            this.GlobalContext.LogEntry();

            string mappedBankNumber = bankNumber;
            if (mapper != null && !string.IsNullOrWhiteSpace(bankNumber)
                && mapper.Mapping != null && mapper.Mapping.TryGetValue(bankNumber, out string mappingResult))
            {
                mappedBankNumber = mappingResult;
                if (mapper.Consolidation != null && mapper.Consolidation.TryGetValue(mappedBankNumber, out string consolidationResult))
                {
                    mappedBankNumber = consolidationResult;
                }

            }

            return mappedBankNumber;
        }

        private bool IsMatchingDigitalFormVerification(alt_DigitalFormVerification dfv, string trimmedAccount)
        {
            string dbAccount = dfv.alt_BankAccountNumber ?? string.Empty;
            if (StringUtils.SafeTrimStartingZeros(dbAccount) != trimmedAccount)
                return false;

            return true;
        }

        private bool IsMatchingPortfolio(alt_Portfolio portfolio, string trimmedAccount)
        {
            var accountDetailsAliased = portfolio.GetAttributeValue<AliasedValue>($"{PortfolioDAL.bankAccountDetailsAlias}.{alt_BankAccountDetails.Fields.alt_Name}");
            string dbAccount = accountDetailsAliased?.Value?.ToString() ?? string.Empty;
            if (StringUtils.SafeTrimStartingZeros(dbAccount) != trimmedAccount)
                return false;

            return true;
        }

        private bool IsDigitalFormVerificationValidForShenhavAutomaticAccountCreate(alt_Deposit targetDeposit, List<alt_DigitalFormVerification> digitalFormVerifications)
        {
            this.GlobalContext.LogEntry();

            bool isValidForUpdate = false;
            decimal minimumDepositRequired = this.GlobalContext.CacheManager.GetGlobalParameter<decimal>(minimumDepositRequiredGlobalParameter);

            this.GlobalContext.Log.Info($"amount bigger: {targetDeposit.alt_DepositAmountDcml >= minimumDepositRequired}");

            if (targetDeposit.alt_DepositAmountDcml >= minimumDepositRequired)
            {
                targetDeposit.alt_DepositAmountBelow5000Bit = false;

                if (targetDeposit.alt_MatchForPortfolioCode.Value == (int)MatchForPortfolioCode.No)
                {
                    if (targetDeposit.alt_MatchForDigitalFormVerificationCode.Value == (int)MatchForDigitalFormVerificationCode.Yes)
                    {
                        alt_DigitalFormVerification firstDigitalFormVerification = digitalFormVerifications.FirstOrDefault();
                        isValidForUpdate = IsDigitalFormVerificationValidForUpdate(firstDigitalFormVerification);
                    }
                    else if (targetDeposit.alt_MatchForDigitalFormVerificationCode.Value == (int)MatchForDigitalFormVerificationCode.MoreThanOne)
                    {
                        AccountHolderDAL accountHolderDAL = new AccountHolderDAL(this.GlobalContext);
                        Guid[] digitalFormVerificationsIds = digitalFormVerifications.Select(digitalFormVerification => digitalFormVerification.Id).ToArray();
                        List<alt_AccountHolder> allAccountHolders = accountHolderDAL.GetAllAccountHoldersByDigitalFormVerificationIds(digitalFormVerificationsIds);
                        isValidForUpdate = IsAllDigitalFormVerificationsValidForUpdate(digitalFormVerifications, allAccountHolders);
                    }
                }
            }
            else
            {
                targetDeposit.alt_DepositAmountBelow5000Bit = true;
                isValidForUpdate = false;
            }

            return isValidForUpdate;
        }

        private bool IsDigitalFormVerificationValidForUpdate(alt_DigitalFormVerification digitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            int retrievedControlStageCode = digitalFormVerification.GetAliasedAttributeValue<int>(Team.EntityLogicalName, Team.Fields.alt_TeamCodeInt);
            bool isOperationalControlTeam = (int)TeamCodes.OperationalControlTeam == retrievedControlStageCode;
            bool isDigitalFormVerificationAwaitingForDeposit = digitalFormVerification.alt_FormStatusCode.Value == (int)FormStatusCode.AwaitingForDeposit;
            bool hasBeneficiery = !string.IsNullOrEmpty(digitalFormVerification.GetAliasedAttributeValue<string>(DigitalFormVerificationDAL.beneficiaryAccountHolderEntityAlias, alt_AccountHolder.Fields.alt_Name));
            return isOperationalControlTeam && isDigitalFormVerificationAwaitingForDeposit && hasBeneficiery;
        }

        private bool IsAllDigitalFormVerificationsValidForUpdate(List<alt_DigitalFormVerification> digitalFormVerifications, List<alt_AccountHolder> allAccountHolders)
        {
            this.GlobalContext.LogEntry();
            bool allDigitalFormVerificationsValid = true;

            var grouped = digitalFormVerifications.ToDictionary(
                dfv => dfv.Id,
                dfv => allAccountHolders
                    .Where(ah => ah.GetAttributeValue<EntityReference>(alt_AccountHolder.Fields.alt_DigitalFormVerificationId).Id == dfv.Id)
                    .ToList()
            );

            foreach (var keyValuePair in grouped)
            {
                if (keyValuePair.Value.Count == 0)
                {
                    return allDigitalFormVerificationsValid = false;
                }
            }

            // step 1
            int expectedAccountHolderCount = grouped.First().Value.Count;
            foreach (var keyValuePair in grouped)
            {
                if (keyValuePair.Value.Count != expectedAccountHolderCount)
                {
                    allDigitalFormVerificationsValid = false;
                }
            }

            // step 2
            foreach (var keyValuePair in grouped)
            {
                var dfvId = keyValuePair.Key;
                var accountHolders = keyValuePair.Value;
                bool hasRequiredType = accountHolders.Any(ah => ah.GetAttributeValue<OptionSetValue>(alt_AccountHolder.Fields.alt_AccountHolderTypeCode)
                    ?.Value == (int)AccountHolderTypeCode.Beneficiary);

                if (!hasRequiredType)
                {
                    allDigitalFormVerificationsValid = false;
                }
            }

            // step 3
            Dictionary<Guid, HashSet<(int status, string name)>> signatures = new Dictionary<Guid, HashSet<(int, string)>>();
            foreach (var dfv in digitalFormVerifications)
            {
                var children = grouped[dfv.Id];
                var set = new HashSet<(int, string)>();

                foreach (var ah in children)
                {
                    int status = ah.GetAttributeValue<OptionSetValue>(alt_AccountHolder.Fields.alt_AccountHolderTypeCode).Value;
                    string name = ah.GetAttributeValue<string>(alt_AccountHolder.Fields.alt_IdentificationNumber);
                    set.Add((status, name));
                }

                signatures[dfv.Id] = set;
            }

            var referenceSignature = signatures.First().Value;
            foreach (var sig in signatures.Values)
            {
                if (!sig.SetEquals(referenceSignature))
                {
                    allDigitalFormVerificationsValid = false;
                }
            }

            return allDigitalFormVerificationsValid;
        }

        private void UpdateRelatedDigitalFormVerification(alt_Deposit targetDeposit)
        {
            this.GlobalContext.LogEntry();

            Guid digitalFormVerificationId = targetDeposit.alt_DigitalFormVerificationId.Id;

            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
            alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification()
            {
                Id = digitalFormVerificationId,
                alt_InitialDepositCode = new OptionSetValue((int)InitialDepositCode.AcceptedDeposit),
                alt_DepositAmountDcml = targetDeposit.alt_DepositAmountDcml,
                alt_AutomaticLaunchedShenhavPortfolioBit = true
            };
            digitalFormVerificationDAL.Update(digitalFormVerificationToUpdate);
        }
    }
}
