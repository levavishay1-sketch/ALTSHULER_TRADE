using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiAccountHolder : ApiEntity
    {
        public const string EntityLogicalName = "alt_accountholder";
        public ApiAccountHolder() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם 
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


        private string identificationNumber;
        /// <summary>
        /// מספר זיהוי ראשי
        /// </summary>
        [CrmEntityMapper("alt_identificationnumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string IdentificationNumber
        {
            get
            {
                return identificationNumber;
            }
            set
            {
                this.SetProperty(value);
                this.identificationNumber = value;
            }
        }

        private string mobilePhone;
        /// <summary>
        /// טלפון נייד
        /// </summary>
        [CrmEntityMapper("alt_mobilephone", CrmPropertyType.String)]
        [StringLength(100)]
        public string MobilePhone
        {
            get
            {
                return mobilePhone;
            }
            set
            {
                this.SetProperty(value);
                this.mobilePhone = value;
            }
        }

        private string homePhone;
        /// <summary>
        /// טלפון בית
        /// </summary>
        [CrmEntityMapper("alt_homephone", CrmPropertyType.String)]
        [StringLength(100)]
        public string HomePhone
        {
            get
            {
                return homePhone;
            }
            set
            {
                this.SetProperty(value);
                this.homePhone = value;
            }
        }

        private string email;
        /// <summary>
        /// כתובת דוא"ל
        /// </summary>
        [CrmEntityMapper("alt_email", CrmPropertyType.String)]
        [StringLength(100)]
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                this.SetProperty(value);
                this.email = value;
            }
        }

        private string firstName;
        /// <summary>
        /// שם פרטי
        /// </summary>
        [StringLength(50)]
        [CrmEntityMapper("alt_firstname", CrmPropertyType.String)]
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                this.SetProperty(value);
                this.firstName = value;
            }
        }

        private string lastName;
        /// <summary>
        /// שם משפחה
        /// </summary>
        [StringLength(50)]
        [CrmEntityMapper("alt_lastname", CrmPropertyType.String)]
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                this.SetProperty(value);
                this.lastName = value;
            }
        }

        private string transferToTradeOneErrorDescription;
        /// <summary>
        /// פירוט כישלון שידור לטרייד 1
        /// </summary>
        [CrmEntityMapper("alt_transfertotradeoneerrordescription", CrmPropertyType.String)]
        public string TransferToTradeOneErrorDescription
        {
            get
            {
                return transferToTradeOneErrorDescription;
            }
            set
            {
                this.SetProperty(value);
                this.transferToTradeOneErrorDescription = value;
            }
        }

        private int? identificationTypeCode;
        /// <summary>
        /// סוג זיהוי ראשי
        /// </summary>
        [CrmEntityMapper("alt_identificationtypecode", CrmPropertyType.OptionSet)]
        [Range(1, 7)]
        public int? IdentificationTypeCode
        {
            get
            {
                return identificationTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.identificationTypeCode = value;
            }
        }

        private int? secondIdentificationTypeCode;
        /// <summary>
        /// סוג זיהוי משני
        /// </summary>
        [CrmEntityMapper("alt_secondidentificationtypecode", CrmPropertyType.OptionSet)]
        [Range(1, 7)]
        public int? SecondIdentificationTypeCode
        {
            get
            {
                return secondIdentificationTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.secondIdentificationTypeCode = value;
            }
        }

        private int? accountHolderTypeCode;
        /// <summary>
        /// סוג בעל חשבון
        /// </summary>
        [CrmEntityMapper("alt_accountholdertypecode", CrmPropertyType.OptionSet)]
        [Range(1, 12)]
        public int? AccountHolderTypeCode
        {
            get
            {
                return accountHolderTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.accountHolderTypeCode = value;
            }
        }

        private int? accountHolderStatusCode;
        /// <summary>
        /// סטטוס בעל חשבון
        /// </summary>
        [CrmEntityMapper("alt_accountholderstatuscode", CrmPropertyType.OptionSet)]
        [Range(1, 4)]
        public int? AccountHolderStatusCode
        {
            get
            {
                return accountHolderStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.accountHolderStatusCode = value;
            }
        }

        private int? genderCode;
        /// <summary>
        /// מין
        /// </summary>
        [Range(1, 2)]
        [CrmEntityMapper("alt_gendercode", CrmPropertyType.OptionSet)]
        public int? GenderCode
        {
            get
            {
                return genderCode;
            }
            set
            {
                this.SetProperty(value);
                this.genderCode = value;
            }
        }

        private DateTime? birthDate;
        /// <summary>
        /// תאריך לידה
        /// </summary>
        [CrmEntityMapper("alt_birthdate", CrmPropertyType.DateTime)]
        public DateTime? BirthDate
        {
            get
            {
                return birthDate;
            }
            set
            {
                this.SetProperty(value);
                this.birthDate = value;
            }
        }


        private ApiCity city;
        /// <summary>
        /// עיר
        /// </summary>
        [CrmEntityMapper("alt_cityid", CrmPropertyType.EntityReference)]
        public ApiCity City
        {
            get
            {
                return city;
            }
            set
            {
                this.SetProperty(value);
                this.city = value;
            }
        }

        private ApiStreet street;
        /// <summary>
        /// רחוב
        /// </summary>
        [CrmEntityMapper("alt_streetid", CrmPropertyType.EntityReference)]
        public ApiStreet Street
        {
            get
            {
                return street;
            }
            set
            {
                this.SetProperty(value);
                this.street = value;
            }
        }

        private string firstNameEng;
        /// <summary>
        /// שם פרטי באנגלית
        /// </summary>
        [CrmEntityMapper("alt_firstnameeng", CrmPropertyType.String)]
        [StringLength(100)]
        public string FirstNameEng
        {
            get
            {
                return firstNameEng;
            }
            set
            {
                this.SetProperty(value);
                this.firstNameEng = value;
            }
        }

        private string lastNameEng;
        /// <summary>
        /// שם משפחה באנגלית
        /// </summary>
        [CrmEntityMapper("alt_lastnameeng", CrmPropertyType.String)]
        [StringLength(100)]
        public string LastNameEng
        {
            get
            {
                return lastNameEng;
            }
            set
            {
                this.SetProperty(value);
                this.lastNameEng = value;
            }
        }

        private string houseNumber;
        /// <summary>
        /// מספר בית
        /// </summary>
        [CrmEntityMapper("alt_housenumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string HouseNumber
        {
            get
            {
                return houseNumber;
            }
            set
            {
                this.SetProperty(value);
                this.houseNumber = value;
            }
        }

        private string flatNumber;
        /// <summary>
        /// מספר דירה
        /// </summary>
        [CrmEntityMapper("alt_flatnumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string FlatNumber
        {
            get
            {
                return flatNumber;
            }
            set
            {
                this.SetProperty(value);
                this.flatNumber = value;
            }
        }

        private string postalCode;
        /// <summary>
        /// מיקוד
        /// </summary>
        [CrmEntityMapper("alt_postalcode", CrmPropertyType.String)]
        [StringLength(100)]
        public string PostalCode
        {
            get
            {
                return postalCode;
            }
            set
            {
                this.SetProperty(value);
                this.postalCode = value;
            }
        }

        private string workPhone;
        /// <summary>
        /// טלפון עבודה
        /// </summary>
        [CrmEntityMapper("alt_workphone", CrmPropertyType.String)]
        public string WorkPhone
        {
            get
            {
                return workPhone;
            }
            set
            {
                this.SetProperty(value);
                this.workPhone = value;
            }
        }
        private string address;
        /// <summary>
        /// כתובת מאוחדת
        /// </summary>
        [CrmEntityMapper("alt_address", CrmPropertyType.String)]
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                this.SetProperty(value);
                this.address = value;
            }
        }

        private string userNameTrade;
        /// <summary>
        /// יוזר בטרייד 1
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_usernametrade", CrmPropertyType.String)]
        public string UserNameTrade
        {
            get
            {
                return userNameTrade;
            }
            set
            {
                this.SetProperty(value);
                this.userNameTrade = value;
            }
        }

        private string beneficiaryDeclarationTranscriptAutentix;
        /// <summary>
        /// תמלול הצהרת נהנה מאותנטיקס
        /// </summary>
        [CrmEntityMapper("alt_beneficiarydeclarationtranscriptautentix", CrmPropertyType.String)]
        public string BeneficiaryDeclarationTranscriptAutentix
        {
            get
            {
                return beneficiaryDeclarationTranscriptAutentix;
            }
            set
            {
                this.SetProperty(value);
                this.beneficiaryDeclarationTranscriptAutentix = value;
            }
        }

        private ApiCountry country;
        /// <summary>
        /// מדינה
        /// </summary>
        [CrmEntityMapper("alt_countryid", CrmPropertyType.EntityReference)]
        public ApiCountry Country
        {
            get
            {
                return country;
            }
            set
            {
                this.SetProperty(value);
                this.country = value;
            }
        }

        private ApiCountry birthCountry;
        /// <summary>
        /// ארץ לידה
        /// </summary>
        [CrmEntityMapper("alt_birthcountryid", CrmPropertyType.EntityReference)]
        public ApiCountry BirthCountry
        {
            get
            {
                return birthCountry;
            }
            set
            {
                this.SetProperty(value);
                this.birthCountry = value;
            }
        }

        private ApiCustomer customerId;
        /// <summary>
        /// לקוח
        /// </summary>
        [CrmEntityMapper("alt_customerid", CrmPropertyType.EntityReference)]
        public ApiCustomer CustomerId
        {
            get
            {
                return customerId;
            }
            set
            {
                this.SetProperty(value);
                this.customerId = value;
            }
        }

        private ApiDigitalFormVerification digitalFormVerification;
        /// <summary>
        /// בקרת טופס הצטרפות
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

        private string secondaryIdentificationNumber;
        /// <summary>
        /// מספר זיהוי משני
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_secondaryidentificationnumber", CrmPropertyType.String)]
        public string SecondaryIdentificationNumber
        {
            get
            {
                return secondaryIdentificationNumber;
            }
            set
            {
                this.SetProperty(value);
                this.secondaryIdentificationNumber = value;
            }
        }

        private bool? allowMarketingContent;
        /// <summary>
        /// אישור קבלת תוכן שיווקי
        /// </summary>
        [CrmEntityMapper("alt_allowmarketingcontentbit", CrmPropertyType.Bool)]
        public bool? AllowMarketingContent
        {
            get
            {
                return allowMarketingContent;
            }
            set
            {
                this.SetProperty(value);
                this.allowMarketingContent = value;
            }
        }

        private bool? creditReportCustomerApproval;
        /// <summary>
        /// הסכמת לקוח לדוח אשראי
        /// </summary>
        [CrmEntityMapper("alt_creditreportcustomerapprovalbit", CrmPropertyType.Bool)]
        public bool? CreditReportCustomerApproval
        {
            get
            {
                return creditReportCustomerApproval;
            }
            set
            {
                this.SetProperty(value);
                this.creditReportCustomerApproval = value;
            }
        }

        private bool? mainAccountHolder;
        /// <summary>
        /// בעל חשבון ראשי
        /// </summary>
        [CrmEntityMapper("alt_mainaccountholderbit", CrmPropertyType.Bool)]
        public bool? MainAccountHolder
        {
            get
            {
                return mainAccountHolder;
            }
            set
            {
                this.SetProperty(value);
                this.mainAccountHolder = value;
            }
        }

        private bool? israeliResidencyBit;
        /// <summary>
        /// תושבות ישראלית
        /// </summary>
        [CrmEntityMapper("alt_israeliresidencybit", CrmPropertyType.Bool)]
        public bool? IsraeliResidency
        {
            get
            {
                return israeliResidencyBit;
            }
            set
            {
                this.SetProperty(value);
                this.israeliResidencyBit = value;
            }
        }

        private bool? usPersonDeclarationBit;
        /// <summary>
        /// הצהרת US Person
        /// </summary>
        [CrmEntityMapper("alt_uspersondeclarationbit", CrmPropertyType.Bool)]
        public bool? USPersonDeclaration
        {
            get
            {
                return usPersonDeclarationBit;
            }
            set
            {
                this.SetProperty(value);
                this.usPersonDeclarationBit = value;
            }
        }

        private bool? changeForeignTaxResidencyBit;
        /// <summary>
        /// תיקון הצהרת קיום תשובות מס זרה 
        /// </summary>
        [CrmEntityMapper("alt_changeforeigntaxresidencybit", CrmPropertyType.Bool)]
        public bool? ChangeForeignTaxResidency
        {
            get
            {
                return changeForeignTaxResidencyBit;
            }
            set
            {
                this.SetProperty(value);
                this.changeForeignTaxResidencyBit = value;
            }
        }

        private bool? foreignTaxResidencyBit;
        /// <summary>
        /// קיימת תושבות מס זרה 
        /// </summary>
        [CrmEntityMapper("alt_foreigntaxresidencybit", CrmPropertyType.Bool)]
        public bool? ForeignTaxResidency
        {
            get
            {
                return foreignTaxResidencyBit;
            }
            set
            {
                this.SetProperty(value);
                this.foreignTaxResidencyBit = value;
            }
        }

        private bool? changeUSPersonDeclarationBit;
        /// <summary>
        /// תיקון תשובות US Person 
        /// </summary>
        [CrmEntityMapper("alt_changeuspersondeclarationbit", CrmPropertyType.Bool)]
        public bool? ChangeUSPersonDeclaration
        {
            get
            {
                return changeUSPersonDeclarationBit;
            }
            set
            {
                this.SetProperty(value);
                this.changeUSPersonDeclarationBit = value;
            }
        }

        private bool? changeIsraeliResidencyBit;
        /// <summary>
        /// תיקון תושבות ישראלית 
        /// </summary>
        [CrmEntityMapper("alt_changeisraeliresidencybit", CrmPropertyType.Bool)]
        public bool? ChangeIsraeliResidency
        {
            get
            {
                return changeIsraeliResidencyBit;
            }
            set
            {
                this.SetProperty(value);
                this.changeIsraeliResidencyBit = value;
            }
        }

        private int? beneficiaryDeclarationCode;
        /// <summary>
        /// הצהרה על נהנה
        /// </summary>
        [Range(1, 2)]
        [CrmEntityMapper("alt_beneficiarydeclarationcode", CrmPropertyType.OptionSet)]
        public int? BeneficiaryDeclarationCode
        {
            get
            {
                return beneficiaryDeclarationCode;
            }
            set
            {
                this.SetProperty(value);
                this.beneficiaryDeclarationCode = value;
            }
        }


        private int? beneficiarySigningDeclarationCode;
        /// <summary>
        /// חתימה על הצהרה על נהנה
        /// </summary>
        [CrmEntityMapper("alt_beneficiarysigningdeclarationcode", CrmPropertyType.OptionSet)]
        [Range(1, 4)]
        public int? BeneficiarySigningDeclarationCode
        {
            get
            {
                return beneficiarySigningDeclarationCode;
            }
            set
            {
                this.SetProperty(value);
                this.beneficiarySigningDeclarationCode = value;
            }
        }

        private int? userCharacteristicCode;
        /// <summary>
        /// הצהרה על אופי המשתמש
        /// </summary>
        [Range(1, 2)]
        [CrmEntityMapper("alt_usercharacteristiccode", CrmPropertyType.OptionSet)]
        public int? UserCharacteristicCode
        {
            get
            {
                return userCharacteristicCode;
            }
            set
            {
                this.SetProperty(value);
                this.userCharacteristicCode = value;
            }
        }

        private int? digitalVisualRecognitionCode;
        /// <summary>
        /// תוצאת זיהוי חזותי דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalvisualrecognitioncode", CrmPropertyType.OptionSet)]
        [Range(1, 4)]
        public int? DigitalVisualRecognitionCode
        {
            get
            {
                return digitalVisualRecognitionCode;
            }
            set
            {
                this.SetProperty(value);
                this.digitalVisualRecognitionCode = value;
            }
        }

        private int? performVerificationCode;
        /// <summary>
        /// אופן ביצוע אימות
        /// </summary>
        [CrmEntityMapper("alt_performverificationcode", CrmPropertyType.OptionSet)]
        [Range(1, 4)]
        public int? PerformVerificationCode
        {
            get
            {
                return performVerificationCode;
            }
            set
            {
                this.SetProperty(value);
                this.performVerificationCode = value;
            }
        }

        private int? performAdditionalVerificationCode;
        /// <summary>
        /// אופן ביצוע אימות נוסף 
        /// </summary>
        [Range(1, 2)]
        [CrmEntityMapper("alt_performadditionalverificationcode", CrmPropertyType.OptionSet)]
        public int? PerformAdditionalVerificationCode
        {
            get
            {
                return performAdditionalVerificationCode;
            }
            set
            {
                this.SetProperty(value);
                this.performAdditionalVerificationCode = value;
            }
        }

        private int? checkTerrorOrganizationCode;
        /// <summary>
        /// בדיקת ארגון טרור 
        /// </summary>
        [Range(1, 2)]
        [CrmEntityMapper("alt_checkterrororganizationcode", CrmPropertyType.OptionSet)]
        public int? CheckTerrorOrganizationCode
        {
            get
            {
                return checkTerrorOrganizationCode;
            }
            set
            {
                this.SetProperty(value);
                this.checkTerrorOrganizationCode = value;
            }
        }

        private int? group;
        /// <summary>
        /// קבוצת הרשאות ליוזר מסחר
        /// </summary>
        [CrmEntityMapper("alt_group", CrmPropertyType.OptionSet)]
        public int? Group
        {
            get
            {
                return group;
            }
            set
            {
                this.SetProperty(value);
                this.group = value;
            }
        }

        private DateTime? performVerificationDate;
        /// <summary>
        /// תאריך ביצוע הזיהוי
        /// </summary>
        [CrmEntityMapper("alt_performverificationdate", CrmPropertyType.DateTime)]
        public DateTime? PerformVerificationDate
        {
            get
            {
                return performVerificationDate;
            }
            set
            {
                this.SetProperty(value);
                this.performVerificationDate = value;
            }
        }

        private DateTime? idIssueDate;
        /// <summary>
        /// תאריך הנפקה ת.ז
        /// </summary>
        [CrmEntityMapper("alt_idissuedate", CrmPropertyType.DateTime)]
        public DateTime? IDIssueDate
        {
            get
            {
                return idIssueDate;
            }
            set
            {
                this.SetProperty(value);
                this.idIssueDate = value;
            }
        }

        private DateTime? secondaryIDIssuedDate;
        /// <summary>
        /// תאריך הנפקה ת.ז משני
        /// </summary>
        [CrmEntityMapper("alt_secondaryidissueddate", CrmPropertyType.DateTime)]
        public DateTime? SecondaryIDIssuedDate
        {
            get
            {
                return secondaryIDIssuedDate;
            }
            set
            {
                this.SetProperty(value);
                this.secondaryIDIssuedDate = value;
            }
        }

        private ApiSystemUser checkTerrorOrganizationSystemUserId;
        /// <summary>
        /// גורם מבצע בדיקת ארגון טרור
        /// </summary>
        [CrmEntityMapper("alt_checkterrororganizationsystemuserid", CrmPropertyType.EntityReference)]
        public ApiSystemUser CheckTerrorOrganizationSystemUser
        {
            get
            {
                return checkTerrorOrganizationSystemUserId;
            }
            set
            {
                this.SetProperty(value);
                this.checkTerrorOrganizationSystemUserId = value;
            }
        }

        private ApiSystemUser performVerificationSystemUserId;
        /// <summary>
        /// גורם מבצע הזיהוי
        /// </summary>
        [CrmEntityMapper("alt_performverificationsystemuserid", CrmPropertyType.EntityReference)]
        public ApiSystemUser PerformVerificationSystemUser
        {
            get
            {
                return performVerificationSystemUserId;
            }
            set
            {
                this.SetProperty(value);
                this.performVerificationSystemUserId = value;
            }
        }

        private ApiCountry identificationIssuingCountry;
        /// <summary>
        /// מדינה מנפיקה ת.ז ראשי
        /// </summary>
        [CrmEntityMapper("alt_identificationissuingcountryid", CrmPropertyType.EntityReference)]
        public ApiCountry IdentificationIssuingCountry
        {
            get
            {
                return identificationIssuingCountry;
            }
            set
            {
                this.SetProperty(value);
                this.identificationIssuingCountry = value;
            }
        }

        private ApiCountry secondaryIdentificationIssuingCountry;
        /// <summary>
        /// מדינה מנפיקה ת.ז משני
        /// </summary>
        [CrmEntityMapper("alt_secondaryidentificationissuingcountryid", CrmPropertyType.EntityReference)]
        public ApiCountry SecondaryIdentificationIssuingCountry
        {
            get
            {
                return secondaryIdentificationIssuingCountry;
            }
            set
            {
                this.SetProperty(value);
                this.secondaryIdentificationIssuingCountry = value;
            }
        }

        private ApiAccountHolder spouseAccountHolder;
        /// <summary>
        /// בן/בת זוג שותף בחשבון
        /// </summary>
        [CrmEntityMapper("alt_spouseaccountholderid", CrmPropertyType.EntityReference)]
        public ApiAccountHolder SpouseAccountHolder
        {
            get
            {
                return spouseAccountHolder;
            }
            set
            {
                this.SetProperty(value);
                this.spouseAccountHolder = value;
            }
        }

        private ApiAccountHolder beneficiarySpouseAccountHolder;
        /// <summary>
        /// נהנה בן/ בת זוג
        /// </summary>
        [CrmEntityMapper("alt_beneficiaryspouseaccountholderid", CrmPropertyType.EntityReference)]
        public ApiAccountHolder BeneficiarySpouseAccountHolder
        {
            get
            {
                return beneficiarySpouseAccountHolder;
            }
            set
            {
                this.SetProperty(value);
                this.beneficiarySpouseAccountHolder = value;
            }
        }

        private ApiPortfolio portfolioId;
        /// <summary>
        /// חשבון שנהב
        /// </summary>
        [CrmEntityMapper("alt_portfolioid", CrmPropertyType.EntityReference)]
        public ApiPortfolio Portfolio
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

        private ApiKYC kycId;
        /// <summary>
        /// הכר את הלקוח
        /// </summary>
        public ApiKYC KYC
        {
            get
            {
                return kycId;
            }
            set
            {
                this.SetProperty(value);
                this.kycId = value;
            }
        }

        private string au10tixSessionID;
        /// <summary>
        /// מזהה תהליך באותנטיקס
        /// </summary>
        [StringLength(100)]
        public string AU10tixSessionID
        {
            get
            {
                return au10tixSessionID;
            }
            set
            {
                this.SetProperty(value);
                this.au10tixSessionID = value;
            }
        }


        private string onlineIdentificationNumber;
        /// <summary>
        /// מספר זיהוי מקוון
        /// </summary>
        [CrmEntityMapper("alt_onlineidentificationnumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string OnlineIdentificationNumber
        {
            get
            {
                return onlineIdentificationNumber;
            }
            set
            {
                this.SetProperty(value);
                this.onlineIdentificationNumber = value;
            }
        }


        private int? clubMembershipEligibilityCode;
        /// <summary>
        /// אימות זכאות למועדון
        /// </summary>
        [CrmEntityMapper("alt_clubmembershipeligibilitycode", CrmPropertyType.OptionSet)]
        public int? ClubMembershipEligibilityCode
        {
            get
            {
                return clubMembershipEligibilityCode;
            }
            set
            {
                this.SetProperty(value);
                this.clubMembershipEligibilityCode = value;
            }
        }

        private string feezbackLink;
        /// <summary>
        /// לינק להפקדה ראשונה מפיזבק (פנימי)
        /// </summary>
        [CrmEntityMapper("alt_feezbacklink", CrmPropertyType.String)]
        [StringLength(100)]
        public string FeezbackLink
        {
            get
            {
                return feezbackLink;
            }
            set
            {
                this.SetProperty(value);
                this.feezbackLink = value;
            }
        }

        private bool? sentCustomerAgreementBit;
        [CrmEntityMapper("alt_sentcustomeragreementbit", CrmPropertyType.Bool)]
        public bool? SentCustomerAgreementBit
        {
            get
            {
                return sentCustomerAgreementBit;
            }
            set
            {
                this.SetProperty(value);
                this.sentCustomerAgreementBit = value;
            }
        }
    }
}
