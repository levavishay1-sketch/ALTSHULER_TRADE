using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBDigitalFormVerificationDAL : ExternalServicesBaseDAL<ESBJoiningForm, ApiDigitalFormVerification>
    {
        string yes = ShenhavYesNoCode.Yes.GetDescriptionAttribute();
        string no = ShenhavYesNoCode.No.GetDescriptionAttribute();
        string dateFormat = "yyyy-MM-dd";

        public ESBDigitalFormVerificationDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        public ActionResult CreatePortfolioInSheinav(ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            if (this.ApiConfiguration.UseOutgoingObjectValidationBit != null
                && this.ApiConfiguration.UseOutgoingObjectValidationBit.Value)
            {

                return this.ExecuteRequestWithObjectValidation(apiDigitalFormVerification);
            }
            else
            {
                return base.ExecuteRequest(apiDigitalFormVerification);
            }
        }

        private ActionResult ExecuteRequestWithObjectValidation(ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            ActionResult dalActionResult;

            ESBJoiningForm targetModel = this.MapApiEntityToTargetModel(apiDigitalFormVerification);
            if (targetModel.ValidateDataModel())
            {
                dalActionResult = this.BuildAndSendRequestHandler(HttpMethod.Post, targetModel);
            }
            else
            {
                string error = $"Invalid {targetModel.GetType()} model: {Environment.NewLine}{string.Join(Environment.NewLine, targetModel.DataModelValidationErrors)}";
                this.GlobalContext.Log.Error(error);
                dalActionResult = new ActionResult();
                dalActionResult.SetToFailedActionResult(error);
            }
            return dalActionResult;
        }

        protected override ESBJoiningForm MapApiEntityToTargetModel(ApiDigitalFormVerification apiEntity)
        {
            this.GlobalContext.LogEntry();

            var accountOwner = apiEntity.AccountHolders
                .Where(a => a.MainAccountHolder != null && a.MainAccountHolder.Value == true).FirstOrDefault();
            var authorizationManagement = apiEntity.AuthorizationManagements?.OrderByDescending(a => a.CreatedOn)?.FirstOrDefault();
            ESBJoiningForm joiningForm = new ESBJoiningForm()
            {
                Body = new ESBPortfolioBody()
                {
                    General = this.GenerateGeneral(apiEntity, accountOwner, authorizationManagement),
                    AccountBeneficiaries = this.GenerateAccountBeneficiaries(apiEntity),
                    AccountEntitlements = this.GenerateAccountEntitlements(apiEntity, authorizationManagement),
                    AccountFrames = this.GenerateAccountFrames(authorizationManagement),
                    AccountDepositsWithdrawals = this.GenerateAccountDepositsWithdrawals(apiEntity)
                }
            };
            return joiningForm;
        }

        private ESBPortfolioGeneral GenerateGeneral(ApiDigitalFormVerification apiEntity, ApiAccountHolder accountOwner, ApiAuthorizationManagement authorizationManagement)
        {
            this.GlobalContext.LogEntry();

            ESBPortfolioGeneral general = new ESBPortfolioGeneral(PortfolioActionTypeCode.New)
            {
                AccountNumber = null,
                AccountType = PortfolioTypeCode.Private.GetDescriptionAttribute(),
                AccountTypeSub = Enum.GetName(typeof(PortfolioTypeSubCode), PortfolioTypeSubCode.Individual),
                AccountTypeTase = apiEntity.AccountClassificationCode,
                IsLimitedAccountList = no,
                IsTaxCalcIgnore = no,
                Country = this.ConvertStringToInt(accountOwner?.Country?.Code),
                City = accountOwner?.City?.Code,
                Street = accountOwner?.Street?.StreetCode,
                BuildingNum = accountOwner.HouseNumber,
                AptNum = accountOwner.FlatNumber,
                ZipCode = accountOwner.PostalCode,
                Mobile = accountOwner.MobilePhone,
                Email = accountOwner.Email,
                Work = accountOwner.WorkPhone,
                Home = accountOwner.HomePhone,
                Address = accountOwner.Address,
                CRMRequestRef = int.Parse(apiEntity.DigitalFormNumber),
                IsDepositThirdParty = this.ConvertBooleanToSheinavString(accountOwner?.KYC?.TransactionsToFromThirdParty),
                IsPublic = this.ConvertBooleanToSheinavString(accountOwner?.KYC?.PublicPerson),
                PublicType = accountOwner?.KYC?.PublicPersonRole,
                KYCDate = accountOwner?.KYC?.CreatedOn.Value.ToString(dateFormat),
                RiskAccountIndex = this.GetEnumDescriptionAttribute<CapitalRiskLevelAccountCode>(authorizationManagement.CapitalRiskLevelAccountCode),
                DisplayName = apiEntity.Name,
                StatementsMailDef = this.GenerateStatementsMailDef(apiEntity),
                AgreementTariff = this.ConvertStringToInt(apiEntity.CommissionClientType?.Code),
                IsCompanyEmployee = this.ConvertBooleanToSheinavString(apiEntity.CompanyEmployeeBit),
                IsClientRequestVote = this.GetEnumDescriptionAttribute<VotingDocumentsCode>(apiEntity.VotingDocumentsCode),
                InformationPackage = Enum.GetName(typeof(InformationPackageCode), InformationPackageCode.Basic),
                IdentificationPhoneCode = apiEntity.AccountVerificationCode,
                ExpectedAmountDeposits = accountOwner?.KYC.TotalDepositForecastPerYearCode != null ?
                            accountOwner?.KYC.TotalDepositForecastPerYearCode.Value.ToString() : null,
                ExpectedDepositsFrequency = accountOwner?.KYC.FundsDepositFrequencyForecastCode != null ?
                            accountOwner?.KYC.FundsDepositFrequencyForecastCode.Value.ToString() : null,
                AccountTypeTax = AccountTypeTaxCode.IsraeliResident.GetDescriptionAttribute(),
                ExpectedWithdrawalsFrequency = accountOwner?.KYC.TotalWithdrawalOrTransferForecastCode != null ?
                            accountOwner?.KYC.TotalWithdrawalOrTransferForecastCode.Value.ToString() : null,
                ExpectedAmountWithdrawals = accountOwner?.KYC.YearlyTotalWithdrawalTransferForecastCode != null ?
                            accountOwner?.KYC.YearlyTotalWithdrawalTransferForecastCode.Value.ToString() : null,
                IsAltConnectedAccount = this.ConvertBooleanToSheinavString(accountOwner?.KYC.AdditionalAccountExistsAtAltshuler),
                IsAccountLien = this.CalculateAccountLien(authorizationManagement),
                LoyaltyProgram = apiEntity.LoyaltyProgramId?.Name
            };

            return general;
        }

        private string CalculateAccountLien(ApiAuthorizationManagement authorizationManagement)
        {
            string isAccountLien = (authorizationManagement.CreditRequestCode != null
                                && authorizationManagement.CreditRequestCode.Value == (int)CreditRequestCode.Yes)
                             || (authorizationManagement.ShortSaleRequestApprovalBit != null
                                && authorizationManagement.ShortSaleRequestApprovalBit.Value)
                             || (authorizationManagement.OptinExerciseRequestApprovalCode != null
                                && authorizationManagement.OptinExerciseRequestApprovalCode.Value == (int)OptinExerciseRequestApprovalCode.IncludeWriteOptions) ?
                                yes : no;
            return isAccountLien;
        }

        private List<PortfolioBeneficiary> GenerateAccountBeneficiaries(ApiDigitalFormVerification apiEntity)
        {
            this.GlobalContext.LogEntry();
            List<PortfolioBeneficiary> portfolioBeneficiaries = new List<PortfolioBeneficiary>();
            foreach (var accountHolder in apiEntity.AccountHolders)
            {
                ApiKYC apiKYC = accountHolder.KYC;
                PortfolioBeneficiary portfolioBeneficiary = new PortfolioBeneficiary()
                {
                    RelatedCountry = this.ConvertStringToInt(accountHolder.Country?.Code),
                    JoinDate = apiEntity.CreatedOn.Value.ToString(dateFormat),
                    BeneficiaryClientID = accountHolder.IdentificationNumber,
                    IDType = this.GetEnumDescriptionAttribute<IdentificationTypeCode>(accountHolder.IdentificationTypeCode),
                    IDTypeSec = this.GetEnumDescriptionAttribute<IdentificationTypeCode>(accountHolder.SecondIdentificationTypeCode),
                    FirstName = accountHolder.FirstName,
                    LastName = accountHolder.LastName,
                    FirstNameEnglish = accountHolder.FirstNameEng,
                    LastNameEnglish = accountHolder.LastNameEng,
                    RelatedCity = accountHolder.City?.Code,
                    RelatedStreet = accountHolder.Street?.StreetCode,
                    RelatedEmail = accountHolder.Email,
                    RelatedMobile = accountHolder.MobilePhone,
                    RelatedAptNum = accountHolder.FlatNumber,
                    RelatedBuildingNum = accountHolder.HouseNumber,
                    DateOfBirth = accountHolder.BirthDate != null ?
                        accountHolder.BirthDate.Value.ToString(dateFormat) : null,
                    CountryOfBirth = this.ConvertStringToInt(accountHolder.BirthCountry?.Code),
                    Gender = this.GetEnumDescriptionAttribute<Gender>(accountHolder.GenderCode),
                    AcctRelationType = this.GetEnumDescriptionAttribute<AccountHolderTypeCode>(accountHolder.AccountHolderTypeCode),
                    ProNonPro = this.GetEnumDescriptionAttribute<UserCharacteristicCode>(accountHolder.UserCharacteristicCode),
                    IssueDate = accountHolder.IDIssueDate != null ?
                        accountHolder.IDIssueDate.Value.ToString(dateFormat) : null,
                    IdentificationMethod = this.GetEnumDescriptionAttribute<PerformVerificationCode>(accountHolder.PerformVerificationCode),
                    SecIDNO = accountHolder.SecondaryIdentificationNumber,
                    SecIssueDate = accountHolder.SecondaryIDIssuedDate != null ?
                        accountHolder.SecondaryIDIssuedDate.Value.ToString(dateFormat) : null,
                    IssuingCountry = this.ConvertStringToInt(accountHolder.IdentificationIssuingCountry?.Code),
                    SecIssuingCountry = this.ConvertStringToInt(accountHolder.SecondaryIdentificationIssuingCountry?.Code),
                    RelatedZipCode = accountHolder.PostalCode,
                    AccpetCreditMonitorReport = this.ConvertBooleanToSheinavString(accountHolder.CreditReportCustomerApproval),
                    IsIsraeliResidentDeclare = this.ConvertBooleanToSheinavString(accountHolder.IsraeliResidency),
                    IsUSPersonResident = this.ConvertBooleanToSheinavString(!accountHolder.USPersonDeclaration.Value),
                    CheckedDate = accountHolder.PerformVerificationDate != null ?
                        accountHolder.PerformVerificationDate.Value.ToString(dateFormat) : null,
                    IsActive = yes,
                    IsMainOwner = ConvertBooleanToSheinavString(accountHolder.MainAccountHolder)

                };
                if (apiKYC != null)
                {
                    portfolioBeneficiary.OtherIncomeSources = apiKYC.IncomeSourcePrivate;
                    portfolioBeneficiary.EmploymentStatus = this.GetEnumDescriptionAttribute<EmploymentTypeCode>(apiKYC.EmploymentTypeCode);
                    portfolioBeneficiary.OtherEmploymentStatus = apiKYC.EmploymentCategoryDesc;
                    portfolioBeneficiary.RestrictedCountries = this.ConvertStringToInt(apiKYC.TradeRelationRiskCountryId?.Code);
                    portfolioBeneficiary.Restricted_Reason = apiKYC?.TradeRelationDesc;
                    portfolioBeneficiary.IsStateRiskRelation = this.ConvertBooleanToSheinavString(apiKYC.TradeRelationRiskTerritory);
                    portfolioBeneficiary.IncomeSources = this.ConvertMultiSelectToSheinavString<FundsSourceCode>(apiKYC.FundsSourceCode);
                    portfolioBeneficiary.RelatedPersonIncome = new ESBPortfolioRelatedPersonIncome()
                    {
                        EmploymentStatus = this.GetEnumDescriptionAttribute<EmploymentTypeCode>(apiKYC.EmploymentTypeCode),
                        OtherEmploymentStatus = apiKYC.EmploymentCategoryDesc,
                        EmploymentPosition = apiKYC.WorkplaceRole,
                        EmploymentJobName = apiKYC.WorkplaceName,
                        EmploymentCompanyName = apiKYC.BusinessName,
                        MonthlyIncomeRange = apiKYC.MonthlyIncomeLevelNISCode != null ?
                            apiKYC.MonthlyIncomeLevelNISCode.Value.ToString() : null,
                        IsOpenAccountRefusal = this.ConvertBooleanToSheinavString(apiKYC?.BankServiceDenial),
                        IsMarketingDataApproval = this.ConvertBooleanToSheinavString(accountHolder.AllowMarketingContent),
                        EmploymentClassification = apiKYC.EmploymentCategoryOccupation?.Code
                    };
                }

                portfolioBeneficiaries.Add(portfolioBeneficiary);
            }

            return portfolioBeneficiaries;
        }

        private ESBPortfolioFrames GenerateAccountFrames(ApiAuthorizationManagement authorizationManagement)
        {
            this.GlobalContext.LogEntry();
            ESBPortfolioFrames portfolioFrames = new ESBPortfolioFrames()
            {
                FrOverdraftFinancialCredit = this.ConvertDecimalToInt(authorizationManagement?.CreditAmountNIS),
                FrAggCreditLimit = this.ConvertDecimalToInt(authorizationManagement?.LineAggregateCreditLimit),
                FrAggCreditLimitPercent = authorizationManagement?.LineAggregateCreditLimitPercent,
                FrWriteOptions = this.ConvertDecimalToInt(authorizationManagement?.LineWriteOptions),
                FrStockShort = this.ConvertDecimalToInt(authorizationManagement?.LineStockShort)
            };
            return portfolioFrames;
        }

        private ESBPortfolioEntitlements GenerateAccountEntitlements(ApiDigitalFormVerification apiEntity, ApiAuthorizationManagement authorizationManagement)
        {
            this.GlobalContext.LogEntry();

            string optionExerciseRequetsSheinavValue = this.GetEnumDescriptionAttribute<OptinExerciseRequestApprovalCode>(authorizationManagement.OptinExerciseRequestApprovalCode);
            string allowedOptions = optionExerciseRequetsSheinavValue != null ? optionExerciseRequetsSheinavValue : OptinExerciseRequestApprovalCode.No.GetDescriptionAttribute();
            string allowedWriteOptions = authorizationManagement.OptinExerciseRequestApprovalCode != null
                && authorizationManagement.OptinExerciseRequestApprovalCode.Value == (int)OptinExerciseRequestApprovalCode.IncludeWriteOptions ?
                yes : no;
            string shortSaleRequestApprovalSheinavValue = this.ConvertBooleanToSheinavString(authorizationManagement.ShortSaleRequestApprovalBit);

            ESBPortfolioEntitlements portfolioEntitlements = new ESBPortfolioEntitlements()
            {
                AllowedForeignOptionsTrading = allowedOptions,
                AllowedIsraelOptionsTrading = allowedOptions,
                AllowedIsraeliFuture = allowedOptions,
                AllowedForeignFuture = allowedOptions,
                AllowedIsraelWeeklyOptions = allowedOptions,
                AllowedWriteForeignOptions = allowedWriteOptions,
                AllowedWriteIsraelOptions = allowedWriteOptions,
                AllowedIsraelShort = shortSaleRequestApprovalSheinavValue,
                AllowedForeignShort = shortSaleRequestApprovalSheinavValue,
                AllowedForeignBuy = yes,
                AllowedForeignSell = yes,
                AllowedIsraeliBuy = yes,
                AllowedIsraeliSell = yes,
                MarketMakerIsraelETF = no,
                MarketMakerIsraelBond = no,
                AllowedIsraelCredit = authorizationManagement.CreditRequestCode != null
                  && authorizationManagement.CreditRequestCode.Value == (int)CreditRequestCode.Yes ? yes : no
            };

            return portfolioEntitlements;
        }

        private ESBPortfolioStatementsMaildef GenerateStatementsMailDef(ApiDigitalFormVerification apiEntity)
        {
            this.GlobalContext.LogEntry();
            string isPost = no;
            string isEmail = no;
            if (apiEntity.QuarterlyReportsSendingCode != null)
            {
                QuarterlyReportsSendingCode quarterlyReportsSendingCode = (QuarterlyReportsSendingCode)apiEntity.QuarterlyReportsSendingCode;
                switch (quarterlyReportsSendingCode)
                {
                    case QuarterlyReportsSendingCode.IsraelPost:
                        {
                            isPost = yes;
                            break;
                        }
                    case QuarterlyReportsSendingCode.Email:
                        {
                            isEmail = yes;
                            break;
                        }
                    case QuarterlyReportsSendingCode.IsraelPostAndEmail:
                        {
                            isPost = yes;
                            isEmail = yes;
                            break;
                        }
                    default:
                        break;
                }
            }
            ESBPortfolioStatementsMaildef portfolioStatementsMaildef = new ESBPortfolioStatementsMaildef()
            {
                IsEmail = isEmail,
                IsPost = isPost
            };

            return portfolioStatementsMaildef;
        }

        private List<ESBPortfolioAccountDepositsWithdrawals> GenerateAccountDepositsWithdrawals(ApiDigitalFormVerification apiEntity)
        {
            this.GlobalContext.LogEntry();
            List<ESBPortfolioAccountDepositsWithdrawals> portfolioAccountDepositsWithdrawals = new List<ESBPortfolioAccountDepositsWithdrawals>()
            {
                new ESBPortfolioAccountDepositsWithdrawals()
                {
                    Bank = this.ConvertStringToInt(apiEntity.Bank?.Code),
                    Branch = this.ConvertStringToInt(apiEntity.Branch?.BranchNumber),
                    ClientAccountNumber = apiEntity.BankAccountNumber,
                    ClientAccountName = apiEntity.BankAccountName
                }
            };
            return portfolioAccountDepositsWithdrawals;
        }

        private string ConvertMultiSelectToSheinavString<T>(List<int> multiSelectValues) where T : Enum
        {
            string valueToReturn = null;
            if (multiSelectValues != null)
            {
                var values = multiSelectValues.Select(v => ((T)Enum.Parse(typeof(T), v.ToString())).GetDescriptionAttribute()).ToList();
                valueToReturn = string.Join(",", values);
            }

            return valueToReturn;
        }

        private string ConvertBooleanToSheinavString(bool? value)
        {
            string valueToReturn = null;
            if (value != null)
            {
                valueToReturn = ((ShenhavYesNoCode)Convert.ToInt32(value)).GetDescriptionAttribute();
            }
            return valueToReturn;
        }

        private string ConvertDecimalToString(decimal? value)
        {
            string valueToReturn = null;
            if (value != null)
            {
                valueToReturn = Convert.ToInt32(value).ToString();
            }
            return valueToReturn;
        }

        private int? ConvertDecimalToInt(decimal? value)
        {
            int? valueToReturn = null;
            if (value != null)
            {
                valueToReturn = Convert.ToInt32(value);
            }
            return valueToReturn;
        }

        private string GetEnumDescriptionAttribute<T>(int? value) where T : Enum
        {
            string valueToReturn = null;
            if (value != null)
            {
                valueToReturn = ((T)Enum.Parse(typeof(T), value.ToString())).GetDescriptionAttribute();
            }
            return valueToReturn;
        }

        private int? ConvertStringToInt(string value)
        {
            int? valueToReturn = null;
            if (value != null && int.TryParse(value, out int result))
            {
                valueToReturn = result;
            }
            return valueToReturn;
        }
    }
}
