using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiDigitalFormVerification : ApiEntity
    {
        public const string EntityLogicalName = "alt_digitalformverification";

        public ApiDigitalFormVerification() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם חשבון  
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_name", CrmPropertyType.String)]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.SetProperty(value);
                this.name = value;
            }
        }

        private string digitalFormNumber;
        /// <summary>
        /// מספר טופס דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformnumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string DigitalFormNumber
        {
            get
            {
                return digitalFormNumber;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormNumber = value;
            }
        }

        private string accountVerificationCode;
        /// <summary>
        /// קוד אימות לחשבון
        /// </summary>
        [CrmEntityMapper("alt_accountverificationcode", CrmPropertyType.String)]
        [StringLength(100)]
        public string AccountVerificationCode
        {
            get
            {
                return accountVerificationCode;
            }
            set
            {
                this.SetProperty(value);
                this.accountVerificationCode = value;
            }
        }

        private string bankAccountNumber;
        /// <summary>
        /// מספר חשבון
        /// </summary>
        [CrmEntityMapper("alt_bankaccountnumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string BankAccountNumber
        {
            get
            {
                return bankAccountNumber;
            }
            set
            {
                this.SetProperty(value);
                this.bankAccountNumber = value;
            }
        }

        private string ipAddress;
        /// <summary>
        /// כתובת IP בסיום תהליךעובד חברה
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_ipaddress", CrmPropertyType.String)]
        public string IPAddress
        {
            get
            {
                return ipAddress;
            }
            set
            {
                this.SetProperty(value);
                this.ipAddress = value;
            }
        }

        private int? creditRequestExistsCode;
        /// <summary>
        /// קיימת בקשת מסגרת אשראי
        /// </summary>
        [CrmEntityMapper("alt_creditrequestexistscode", CrmPropertyType.OptionSet)]
        [Range(1, 2)]
        public int? CreditRequestExistsCode
        {
            get
            {
                return creditRequestExistsCode;
            }
            set
            {
                this.SetProperty(value);
                this.creditRequestExistsCode = value;
            }
        }

        private int? shortSaleRequestApprovaIExistsCode;
        /// <summary>
        /// קיימת בקשה למכירת ני"ע בחסר
        /// </summary>
        [CrmEntityMapper("alt_shortsalerequestapprovaiexistscode", CrmPropertyType.OptionSet)]
        [Range(1, 2)]
        public int? ShortSaleRequestApprovaIExistsCode
        {
            get
            {
                return shortSaleRequestApprovaIExistsCode;
            }
            set
            {
                this.SetProperty(value);
                this.shortSaleRequestApprovaIExistsCode = value;
            }
        }

        /// <summary>
        /// קיימת בקשה למסחר באופציות
        /// </summary>
        private int? optionExerciseRequestApprovalExistsCode;
        [CrmEntityMapper("alt_optionexerciserequestapprovalexistscode", CrmPropertyType.OptionSet)]
        [Range(1, 3)]
        public int? OptionExerciseRequestApprovalExistsCode
        {
            get
            {
                return optionExerciseRequestApprovalExistsCode;
            }
            set
            {
                this.SetProperty(value);
                this.optionExerciseRequestApprovalExistsCode = value;
            }
        }

        private int? votingDocumentsCode;
        /// <summary>
        /// כתבי הצבעה ואסיפות כלליות
        /// </summary>
        [CrmEntityMapper("alt_votingdocumentscode", CrmPropertyType.OptionSet)]
        [Range(1, 3)]
        public int? VotingDocumentsCode
        {
            get
            {
                return votingDocumentsCode;
            }
            set
            {
                this.SetProperty(value);
                this.votingDocumentsCode = value;
            }
        }

        private int? quarterlyReportsSendingCode;
        /// <summary>
        /// אופן שליחת דיווחים רבעוניים
        /// </summary>
        [CrmEntityMapper("alt_quarterlyreportssendingcode", CrmPropertyType.OptionSet)]
        [Range(1, 3)]
        public int? QuarterlyReportsSendingCode
        {
            get
            {
                return quarterlyReportsSendingCode;
            }
            set
            {
                this.SetProperty(value);
                this.quarterlyReportsSendingCode = value;
            }
        }

        private int? formStatusCode;
        /// <summary>
        /// סטטוס הטופס
        /// </summary>
        [CrmEntityMapper("alt_formstatuscode", CrmPropertyType.OptionSet)]  
        public int? FormStatusCode
        {
            get
            {
                return formStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.formStatusCode = value;
            }
        }

        private ApiCommissionClientType commissionClientType;
        /// <summary>
        /// סוג לקוח לעמלה
        /// </summary>
        [CrmEntityMapper("alt_commissionclienttypeid", CrmPropertyType.EntityReference)]
        public ApiCommissionClientType CommissionClientType
        {
            get
            {
                return commissionClientType;
            }
            set
            {
                this.SetProperty(value);
                this.commissionClientType = value;
            }
        }

        private bool? companyEmployeeBit;
        /// <summary>
        /// עובד חברה
        /// </summary>
        [CrmEntityMapper("alt_companyemployeebit", CrmPropertyType.Bool)]
        public bool? CompanyEmployeeBit
        {
            get
            {
                return companyEmployeeBit;
            }
            set
            {
                this.SetProperty(value);
                this.companyEmployeeBit = value;
            }
        }

        private decimal? creditAmountNISRequest;
        /// <summary>
        /// גובה המסגרת  המבוקשת בשקלים
        /// </summary>
        [CrmEntityMapper("alt_creditamountnisrequestmny", CrmPropertyType.Money)]
        public decimal? CreditAmountNISRequest
        {
            get
            {
                return creditAmountNISRequest;
            }
            set
            {
                this.SetProperty(value);
                this.creditAmountNISRequest = value;
            }
        }

        private ApiDigitalForm digitalForm;
        /// <summary>
        /// טופס דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformid", CrmPropertyType.EntityReference)]
        public ApiDigitalForm DigitalForm
        {
            get
            {
                return digitalForm;
            }
            set
            {
                this.SetProperty(value);
                this.digitalForm = value;
            }
        }

        private ApiTeam controlStageTeamId;
        /// <summary>
        /// שלב הבקרה
        /// </summary>
        [CrmEntityMapper("alt_controlstageteamid", CrmPropertyType.EntityReference)]
        public ApiTeam ControlStageTeamId
        {
            get
            {
                return controlStageTeamId;
            }
            set
            {
                this.SetProperty(value);
                this.controlStageTeamId = value;
            }
        }

        private ApiBank bank;
        /// <summary>
        /// בנק
        /// </summary>
        [CrmEntityMapper("alt_bankid", CrmPropertyType.EntityReference)]
        public ApiBank Bank
        {
            get
            {
                return bank;
            }
            set
            {
                this.SetProperty(value);
                this.bank = value;
            }
        }

        private ApiBranch branch;
        /// <summary>
        /// סניף
        /// </summary>
        [CrmEntityMapper("alt_branchid", CrmPropertyType.EntityReference)]
        public ApiBranch Branch
        {
            get
            {
                return branch;
            }
            set
            {
                this.SetProperty(value);
                this.branch = value;
            }
        }

        private int? transferToShenhavStatusCode;
        /// <summary>
        /// סטטוס שידור לשינב
        /// </summary>
        [CrmEntityMapper("alt_transfertoshenhavstatuscode", CrmPropertyType.OptionSet)]
        public int? TransferToShenhavStatusCode
        {
            get
            {
                return transferToShenhavStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.transferToShenhavStatusCode = value;
            }
        }

        private int? accountClassificationCode;
        /// <summary>
        /// סיווג חשבון לבורסה
        /// </summary>
        [CrmEntityMapper("alt_accountclassificationcode", CrmPropertyType.OptionSet)]
        public int? AccountClassificationCode
        {
            get
            {
                return accountClassificationCode;
            }
            set
            {
                this.SetProperty(value);
                this.accountClassificationCode = value;
            }
        }

        private string transferToShenhavErrorDescription;
        /// <summary>
        /// פירוט כישלון שידור לשינב
        /// </summary>
        [CrmEntityMapper("alt_transfertoshenhaverrordescription", CrmPropertyType.String)]
        public string TransferToShenhavErrorDescription
        {
            get
            {
                return transferToShenhavErrorDescription;
            }
            set
            {
                this.SetProperty(value);
                this.transferToShenhavErrorDescription = value;
            }
        }

        private string bankAccountName;
        /// <summary>
        /// שם חשבון בבנק
        /// </summary>
        [CrmEntityMapper("alt_bankaccountname", CrmPropertyType.String)]
        public string BankAccountName
        {
            get
            {
                return bankAccountName;
            }
            set
            {
                this.SetProperty(value);
                this.bankAccountName = value;
            }
        }

        private ApiPortfolio portfolioId;
        /// <summary>
        /// מספר חשבון שנהב
        /// </summary>
        [CrmEntityMapper("alt_portfolioid", CrmPropertyType.EntityReference)]
        [JsonIgnore]
        public ApiPortfolio PortfolioId
        {
            get
            {
                return portfolioId;
            }
            set
            {
                this.SetProperty(value);
                this.portfolioId = value;
            }
        }

        private ApiOpportunity opportunityId;
        /// <summary>
        /// הזדמנות
        /// </summary>
        [CrmEntityMapper("alt_opportunityid", CrmPropertyType.EntityReference)]
        public ApiOpportunity OpportunityId
        {
            get
            {
                return opportunityId;
            }
            set
            {
                this.SetProperty(value);
                this.opportunityId = value;
            }
        }

        private ApiLoyaltyProgram loyaltyProgramId;
        /// <summary>
        /// מועדון
        /// </summary>
        [CrmEntityMapper("alt_loyaltyprogramid", CrmPropertyType.EntityReference)]
        public ApiLoyaltyProgram LoyaltyProgramId
        {
            get
            {
                return loyaltyProgramId;
            }
            set
            {
                this.SetProperty(value);
                this.loyaltyProgramId = value;
            }
        }

        private ApiAccountHolder primaryAccountHolderId;
        /// <summary>
        /// בעל חשבון ראשי
        /// </summary>
        [CrmEntityMapper("alt_primaryaccountholderid", CrmPropertyType.EntityReference)]
        public ApiAccountHolder PrimaryAccountHolderId
        {
            get
            {
                return primaryAccountHolderId;
            }
            set
            {
                this.SetProperty(value);
                this.primaryAccountHolderId = value;
            }
        }

        private List<ApiAccountHolder> accountHolders = new List<ApiAccountHolder>();
        [JsonIgnore]
        public List<ApiAccountHolder> AccountHolders
        {
            set { accountHolders = value; }
            get
            {
                return accountHolders;
            }
        }

        private List<ApiAccountHolder> portfolioBeneficiaries;
        public List<ApiAccountHolder> PortfolioBeneficiaries
        {
            get
            {
                return portfolioBeneficiaries != null && portfolioBeneficiaries.Count > 0 ? portfolioBeneficiaries : null;
            }
            set
            {
                this.portfolioBeneficiaries = value;
                this.SetProperty(value);
                this.accountHolders.AddRange(value);
            }
        }

        private List<ApiAccountHolder> portfolioOwners;
        public List<ApiAccountHolder> PortfolioOwners
        {
            get
            {
                return portfolioOwners;
            }
            set
            {
                this.portfolioOwners = value;
                this.SetProperty(value);
                this.accountHolders.AddRange(this.portfolioOwners);
            }
        }

        private List<ApiAuthorizationManagement> authorizationManagements;
        public List<ApiAuthorizationManagement> AuthorizationManagements
        {
            get
            {
                return authorizationManagements;
            }
            set
            {
                this.authorizationManagements = value;
                this.SetProperty(value);
            }
        }
    }
}
