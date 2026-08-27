using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPortfolioGeneral : ExternalEntityBase
    {
        public ESBPortfolioGeneral(PortfolioActionTypeCode type)
        {
            this.ActionType = type.GetDescriptionAttribute();
        }

        private int? cRMRequestRef;
        /// <summary>
        /// אסמכתת פניה מ-CRM
        /// </summary>
        [Required]
        public int? CRMRequestRef
        {
            get => this.cRMRequestRef;
            set
            {
                base.SetProperty(value);
                this.cRMRequestRef = value;
            }
        }

        private string actionType;
        /// <summary>
        /// סוג פעולה
        /// </summary>
        [Required]
        public string ActionType
        {
            get => this.actionType;
            set
            {
                base.SetProperty(value);
                this.actionType = value;
            }
        }

        private int? accountNumber;
        /// <summary>
        /// מספר חשבון
        /// </summary>
       //[Required]
        public int? AccountNumber
        {
            get => this.accountNumber;
            set
            {
                base.SetProperty(value);
                this.accountNumber = value;
            }
        }

        private string displayName;
        /// <summary>
        /// שם חשבון (חש"ב)
        /// </summary>
        public string DisplayName
        {
            get => this.displayName;
            set
            {
                base.SetProperty(value);
                this.displayName = value;
            }
        }

        private string accountType;
        /// <summary>
        /// סוג חשבון
        /// </summary>
        [Required]
        public string AccountType
        {
            get => this.accountType;
            set
            {
                base.SetProperty(value);
                this.accountType = value;
            }
        }

        private string accountTypeSub;
        /// <summary>
        /// תת סוג חשבון
        /// </summary>
        [Required]
        public string AccountTypeSub
        {
            get => this.accountTypeSub;
            set
            {
                base.SetProperty(value);
                this.accountTypeSub = value;
            }
        }

        private int? accountTypeTase;
        /// <summary>
        /// סיווג חשבון לבורס
        /// </summary>
        public int? AccountTypeTase
        {
            get => this.accountTypeTase;
            set
            {
                base.SetProperty(value);
                this.accountTypeTase = value;
            }
        }

        private string isManaged;
        /// <summary>
        /// האם החשבון מנוהל ע"י אלטשולר (בעתיד תינתן אפשרות כזו).
        /// ברירת מחדל ריק.
        /// </summary>
        public string IsManaged
        {
            get => this.isManaged;
            set
            {
                base.SetProperty(value);
                this.isManaged = value;
            }
        }

        private string isLimitedAccountList;
        /// <summary>
        /// חשבון במערכת סגורה
        /// </summary>
        public string IsLimitedAccountList
        {
            get => this.isLimitedAccountList;
            set
            {
                base.SetProperty(value);
                this.isLimitedAccountList = value;
            }
        }

        private ESBPortfolioStatementsMaildef statementsMailDef;
        /// <summary>
        /// העדפות דיוורים
        /// </summary>
        [Required]
        public ESBPortfolioStatementsMaildef StatementsMailDef
        {
            get => this.statementsMailDef;
            set
            {
                base.SetProperty(value);
                this.statementsMailDef = value;
            }
        }

        private string isConnectedAccount;
        /// <summary>
        /// צד קשור
        /// </summary>
        public string IsConnectedAccount
        {
            get => this.isConnectedAccount;
            set
            {
                base.SetProperty(value);
                this.isConnectedAccount = value;
            }
        }

        private string riskAccountIndex;
        /// <summary>
        /// מדד סיכון חשבון
        /// </summary>
        [Required]
        public string RiskAccountIndex
        {
            get => this.riskAccountIndex;
            set
            {
                base.SetProperty(value);
                this.riskAccountIndex = value;
            }
        }

        private int? approvedActivityFrame;
        /// <summary>
        /// מסגרת אשראי מאושרת
        /// </summary>
        public int? ApprovedActivityFrame
        {
            get => this.approvedActivityFrame;
            set
            {
                base.SetProperty(value);
                this.approvedActivityFrame = value;
            }
        }

        private string isDepositThirdParty;
        /// <summary>
        /// ביצוע הפקדות ו/או העברות כספים אל/מאת צד ג'
        /// </summary>
        public string IsDepositThirdParty
        {
            get => this.isDepositThirdParty;
            set
            {
                base.SetProperty(value);
                this.isDepositThirdParty = value;
            }
        }

        private string isClientRequestVote;
        /// <summary>
        /// כתבי הצבעה ואסיפות כלליות
        /// </summary>
        public string IsClientRequestVote
        {
            get => this.isClientRequestVote;
            set
            {
                base.SetProperty(value);
                this.isClientRequestVote = value;
            }
        }

        private string isPublic;
        /// <summary>
        /// איש ציבור בארץ או בחו"ל
        /// </summary>
        public string IsPublic
        {
            get => this.isPublic;
            set
            {
                base.SetProperty(value);
                this.isPublic = value;
            }
        }

        private int? agreementTariff;
        /// <summary>
        /// הרשאת שורט ני"ע
        /// </summary>
        [Required]
        public int? AgreementTariff
        {
            get => this.agreementTariff;
            set
            {
                base.SetProperty(value);
                this.agreementTariff = value;
            }
        }

        private int? country;
        /// <summary>
        /// מדינה
        /// </summary>
        [Required]
        public int? Country
        {
            get => this.country;
            set
            {
                base.SetProperty(value);
                this.country = value;
            }
        }

        private string isCompanyEmployee;
        /// <summary>
        /// עובד חברה
        /// </summary>
        public string IsCompanyEmployee
        {
            get => this.isCompanyEmployee;
            set
            {
                base.SetProperty(value);
                this.isCompanyEmployee = value;
            }
        }

        private string city;
        /// <summary>
        /// עיר
        /// </summary>
        [Required]
        public string City
        {
            get => this.city;
            set
            {
                base.SetProperty(value);
                this.city = value;
            }
        }

        private string street;
        /// <summary>
        /// רחוב
        /// </summary>
        [Required]
        public string Street
        {
            get => this.street;
            set
            {
                base.SetProperty(value);
                this.street = value;
            }
        }

        private string address;
        /// <summary>
        /// כתובת כללית
        /// </summary>
        public string Address
        {
            get => this.address;
            set
            {
                base.SetProperty(value);
                this.address = value;
            }
        }

        private string zipCode;
        /// <summary>
        /// מיקוד
        /// </summary>
        [Required]
        public string ZipCode
        {
            get => this.zipCode;
            set
            {
                base.SetProperty(value);
                this.zipCode = value;
            }
        }

        private string mobile;
        /// <summary>
        /// טלפון נייד
        /// </summary>
        [Required]
        public string Mobile
        {
            get => this.mobile;
            set
            {
                base.SetProperty(value);
                this.mobile = value;
            }
        }

        private string work;
        /// <summary>
        /// טלפון במשרד
        /// </summary>
        public string Work
        {
            get => this.work;
            set
            {
                base.SetProperty(value);
                this.work = value;
            }
        }

        private string home;
        /// <summary>
        /// טלפון בבית
        /// </summary>
        public string Home
        {
            get => this.home;
            set
            {
                base.SetProperty(value);
                this.home = value;
            }
        }

        private string email;
        /// <summary>
        /// מייל
        /// </summary>
        [Required]
        public string Email
        {
            get => this.email;
            set
            {
                base.SetProperty(value);
                this.email = value;
            }
        }

        private string isTaxCalcIgnore;
        /// <summary>
        /// פטור מניכוי מס במקור - קבלת הנתון משע"מ / דיווח ידני
        /// </summary>
        [Required]
        public string IsTaxCalcIgnore
        {
            get => this.isTaxCalcIgnore;
            set
            {
                base.SetProperty(value);
                this.isTaxCalcIgnore = value;
            }
        }

        private string kYCDate;
        /// <summary>
        /// תאריך ביצוע הכר את הלקוח
        /// </summary>
        public string KYCDate
        {
            get => this.kYCDate;
            set
            {
                base.SetProperty(value);
                this.kYCDate = value;
            }
        }

        private string publicType;
        /// <summary>
        /// סוג איש ציבור
        /// </summary>
        public string PublicType
        {
            get => this.publicType;
            set
            {
                base.SetProperty(value);
                this.publicType = value;
            }
        }

        private string buildingNum;
        /// <summary>
        /// מספר בניין
        /// </summary>
        public string BuildingNum
        {
            get => this.buildingNum;
            set
            {
                base.SetProperty(value);
                this.buildingNum = value;
            }
        }

        private string aptNum;
        /// <summary>
        /// מספר דירה
        /// </summary>
        public string AptNum
        {
            get => this.aptNum;
            set
            {
                base.SetProperty(value);
                this.aptNum = value;
            }
        }

        private string provisionTaxAcct;
        /// <summary>
        /// סיווג חשבון שבוצעה בגינו הפרשה לחומס
        /// </summary>
        public string ProvisionTaxAcct
        {
            get => this.provisionTaxAcct;
            set
            {
                base.SetProperty(value);
                this.provisionTaxAcct = value;
            }
        }

        private string informationPackage;
        /// <summary>
        /// חבילת מידע ז"א ישראל וחו"ל
        /// </summary>
        public string InformationPackage
        {
            get => this.informationPackage;
            set
            {
                base.SetProperty(value);
                this.informationPackage = value;
            }
        }

        private string identificationPhoneCode;
        /// <summary>
        /// קוד זיהוי טלפוני
        /// </summary>
        public string IdentificationPhoneCode
        {
            get => this.identificationPhoneCode;
            set
            {
                base.SetProperty(value);
                this.identificationPhoneCode = value;
            }
        }

        private string expectedAmountDeposits;
        /// <summary>
        /// סכום הפקדות שוטפות צפוי
        /// </summary>
        public string ExpectedAmountDeposits
        {
            get => this.expectedAmountDeposits;
            set
            {
                base.SetProperty(value);
                this.expectedAmountDeposits = value;
            }
        }

        private string expectedDepositsFrequency;
        /// <summary>
        /// צפי תדירות הפקדת כספים בחשבון
        /// </summary>
        public string ExpectedDepositsFrequency
        {
            get => this.expectedDepositsFrequency;
            set
            {
                base.SetProperty(value);
                this.expectedDepositsFrequency = value;
            }
        }


        private string accountTypeTax;
        /// <summary>
        /// סוג חשבון למיסוי.
        /// ברירת מחדל תושב ישראל בכל הפעלת ממשק.
        /// </summary>
        public string AccountTypeTax
        {
            get => this.accountTypeTax;
            set
            {
                base.SetProperty(value);
                this.accountTypeTax = value;
            }
        }

        private string expectedWithdrawalsFrequency;
        /// <summary>
        /// צפי תדירות משיכת כספים בחשבון
        /// </summary>
        public string ExpectedWithdrawalsFrequency
        {
            get => expectedWithdrawalsFrequency;
            set
            {
                base.SetProperty(value);
                this.expectedWithdrawalsFrequency = value;
            }
        }

        private string expectedAmountWithdrawals;
        /// <summary>
        /// סכום משיכות צפוי מהחשבון
        /// </summary>
        public string ExpectedAmountWithdrawals
        {
            get => expectedAmountWithdrawals;
            set
            {
                base.SetProperty(value);
                this.expectedAmountWithdrawals = value;
            }
        }


        private string isAltConnectedAccount;
        /// <summary>
        /// חשבונות קשורים באלטשולר שחם 
        /// </summary>
        public string IsAltConnectedAccount
        {
            get => this.isAltConnectedAccount;
            set
            {
                base.SetProperty(value);
                this.isAltConnectedAccount = value;
            }
        }

        private string isAccountLien;
        /// <summary>
        /// חשבון משועבד 
        /// </summary>
        public string IsAccountLien
        {
            get => this.isAccountLien;
            set
            {
                base.SetProperty(value);
                this.isAccountLien = value;
            }
        }

        private string loyaltyProgram;
        /// <summary>
        /// מועדון 
        /// </summary>
        public string LoyaltyProgram
        {
            get => this.loyaltyProgram;
            set
            {
                base.SetProperty(value);
                this.loyaltyProgram = value;
            }
        }
    }
}
