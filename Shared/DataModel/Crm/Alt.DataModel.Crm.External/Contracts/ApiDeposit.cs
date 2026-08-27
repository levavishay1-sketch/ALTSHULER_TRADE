using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiDeposit : ApiEntity
    {
        public const string EntityLogicalName = "alt_deposit";
        public ApiDeposit() : base(EntityLogicalName) { }

        private DateTime? valueDate;
        /// <summary>
        /// תאריך ערך
        /// </summary>
        [CrmEntityMapper("alt_valuedate", CrmPropertyType.DateTime)]
        public DateTime? ValueDate
        {
            get
            {
                return valueDate;
            }
            set
            {
                this.SetProperty(value);
                this.valueDate = value;
            }
        }

        private decimal? depositAmount;
        /// <summary>
        /// סכום הפקדה
        /// </summary>
        [CrmEntityMapper("alt_depositamountdcml", CrmPropertyType.Decimal)]
        public decimal? DepositAmount
        {
            get
            {
                return depositAmount;
            }
            set
            {
                this.SetProperty(value);
                this.depositAmount = value;
            }
        }

        private int? currencyCode;
        /// <summary>
        /// מטבע
        /// </summary>
        [CrmEntityMapper("alt_currencycode", CrmPropertyType.OptionSet)]
        public int? CurrencyCode
        {
            get { return currencyCode; }
            set
            {
                this.SetProperty(value);
                currencyCode = value;
            }
        }

        private string referenceNumberInBank;
        /// <summary>
        /// מספר אסכמתא בבנק
        /// </summary>
        [CrmEntityMapper("alt_referencenumberinbank", CrmPropertyType.String)]
        public string ReferenceNumberInBank
        {
            get { return referenceNumberInBank; }
            set
            {
                this.SetProperty(value);
                referenceNumberInBank = value;
            }
        }

        private string bankAccountName;
        /// <summary>
        /// שם חשבון בנק
        /// </summary>
        [CrmEntityMapper("alt_bankaccountname", CrmPropertyType.String)]
        public string BankAccountName
        {
            get { return bankAccountName; }
            set
            {
                this.SetProperty(value);
                bankAccountName = value;
            }
        }

        private string cRMOppositeBankNumber;
        /// <summary>
        /// מספר בנק נגדי ב-CRM
        /// </summary>
        [CrmEntityMapper("alt_crmoppositebanknumber", CrmPropertyType.String)]
        public string CRMOppositeBankNumber
        {
            get { return cRMOppositeBankNumber; }
            set
            {
                this.SetProperty(value);
                cRMOppositeBankNumber = value;
            }
        }

        private string opposingBranchNumber;
        /// <summary>
        /// מספר סניף נגדי
        /// </summary>
        [CrmEntityMapper("alt_opposingbranchnumber", CrmPropertyType.String)]
        public string OpposingBranchNumber
        {
            get { return opposingBranchNumber; }
            set
            {
                this.SetProperty(value);
                opposingBranchNumber = value;
            }
        }

        private string opposingAccountNumber;
        /// <summary>
        /// מספר חשבון נגדי
        /// </summary>
        [CrmEntityMapper("alt_opposingaccountnumber", CrmPropertyType.String)]
        public string OpposingAccountNumber
        {
            get { return opposingAccountNumber; }
            set
            {
                this.SetProperty(value);
                opposingAccountNumber = value;
            }
        }

        private int? matchForDigitalFormVerificationCode;
        /// <summary>
        /// התאמה לבקרת טופס הצטרפות
        /// </summary>
        [CrmEntityMapper("alt_matchfordigitalformverificationcode", CrmPropertyType.OptionSet)]
        public int? MatchForDigitalFormVerificationCode
        {
            get { return matchForDigitalFormVerificationCode; }
            set
            {
                this.SetProperty(value);
                matchForDigitalFormVerificationCode = value;
            }
        }

        private string digitalFormNumber;
        /// <summary>
        /// מספר טופס דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformnumber", CrmPropertyType.String)]
        public string DigitalFormNumber
        {
            get { return digitalFormNumber; }
            set
            {
                this.SetProperty(value);
                digitalFormNumber = value;
            }
        }

        private int? digitalFormVerificationStatusCode;
        /// <summary>
        /// סטטוס בקרת טופס הצטרפות
        /// </summary>
        [CrmEntityMapper("alt_digitalformverificationstatuscode", CrmPropertyType.OptionSet)]
        public int? DigitalFormVerificationStatusCode
        {
            get { return digitalFormVerificationStatusCode; }
            set
            {
                this.SetProperty(value);
                digitalFormVerificationStatusCode = value;
            }
        }

        private string beneficiaryAccountHolder;
        /// <summary>
        /// נהנה
        /// </summary>
        [CrmEntityMapper("alt_beneficiaryaccountholder", CrmPropertyType.String)]
        public string BeneficiaryAccountHolder
        {
            get { return beneficiaryAccountHolder; }
            set
            {
                this.SetProperty(value);
                beneficiaryAccountHolder = value;
            }
        }

        private int? matchForPortfolioCode;
        /// <summary>
        /// התאמה לחשבון
        /// </summary>
        [CrmEntityMapper("alt_matchforportfoliocode", CrmPropertyType.OptionSet)]
        public int? MatchForPortfolioCode
        {
            get { return matchForPortfolioCode; }
            set
            {
                this.SetProperty(value);
                matchForPortfolioCode = value;
            }
        }

        private string joiningProcessNumber;
        /// <summary>
        /// מספר תהליך הצטרפות
        /// </summary>
        [CrmEntityMapper("alt_joiningprocessnumber", CrmPropertyType.String)]
        public string JoiningProcessNumber
        {
            get { return joiningProcessNumber; }
            set
            {
                this.SetProperty(value);
                joiningProcessNumber = value;
            }
        }

        private int? shenhavStatusCode;
        /// <summary>
        /// סטטוס חשבון בשנהב
        /// </summary>
        [CrmEntityMapper("alt_shenhavstatuscode", CrmPropertyType.OptionSet)]
        public int? ShenhavStatusCode
        {
            get { return shenhavStatusCode; }
            set
            {
                this.SetProperty(value);
                shenhavStatusCode = value;
            }
        }

        private string shenhavAccountNumber;
        /// <summary>
        /// מספר חשבון שנהב מה CRM
        /// </summary>
        [CrmEntityMapper("alt_shenhavaccountnumber", CrmPropertyType.String)]
        public string ShenhavAccountNumber
        {
            get { return shenhavAccountNumber; }
            set
            {
                this.SetProperty(value);
                shenhavAccountNumber = value;
            }
        }
       
        private bool? automaticLaunchedShenhavPortfolio;
        /// <summary>
        /// הוזנק אוט' ממשק לשנהב בעקבות קליטת הפקדה
        /// </summary>
        [CrmEntityMapper("alt_automaticlaunchedshenhavportfoliobit", CrmPropertyType.Bool)]
        public bool? AutomaticLaunchedShenhavPortfolio
        {
            get
            {
                return automaticLaunchedShenhavPortfolio;
            }
            set
            {
                this.SetProperty(value);
                this.automaticLaunchedShenhavPortfolio = value;
            }
        }

        private DateTime? automaticLaunchShenhavPortfolioDate;
        /// <summary>
        /// תאריך הזנקה אוט' ממשק לשנהב בעקבות קליטת הפקדה
        /// </summary>
        [CrmEntityMapper("alt_automaticlaunchshenhavportfoliodate", CrmPropertyType.DateTime)]
        public DateTime? AutomaticLaunchShenhavPortfolioDate
        {
            get
            {
                return automaticLaunchShenhavPortfolioDate;
            }
            set
            {
                this.SetProperty(value);
                this.automaticLaunchShenhavPortfolioDate = value;
            }
        }

        private bool? depositAmountBelow5000;
        /// <summary>
        /// לא הוזנק ממשק לשנהב - סכום נמוך מ-5000
        /// </summary>
        [CrmEntityMapper("alt_depositamountbelow5000bit", CrmPropertyType.Bool)]
        public bool? DepositAmountBelow5000
        {
            get
            {
                return depositAmountBelow5000;
            }
            set
            {
                this.SetProperty(value);
                this.depositAmountBelow5000 = value;
            }
        }

        private string mainAccountHolder;
        /// <summary>
        /// בעל חשבון ראשי
        /// </summary>
        [CrmEntityMapper("alt_mainaccountholder", CrmPropertyType.String)]
        public string MainAccountHolder
        {
            get { return mainAccountHolder; }
            set
            {
                this.SetProperty(value);
                mainAccountHolder = value;
            }
        }

        private string mainAccountHolderIdentificationNumber;
        /// <summary>
        /// ת.ז בעל חשבון ראשי
        /// </summary>
        [CrmEntityMapper("alt_mainaccountholderidentificationnumber", CrmPropertyType.String)]
        public string MainAccountHolderIdentificationNumber
        {
            get { return mainAccountHolderIdentificationNumber; }
            set
            {
                this.SetProperty(value);
                mainAccountHolderIdentificationNumber = value;
            }
        }

        private string firstCreatedDigitalFormNumber;
        /// <summary>
        /// מספר טופס דיגיטלי ראשון שנוצר
        /// </summary>
        [CrmEntityMapper("alt_firstcreateddigitalformnumber", CrmPropertyType.String)]
        public string FirstCreatedDigitalFormNumber
        {
            get { return firstCreatedDigitalFormNumber; }
            set
            {
                this.SetProperty(value);
                firstCreatedDigitalFormNumber = value;
            }
        }

        private string firstCreatedShenhavAccountNumber;
        /// <summary>
        /// מספר חשבון ראשון שנוצר בשנהב
        /// </summary>
        [CrmEntityMapper("alt_firstcreatedshenhavaccountnumber", CrmPropertyType.String)]
        public string FirstCreatedShenhavAccountNumber
        {
            get { return firstCreatedShenhavAccountNumber; }
            set
            {
                this.SetProperty(value);
                firstCreatedShenhavAccountNumber = value;
            }
        }

        private int? firstCreatedPortfolioShenhavStatusCode;
        /// <summary>
        /// סטטוס חשבון ראשון שנוצר בשנהב
        /// </summary>
        [CrmEntityMapper("alt_firstcreatedportfolioshenhavstatuscode", CrmPropertyType.OptionSet)]
        public int? FirstCreatedPortfolioShenhavStatusCode
        {
            get { return firstCreatedPortfolioShenhavStatusCode; }
            set
            {
                this.SetProperty(value);
                firstCreatedPortfolioShenhavStatusCode = value;
            }
        }
    }
}
