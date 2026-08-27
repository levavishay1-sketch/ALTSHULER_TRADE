using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.External.ValidationAttributes;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiLead : ApiEntity
    {
        public const string EntityLogicalName = "lead";
        public ApiLead() : base(EntityLogicalName)
        {
        }

        private string leadIdentityNumber;
        /// <summary>
        /// מזהה ההפניה
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_leadidentitynumber", CrmPropertyType.String)]
        public string LeadIdentityNumber
        {
            get { return leadIdentityNumber; }
            set
            {
                this.SetProperty(value);
                leadIdentityNumber = value;
                this.SetEntityKeys("alt_leadidentitynumber", value);
            }
        }

        private string mobilePhone;
        /// <summary>
        /// נייד
        /// </summary>
        [CrmEntityMapper("mobilephone", CrmPropertyType.String)]
        public string MobilePhone
        {
            get { return mobilePhone; }
            set
            {
                this.SetProperty(value);
                mobilePhone = value;
            }
        }

        private int? leadSourceCode;
        /// <summary>
        /// מקור הפניה
        /// </summary>
        [OptionSetAvailableValues(new[] { "2", "4", "7", "15" })]
        [CrmEntityMapper("leadsourcecode", CrmPropertyType.OptionSet)]
        public int? LeadSourceCode
        {
            get { return leadSourceCode; }
            set
            {
                this.SetProperty(value);
                leadSourceCode = value;
            }
        }

        private string fullName;
        /// <summary>
        /// שם מלא
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("fullname", CrmPropertyType.String, MappToCrm = false)]
        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {
                this.SetProperty(value);
                this.fullName = value;
            }
        }

        private string firstName;
        /// <summary>
        /// שם פרטי
        /// </summary>
        [StringLength(50)]
        [CrmEntityMapper("firstname", CrmPropertyType.String)]
        public string FirstName
        {
            get { return firstName; }
            set
            {
                this.SetProperty(value);
                firstName = value;
            }
        }

        private string lastName;
        /// <summary>
        /// שם משפחה
        /// </summary>
        [StringLength(50)]
        [CrmEntityMapper("lastname", CrmPropertyType.String)]
        public string LastName
        {
            get { return lastName; }
            set
            {
                this.SetProperty(value);
                lastName = value;
            }
        }

        private string campaignName;
        /// <summary>
        /// שם קמפיין
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_campaignname", CrmPropertyType.String)]
        public string CampaignName
        {
            get { return campaignName; }
            set
            {
                this.SetProperty(value);
                campaignName = value;
            }
        }

        private string identityNumber;
        /// <summary>
        /// מספר מזהה לקוח
        /// </summary>
        [StringLength(9)]
        [CrmEntityMapper("alt_identitynumber", CrmPropertyType.String)]
        public string IdentityNumber
        {
            get { return identityNumber; }
            set
            {
                this.SetProperty(value);
                identityNumber = value;
            }
        }

        private string marketingMedium;
        /// <summary>
        /// מדיום
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_marketingmedium", CrmPropertyType.String)]
        public string MarketingMedium
        {
            get { return marketingMedium; }
            set
            {
                this.SetProperty(value);
                marketingMedium = value;
            }
        }

        private string marketingPageType;
        /// <summary>
        /// סוג עמוד
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_marketingpagetype", CrmPropertyType.String)]
        public string MarketingPageType
        {
            get { return marketingPageType; }
            set
            {
                this.SetProperty(value);
                marketingPageType = value;
            }
        }

        private string marketingPhrase;
        /// <summary>
        /// ביטוי
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_marketingphrase", CrmPropertyType.String)]
        public string MarketingPhrase
        {
            get { return marketingPhrase; }
            set
            {
                this.SetProperty(value);
                marketingPhrase = value;
            }
        }

        private string externalIdentifier;
        /// <summary>
        /// מזהה חיצוני
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_externalidentifier", CrmPropertyType.String)]
        public string ExternalIdentifier
        {
            get { return externalIdentifier; }
            set
            {
                this.SetProperty(value);
                externalIdentifier = value;
            }
        }

        private string marketingSource;
        /// <summary>
        /// מקור
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_marketingsource", CrmPropertyType.String)]
        public string MarketingSource
        {
            get { return marketingSource; }
            set
            {
                this.SetProperty(value);
                marketingSource = value;
            }
        }

        private string emailAddress1;
        /// <summary>
        /// דואר אלקטרוני
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("emailaddress1", CrmPropertyType.String)]
        public string EmailAddress1
        {
            get
            {
                return emailAddress1;
            }
            set
            {
                this.SetProperty(value);
                this.emailAddress1 = value;
            }
        }

        private string companyName;
        /// <summary>
        /// שם חברה
        /// </summary>
        [StringLength(250)]
        [CrmEntityMapper("companyname", CrmPropertyType.String)]
        public string CompanyName
        {
            get { return companyName; }
            set
            {
                this.SetProperty(value);
                companyName = value;
            }
        }

        private string digitalFormLink;
        /// <summary>
        /// לינק לטופס דיגיטלי
        /// </summary>
        [StringLength(250)]
        [CrmEntityMapper("alt_digitalformlink", CrmPropertyType.String)]
        public string DigitalFormLink
        {
            get { return digitalFormLink; }
            set
            {
                this.SetProperty(value);
                digitalFormLink = value;
            }
        }

        private string description;
        /// <summary>
        /// תיאור
        /// </summary>
        [CrmEntityMapper("description", CrmPropertyType.String)]
        [StringLength(2000)]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.SetProperty(value);
                this.description = value;
            }
        }

        private int? marketingClub;
        /// <summary>
        /// מועדון
        /// </summary>
        [CrmEntityMapper("alt_marketingclub", CrmPropertyType.Int)]
        public int? MarketingClub
        {
            get { return marketingClub; }
            set
            {
                this.SetProperty(value);
                marketingClub = value;
            }
        }

        private ApiContact parentContactId;
        [CrmEntityMapper("parentcontactid", CrmPropertyType.EntityReference)]
        public ApiContact ParentContactId
        {
            get { return parentContactId; }
            set
            {
                this.SetProperty(value);
                parentContactId = value;
            }
        }

        private ApiContact parentAccountId;
        [CrmEntityMapper("parentaccountid", CrmPropertyType.EntityReference)]
        public ApiContact ParentAccountId
        {
            get { return parentAccountId; }
            set
            {
                this.SetProperty(value);
                parentAccountId = value;
            }
        }

        private ApiOpportunity qualifyingOpportunityId;
        [CrmEntityMapper("qualifyingopportunityid", CrmPropertyType.EntityReference)]
        public ApiOpportunity QualifyingOpportunityId
        {
            get { return qualifyingOpportunityId; }
            set
            {
                this.SetProperty(value);
                qualifyingOpportunityId = value;
            }
        }

        private string refferingCustomerAccountNumber;
        /// <summary>
        /// מספר חשבון הלקוח הממליץ
        /// </summary>
        [StringLength(20)]
        [CrmEntityMapper("alt_refferingcustomeraccountnumber", CrmPropertyType.String)]
        public string RefferingCustomerAccountNumber
        {
            get { return refferingCustomerAccountNumber; }
            set
            {
                this.SetProperty(value);
                refferingCustomerAccountNumber = value;
            }
        }

        private bool? sentToIVRBit;
        /// <summary>
        /// נשלח לחייגן
        /// </summary>
        [CrmEntityMapper("alt_senttoivrbit", CrmPropertyType.Bool)]
        public bool? SentToIVRBit
        {
            get { return sentToIVRBit; }
            set
            {
                this.SetProperty(value);
                sentToIVRBit = value;
            }
        }

        private int? iVRCampaignCode;
        /// <summary>
        /// קמפיין ב-IVR 
        /// </summary>
        [CrmEntityMapper("alt_ivrcampaigncode", CrmPropertyType.OptionSet)]
        public int? IVRCampaignCode
        {
            get { return iVRCampaignCode; }
            set
            {
                this.SetProperty(value);
                iVRCampaignCode = value;
            }
        }

        private int? totalMissedPhoneCallsTodayInt;
        /// <summary>
        /// סה"כ שיחות שלא נענו מחייגן היום
        /// </summary>
        [CrmEntityMapper("alt_totalmissedphonecallstodayint", CrmPropertyType.Int)]
        public int? TotalMissedPhoneCallsTodayInt
        {
            get { return totalMissedPhoneCallsTodayInt; }
            set
            {
                this.SetProperty(value);
                totalMissedPhoneCallsTodayInt = value;
            }
        }
    }
}
