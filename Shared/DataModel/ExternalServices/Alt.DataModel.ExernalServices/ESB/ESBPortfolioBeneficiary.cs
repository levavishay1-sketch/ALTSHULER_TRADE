using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class PortfolioBeneficiary : ExternalEntityBase
    {
        private string beneficiaryClientID;
        /// <summary>
        /// מזהה לקוח
        /// </summary>
        [Required]
        [JsonPropertyName("Beneficiary/ClientID")]
        public string BeneficiaryClientID
        {
            get => this.beneficiaryClientID;
            set
            {
                base.SetProperty(value);
                this.beneficiaryClientID = value;
            }
        }

        private string iDType;
        /// <summary>
        /// סוג אמצעי זיהוי
        /// </summary>
        [Required]
        public string IDType
        {
            get => this.iDType;
            set
            {
                base.SetProperty(value);
                this.iDType = value;
            }
        }

        private string issueDate;
        /// <summary>
        /// ת.הנפקה ראשי
        /// </summary>
        [Required]
        public string IssueDate
        {
            get => this.issueDate;
            set
            {
                base.SetProperty(value);
                this.issueDate = value;
            }
        }

        private string checkedDate;
        /// <summary>
        /// ת.בדיקת תעודה
        /// </summary>
        [Required]
        public string CheckedDate
        {
            get => this.checkedDate;
            set
            {
                base.SetProperty(value);
                this.checkedDate = value;
            }
        }

        private string secIssueDate;
        /// <summary>
        /// תאריך הנפקה משני
        /// </summary>
        public string SecIssueDate
        {
            get => this.secIssueDate;
            set
            {
                base.SetProperty(value);
                this.secIssueDate = value;
            }
        }

        private string identificationMethod;
        /// <summary>
        /// שיטת זיהוי
        /// </summary>
        [Required]
        public string IdentificationMethod
        {
            get => this.identificationMethod;
            set
            {
                base.SetProperty(value);
                this.identificationMethod = value;
            }
        }

        private string secIDNO;
        /// <summary>
        /// מספר זיהוי משני
        /// </summary>
        public string SecIDNO
        {
            get => this.secIDNO;
            set
            {
                base.SetProperty(value);
                this.secIDNO = value;
            }
        }

        private string firstName;
        /// <summary>
        /// שם פרטי/תאגיד עברית
        /// </summary>
        [Required]
        public string FirstName
        {
            get => this.firstName;
            set
            {
                base.SetProperty(value);
                this.firstName = value;
            }
        }

        private string lastName;
        /// <summary>
        /// שם משפחה עברית
        /// </summary>
        [Required]
        public string LastName
        {
            get => this.lastName;
            set
            {
                base.SetProperty(value);
                this.lastName = value;
            }
        }

        private string firstNameEnglish;
        /// <summary>
        /// שם פרטי/תאגיד אנגלית
        /// </summary>
        public string FirstNameEnglish
        {
            get => this.firstNameEnglish;
            set
            {
                base.SetProperty(value);
                this.firstNameEnglish = value;
            }
        }

        private string lastNameEnglish;
        /// <summary>
        /// שם משפחה אנגלית
        /// </summary>
        public string LastNameEnglish
        {
            get => this.lastNameEnglish;
            set
            {
                base.SetProperty(value);
                this.lastNameEnglish = value;
            }
        }

        private string dateOfBirth;
        /// <summary>
        /// תאריך לידה/התאגדות
        /// </summary>
        [Required]
        public string DateOfBirth
        {
            get => this.dateOfBirth;
            set
            {
                base.SetProperty(value);
                this.dateOfBirth = value;
            }
        }

        private string gender;
        /// <summary>
        /// מין
        /// 
        /// </summary>
        [Required]
        public string Gender
        {
            get => this.gender;
            set
            {
                base.SetProperty(value);
                this.gender = value;
            }
        }

        private string proNonPro;
        /// <summary>
        /// סוג משתמש לקבלת נתוני שוק
        /// </summary>
        [Required]
        public string ProNonPro
        {
            get => this.proNonPro;
            set
            {
                base.SetProperty(value);
                this.proNonPro = value;
            }
        }

        private string acctRelationType;
        /// <summary>
        /// סוג הקשר
        /// </summary>
        [Required]
        public string AcctRelationType
        {
            get => this.acctRelationType;
            set
            {
                base.SetProperty(value);
                this.acctRelationType = value;
            }
        }

        private string isIsraeliResidentDeclare;
        /// <summary>
        /// הצהרת תושבות ישראלית
        /// </summary>
        [Required]
        public string IsIsraeliResidentDeclare
        {
            get => this.isIsraeliResidentDeclare;
            set
            {
                base.SetProperty(value);
                this.isIsraeliResidentDeclare = value;
            }
        }

        private string isReturningResident;
        /// <summary>
        /// הצהרת תושב חוזר
        /// </summary>
        public string IsReturningResident
        {
            get => this.isReturningResident;
            set
            {
                base.SetProperty(value);
                this.isReturningResident = value;
            }
        }

        private string isUSPersonResident;
        /// <summary>
        /// הצהרת לקוח US person
        /// הערכים עוברים בפורמט Y/N. נדרש להעביר ערך ברירת מחדל Y
        /// </summary>
        [Required]
        public string IsUSPersonResident
        {
            get => this.isUSPersonResident;
            set
            {
                base.SetProperty(value);
                this.isUSPersonResident = value;
            }
        }

        private string isStateRiskRelation;
        /// <summary>
        /// קשרי מסחר עם מדינות בסיכון
        /// </summary>
        public string IsStateRiskRelation
        {
            get => this.isStateRiskRelation;
            set
            {
                base.SetProperty(value);
                this.isStateRiskRelation = value;
            }
        }

        private string joinDate;
        /// <summary>
        /// ת.ביצוע שאלון KYC
        /// תאריך סיום מילוי טופס (תאריך יצירת הסבב אישורים).
        /// פורמט התאריך: DD.MM.YYYY
        /// </summary>
        public string JoinDate
        {
            get => this.joinDate;
            set
            {
                base.SetProperty(value);
                this.joinDate = value;
            }
        }

        private int? relatedCountry;
        /// <summary>
        /// מדינה
        /// להעביר ערך קבוע "ישראל"
        /// </summary>
        [Required]
        public int? RelatedCountry
        {
            get => this.relatedCountry;
            set
            {
                base.SetProperty(value);
                this.relatedCountry = value;
            }
        }

        private string relatedStreet;
        /// <summary>
        /// רחוב
        /// </summary>
        [Required]
        public string RelatedStreet
        {
            get => this.relatedStreet;
            set
            {
                base.SetProperty(value);
                this.relatedStreet = value;
            }
        }

        private string relatedAddress;
        /// <summary>
        /// כתובת כללית
        /// </summary>     
        public string RelatedAddress
        {
            get => this.relatedAddress;
            set
            {
                base.SetProperty(value);
                this.relatedAddress = value;
            }
        }

        private string relatedZipCode;
        /// <summary>
        /// מיקוד
        /// </summary>
        [Required]
        public string RelatedZipCode
        {
            get => this.relatedZipCode;
            set
            {
                base.SetProperty(value);
                this.relatedZipCode = value;
            }
        }

        private string relatedEmail;
        /// <summary>
        /// מייל
        /// </summary>
        [Required]
        public string RelatedEmail
        {
            get => this.relatedEmail;
            set
            {
                base.SetProperty(value);
                this.relatedEmail = value;
            }
        }

        private string relatedMobile;
        /// <summary>
        /// טלפון נייד
        /// </summary>
        [Required]
        public string RelatedMobile
        {
            get => this.relatedMobile;
            set
            {
                base.SetProperty(value);
                this.relatedMobile = value;
            }
        }

        private string employmentStatus;
        /// <summary>
        /// סטטוס תעסוקתי
        /// </summary>
        public string EmploymentStatus
        {
            get => this.employmentStatus;
            set
            {
                base.SetProperty(value);
                this.employmentStatus = value;
            }
        }

        private string otherEmploymentStatus;
        /// <summary>
        /// סטטוס תעסוקתי אחר
        /// </summary>
        public string OtherEmploymentStatus
        {
            get => this.otherEmploymentStatus;
            set
            {
                base.SetProperty(value);
                this.otherEmploymentStatus = value;
            }
        }

        private string incomeSources;
        /// <summary>
        /// מקורות הכנסה/כספים שיופקדו
        /// </summary>
        public string IncomeSources
        {
            get => this.incomeSources;
            set
            {
                base.SetProperty(value);
                this.incomeSources = value;
            }
        }

        private string otherIncomeSources;
        /// <summary>
        /// מקור הכנסה/כספים אחר
        /// </summary>
        public string OtherIncomeSources
        {
            get => this.otherIncomeSources;
            set
            {
                base.SetProperty(value);
                this.otherIncomeSources = value;
            }
        }

        private string relatedCity;
        /// <summary>
        /// עיר
        /// </summary>
        [Required]
        public string RelatedCity
        {
            get => this.relatedCity;
            set
            {
                base.SetProperty(value);
                this.relatedCity = value;
            }
        }

        private int? countryOfBirth;
        /// <summary>
        /// ארץ לידה
        /// </summary>
        [Required]
        public int? CountryOfBirth
        {
            get => this.countryOfBirth;
            set
            {
                base.SetProperty(value);
                this.countryOfBirth = value;
            }
        }

        private int? issuingCountry;
        /// <summary>
        /// מדינה מנפיקה ת.ז ראשי
        /// </summary>
        [Required]
        public int? IssuingCountry
        {
            get => this.issuingCountry;
            set
            {
                base.SetProperty(value);
                this.issuingCountry = value;
            }
        }

        private int? secIssuingCountry;
        /// <summary>
        /// מדינה מנפיקה ת.ז משני
        /// </summary>
        public int? SecIssuingCountry
        {
            get => this.secIssuingCountry;
            set
            {
                base.SetProperty(value);
                this.secIssuingCountry = value;
            }
        }

        //private string isClientRequestVote;
        ///// <summary>
        ///// השתתפות באסיפות
        ///// </summary>
        //public string IsClientRequestVote
        //{
        //    get => this.isClientRequestVote;
        //    set
        //    {
        //        base.SetProperty(value);
        //        this.isClientRequestVote = value;
        //    }
        //}

        private string relatedBuildingNum;
        /// <summary>
        /// מספר בניין
        /// </summary>
        public string RelatedBuildingNum
        {
            get => this.relatedBuildingNum;
            set
            {
                base.SetProperty(value);
                this.relatedBuildingNum = value;
            }
        }

        private string relatedAptNum;
        /// <summary>
        /// מספר דירה
        /// </summary>
        public string RelatedAptNum
        {
            get => this.relatedAptNum;
            set
            {
                base.SetProperty(value);
                this.relatedAptNum = value;
            }
        }

        private string relatedWork;
        /// <summary>
        /// טלפון במשרד
        /// </summary>
        public string RelatedWork
        {
            get => this.relatedWork;
            set
            {
                base.SetProperty(value);
                this.relatedWork = value;
            }
        }

        private string accpetCreditMonitorReport;
        /// <summary>
        /// הסכמת לקוח לדוח אשראי וניטור בק"ע אשראי בחשבון
        /// </summary>
        public string AccpetCreditMonitorReport
        {
            get => this.accpetCreditMonitorReport;
            set
            {
                base.SetProperty(value);
                this.accpetCreditMonitorReport = value;
            }
        }

        private string relatedHome;
        /// <summary>
        /// טלפון בבית
        /// </summary>
        public string RelatedHome
        {
            get => this.relatedHome;
            set
            {
                base.SetProperty(value);
                this.relatedHome = value;
            }
        }

        private string trade1Acct;
        /// <summary>
        /// שם משתמש בטרייד1
        /// </summary>
        public string Trade1Acct
        {
            get => this.trade1Acct;
            set
            {
                base.SetProperty(value);
                this.trade1Acct = value;
            }
        }

        private int? restrictedCountries;
        /// <summary>
        /// מדינות אסורות למסחר
        /// </summary>
        public int? RestrictedCountries
        {
            get => this.restrictedCountries;
            set
            {
                base.SetProperty(value);
                this.restrictedCountries = value;
            }
        }

        private string restrictedReason;
        /// <summary>
        /// תאור קשר
        /// </summary>
        public string Restricted_Reason
        {
            get => this.restrictedReason;
            set
            {
                base.SetProperty(value);
                this.restrictedReason = value;
            }
        }

        private string iDTypeSec;
        public string IDTypeSec
        {
            get => this.iDTypeSec;
            set
            {
                base.SetProperty(value);
                this.iDTypeSec = value;
            }
        }


        private string isActive;
        /// <summary>
        /// האם מיופה כוח פעיל / לא פעיל.
        /// ברירת מחדל כן
        /// </summary>
        [Required]
        public string IsActive
        {
            get => this.isActive;
            set
            {
                base.SetProperty(value);
                this.isActive = value;
            }
        }

        private ESBPortfolioRelatedPersonIncome relatedPersonIncome;
        [Required]
        public ESBPortfolioRelatedPersonIncome RelatedPersonIncome
        {
            get => this.relatedPersonIncome;
            set
            {
                base.SetProperty(value);
                this.relatedPersonIncome = value;
            }
        }

        private string isMainOwner;
        /// <summary>
        /// האם בעל חשבון ראשי
        /// </summary>
        public string IsMainOwner
        {
            get => this.isMainOwner;
            set
            {
                base.SetProperty(value);
                this.isMainOwner = value;
            }
        }
    }
}
