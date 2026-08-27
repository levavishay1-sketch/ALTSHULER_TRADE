using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiKYC : ApiEntity
    {
        public const string EntityLogicalName = "alt_kyc";
        public ApiKYC() : base(EntityLogicalName)
        {
        }

        private string workplaceRole;
        /// <summary>
        /// תפקיד
        /// </summary>
        [CrmEntityMapper("alt_workplacerole", CrmPropertyType.String)]
        [StringLength(100)]
        public string WorkplaceRole
        {
            get
            {
                return workplaceRole;
            }
            set
            {
                this.SetProperty(value);
                this.workplaceRole = value;
            }
        }

        private string workplaceName;
        /// <summary>
        /// שם מקום העבודה
        /// </summary>
        [CrmEntityMapper("alt_workplacename", CrmPropertyType.String)]
        [StringLength(100)]
        public string WorkplaceName
        {
            get
            {
                return workplaceName;
            }
            set
            {
                this.SetProperty(value);
                this.workplaceName = value;
            }
        }

        private string businessName;
        /// <summary>
        /// שם העסק
        /// </summary>
        [CrmEntityMapper("alt_businessname", CrmPropertyType.String)]
        [StringLength(100)]
        public string BusinessName
        {
            get
            {
                return businessName;
            }
            set
            {
                this.SetProperty(value);
                this.businessName = value;
            }
        }

        private string incomeSourcePrivate;
        /// <summary>
        /// מקור הכנסה -פירוט ירושה / מתנה / אחר
        /// </summary>
        [CrmEntityMapper("alt_incomesourceprivate", CrmPropertyType.String)]
        [StringLength(100)]
        public string IncomeSourcePrivate
        {
            get
            {
                return incomeSourcePrivate;
            }
            set
            {
                this.SetProperty(value);
                this.incomeSourcePrivate = value;
            }
        }

        private string publicPersonRole;
        /// <summary>
        /// תפקיד של איש ציבור
        /// </summary>
        [CrmEntityMapper("alt_publicpersonrole", CrmPropertyType.String)]
        [StringLength(100)]
        public string PublicPersonRole
        {
            get
            {
                return publicPersonRole;
            }
            set
            {
                this.SetProperty(value);
                this.publicPersonRole = value;
            }
        }

        private string employmentCategoryDesc;
        /// <summary>
        /// פירוט עיסוק אחר
        /// </summary>
        [CrmEntityMapper("alt_employmentcategorydesc", CrmPropertyType.String)]
        [StringLength(100)]
        public string EmploymentCategoryDesc
        {
            get
            {
                return employmentCategoryDesc;
            }
            set
            {
                this.SetProperty(value);
                this.employmentCategoryDesc = value;
            }
        }

        private string otherEmploymentDesc;
        /// <summary>
        /// פירוט אחר
        /// </summary>
        [CrmEntityMapper("alt_otheremploymentdesc", CrmPropertyType.String)]
        [StringLength(100)]
        public string OtherEmploymentDesc
        {
            get
            {
                return otherEmploymentDesc;
            }
            set
            {
                this.SetProperty(value);
                this.otherEmploymentDesc = value;
            }
        }

        private string tradeRelationDesc;
        /// <summary>
        /// פירוט קשרי מסחר
        /// </summary>
        [CrmEntityMapper("alt_traderelationdesc", CrmPropertyType.String)]
        public string TradeRelationDesc
        {
            get
            {
                return tradeRelationDesc;
            }
            set
            {
                this.SetProperty(value);
                this.tradeRelationDesc = value;
            }
        }

        private string additionalAccountDetails;
        /// <summary>
        /// פרוט חשבונות נוספים
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_additionalaccountdetails", CrmPropertyType.String)]
        public string AdditionalAccountDetails
        {
            get
            {
                return additionalAccountDetails;
            }
            set
            {
                this.SetProperty(value);
                this.additionalAccountDetails = value;
            }
        }

        private bool? publicPerson;
        /// <summary>
        /// איש ציבור בארץ או בחו"ל
        /// </summary>
        [CrmEntityMapper("alt_publicpersonbit", CrmPropertyType.Bool)]
        public bool? PublicPerson
        {
            get
            {
                return publicPerson;
            }
            set
            {
                this.SetProperty(value);
                this.publicPerson = value;
            }
        }

        private bool? bankServiceDenial;
        /// <summary>
        /// סירוב קבלת שירותים בנקאיים
        /// </summary>
        [CrmEntityMapper("alt_bankservicedenialbit", CrmPropertyType.Bool)]
        public bool? BankServiceDenial
        {
            get
            {
                return bankServiceDenial;
            }
            set
            {
                this.SetProperty(value);
                this.bankServiceDenial = value;
            }
        }

        private bool? transactionsToFromThirdParty;
        /// <summary>
        /// ביצוע הפקדות ו/או העברות כספים אל/מאת צד ג'
        /// </summary>
        [CrmEntityMapper("alt_transactionstofromthirdpartybit", CrmPropertyType.Bool)]
        public bool? TransactionsToFromThirdParty
        {
            get
            {
                return transactionsToFromThirdParty;
            }
            set
            {
                this.SetProperty(value);
                this.transactionsToFromThirdParty = value;
            }
        }

        private bool? tradeRelationRiskTerritory;
        /// <summary>
        /// קיימים קשרי מסחר עם מדינה בסיכון
        /// </summary>
        [CrmEntityMapper("alt_traderelationriskterritorybit", CrmPropertyType.Bool)]
        public bool? TradeRelationRiskTerritory
        {
            get
            {
                return tradeRelationRiskTerritory;
            }
            set
            {
                this.SetProperty(value);
                this.tradeRelationRiskTerritory = value;
            }
        }

        private bool? additionalAccountExistsAtAltshuler;
        /// <summary>
        /// קיימים חשבונות נוספים באלטשולר שחם טרייד
        /// </summary>
        [CrmEntityMapper("alt_additionalaccountexistsataltshulerbit", CrmPropertyType.Bool)]
        public bool? AdditionalAccountExistsAtAltshuler
        {
            get
            {
                return additionalAccountExistsAtAltshuler;
            }
            set
            {
                this.SetProperty(value);
                this.additionalAccountExistsAtAltshuler = value;
            }
        }

        private int? accountObjectiveCode;
        /// <summary>
        /// מטרת פתיחת החשבון
        /// </summary>
        [CrmEntityMapper("alt_accountobjectivecode", CrmPropertyType.OptionSet)]
        [Range(1, 2)]
        public int? AccountObjectiveCode
        {
            get
            {
                return accountObjectiveCode;
            }
            set
            {
                this.SetProperty(value);
                this.accountObjectiveCode = value;
            }
        }

        private int? monthlyIncomeLevelNISCode;
        /// <summary>
        /// רמת הכנסה חודשית בש"ח
        /// </summary>
        [CrmEntityMapper("alt_monthlyincomelevelniscode", CrmPropertyType.OptionSet)]
        [Range(1, 5)]
        public int? MonthlyIncomeLevelNISCode
        {
            get
            {
                return monthlyIncomeLevelNISCode;
            }
            set
            {
                this.SetProperty(value);
                this.monthlyIncomeLevelNISCode = value;
            }
        }

        private int? fundsDepositFrequencyForecastCode;
        /// <summary>
        /// צפי תדירות הפקדת כספים בחשבון
        /// </summary>
        [CrmEntityMapper("alt_fundsdepositfrequencyforecastcode", CrmPropertyType.OptionSet)]
        [Range(1, 5)]
        public int? FundsDepositFrequencyForecastCode
        {
            get
            {
                return fundsDepositFrequencyForecastCode;
            }
            set
            {
                this.SetProperty(value);
                this.fundsDepositFrequencyForecastCode = value;
            }
        }

        private int? totalDepositForecastPerYearCode;
        /// <summary>
        /// סה"כ הפקדות שוטפות  צפוי לתקופה של שנה
        /// </summary>
        [CrmEntityMapper("alt_totaldepositforecastperyearcode", CrmPropertyType.OptionSet)]
        [Range(1, 4)]
        public int? TotalDepositForecastPerYearCode
        {
            get
            {
                return totalDepositForecastPerYearCode;
            }
            set
            {
                this.SetProperty(value);
                this.totalDepositForecastPerYearCode = value;
            }
        }

        private int? totalWithdrawalOrTransferForecastCode;
        /// <summary>
        /// צפי תדירות משיכות/ העברת כספים מהחשבון
        /// </summary>
        [CrmEntityMapper("alt_totalwithdrawalortransferforecastcode", CrmPropertyType.OptionSet)]
        [Range(1, 5)]
        public int? TotalWithdrawalOrTransferForecastCode
        {
            get
            {
                return totalWithdrawalOrTransferForecastCode;
            }
            set
            {
                this.SetProperty(value);
                this.totalWithdrawalOrTransferForecastCode = value;
            }
        }

        private int? yearlyTotalWithdrawalTransferForecastCode;
        /// <summary>
        /// סכום משיכות/העברות הצפוי מהחשבון בתקופה של שנה
        /// </summary>
        [CrmEntityMapper("alt_yearlytotalwithdrawaltransferforecastcode", CrmPropertyType.OptionSet)]
        [Range(1, 4)]
        public int? YearlyTotalWithdrawalTransferForecastCode
        {
            get
            {
                return yearlyTotalWithdrawalTransferForecastCode;
            }
            set
            {
                this.SetProperty(value);
                this.yearlyTotalWithdrawalTransferForecastCode = value;
            }
        }

        private int? transactionsRelationToFromThirdPartyCode;
        /// <summary>
        /// מהי הזיקה לצד ג' (הפקדות / העברות)
        /// </summary>
        [CrmEntityMapper("alt_transactionsrelationtofromthirdpartycode", CrmPropertyType.OptionSet)]
        [Range(1, 2)]
        public int? TransactionsRelationToFromThirdPartyCode
        {
            get
            {
                return transactionsRelationToFromThirdPartyCode;
            }
            set
            {
                this.SetProperty(value);
                this.transactionsRelationToFromThirdPartyCode = value;
            }
        }

        private string transactionsRelationToFromThirdParty;
        /// <summary>
        /// פרט הזיקה לצג ג' (הפקדות/העברות)
        /// </summary>
        [CrmEntityMapper("alt_transactionsrelationtofromthirdparty", CrmPropertyType.String)]
        [StringLength(100)]
        public string TransactionsRelationToFromThirdParty
        {
            get
            {
                return transactionsRelationToFromThirdParty;
            }
            set
            {
                this.SetProperty(value);
                this.transactionsRelationToFromThirdParty = value;
            }
        }

        private int? employmentTypeCode;
        /// <summary>
        /// אופי התעסוקה
        /// </summary>
        [CrmEntityMapper("alt_employmenttypecode", CrmPropertyType.OptionSet)]
        public int? EmploymentTypeCode
        {
            get
            {
                return employmentTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.employmentTypeCode = value;
            }
        }

        private List<int> incomeSourceCode;
        /// <summary>
        /// מה מקורות ההכנסה שלך?
        /// </summary>
        [CrmEntityMapper("alt_incomesourcecode", CrmPropertyType.OptionSetCollection)]
        public List<int> IncomeSourceCode
        {
            get
            {
                return incomeSourceCode;
            }
            set
            {
                this.SetProperty(value);
                this.incomeSourceCode = value;
            }
        }

        private List<int> fundsSourceCode;
        /// <summary>
        /// מקור הכספים שיופקדו בחשבון המסחר
        /// </summary>
        [CrmEntityMapper("alt_fundssourcecode", CrmPropertyType.OptionSetCollection)]
        public List<int> FundsSourceCode
        {
            get
            {
                return fundsSourceCode;
            }
            set
            {
                this.SetProperty(value);
                this.fundsSourceCode = value;
            }
        }

        private ApiOccupation employmentCategoryOccupation;
        /// <summary>
        /// תחום העיסוק
        /// </summary>
        [CrmEntityMapper("alt_employmentcategoryoccupationid", CrmPropertyType.EntityReference)]
        public ApiOccupation EmploymentCategoryOccupation
        {
            get
            {
                return employmentCategoryOccupation;
            }
            set
            {
                this.SetProperty(value);
                this.employmentCategoryOccupation = value;
            }
        }

        private ApiAccountHolder accountHolder;
        /// <summary>
        /// בעל חשבון
        /// </summary>
        [CrmEntityMapper("alt_accountholderid", CrmPropertyType.EntityReference)]
        public ApiAccountHolder AccountHolder
        {
            get
            {
                return accountHolder;
            }
            set
            {
                this.SetProperty(value);
                this.accountHolder = value;
            }
        }

        private ApiDigitalFormVerification digitalFormVerification;
        /// <summary>
        /// מספר טופס בקרת הצטרפות
        /// </summary>
        [CrmEntityMapper("alt_digitalformverificationid", CrmPropertyType.EntityReference)]

        public ApiDigitalFormVerification DigitalFormVerification
        {
            get
            {
                return digitalFormVerification;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormVerification = value;
            }
        }

        private ApiCountry tradeRelationRiskCountryId;
        /// <summary>
        /// פרט מדינה
        /// </summary>
        [CrmEntityMapper("alt_traderelationriskcountryid", CrmPropertyType.EntityReference)]
        public ApiCountry TradeRelationRiskCountryId
        {
            get
            {
                return tradeRelationRiskCountryId;
            }
            set
            {
                this.SetProperty(value);
                this.tradeRelationRiskCountryId = value;
            }
        }

        private string relatedPortfolioIdentityNumber;
        /// <summary>
        /// מספר זהות – חשבון קשור
        /// </summary>
        [CrmEntityMapper("alt_relatedportfolioidentitynumber", CrmPropertyType.String)]
        public string RelatedPortfolioIdentityNumber
        {
            get
            {
                return relatedPortfolioIdentityNumber;
            }
            set
            {
                this.SetProperty(value);
                this.relatedPortfolioIdentityNumber = value;
            }
        }

        private string relatedPortfolioRelationshipType;
        /// <summary>
        /// סוג קרבה – חשבון קשור
        /// </summary>
        [CrmEntityMapper("alt_relatedportfoliorelationshiptype", CrmPropertyType.String)]
        public string RelatedPortfolioRelationshipType
        {
            get
            {
                return relatedPortfolioRelationshipType;
            }
            set
            {
                this.SetProperty(value);
                this.relatedPortfolioRelationshipType = value;
            }
        }

        private string relatedPortfolioLastName;
        /// <summary>
        /// שם משפחה – חשבון קשור
        /// </summary>
        [CrmEntityMapper("alt_relatedportfoliolastname", CrmPropertyType.String)]
        [StringLength(100)]
        public string RelatedPortfolioLastName
        {
            get
            {
                return relatedPortfolioLastName;
            }
            set
            {
                this.SetProperty(value);
                this.relatedPortfolioLastName = value;
            }
        }

        private string relatedPortfolioFirstName;
        /// <summary>
        /// שם פרטי – חשבון קשור
        /// </summary>
        [CrmEntityMapper("alt_relatedportfoliofirstname", CrmPropertyType.String)]
        [StringLength(100)]
        public string RelatedPortfolioFirstName
        {
            get
            {
                return relatedPortfolioFirstName;
            }
            set
            {
                this.SetProperty(value);
                this.relatedPortfolioFirstName = value;
            }
        }
    }
}
