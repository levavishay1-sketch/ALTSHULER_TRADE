using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiDigitalForm : ApiActivityPointer
    {
        public const string EntityLogicalName = "alt_digitalform";
        public ApiDigitalForm() : base(EntityLogicalName)
        {
        }

        private string digitalFormLink;
        /// <summary>
        /// לינק לטופס הדיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformlink", CrmPropertyType.String)]
        [StringLength(250)]
        public string DigitalFormLink
        {
            get
            {
                return digitalFormLink;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormLink = value;
            }
        }

        private string digitalFormDetails;
        /// <summary>
        /// פרטי טופס דיגיטלי - פנימי
        /// </summary>
        [CrmEntityMapper("alt_digitalformdetails", CrmPropertyType.String)]
        public string DigitalFormDetails
        {
            get
            {
                return digitalFormDetails;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormDetails = value;
            }
        }

        private ApiDigitalFormStatus digitalFormStatus;
        /// <summary>
        /// סטטוס טופס דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformstatusid", CrmPropertyType.EntityReference)]
        public ApiDigitalFormStatus DigitalFormStatus
        {
            get
            {
                return digitalFormStatus;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormStatus = value;
            }
        }

        private string digitalFormIdentityNumber;
        /// <summary>
        /// מזהה טופס דיגיטלי
        /// </summary>
        [CrmEntityMapper("alt_digitalformidentitynumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string DigitalFormIdentityNumber
        {
            get
            {
                return digitalFormIdentityNumber;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormIdentityNumber = value;
                this.SetEntityKeys("alt_digitalformidentitynumber", value);
            }
        }

        /// <summary>
        /// סוג טופס דיגיטלי
        /// </summary>
        private int? digitalFormType;
        [CrmEntityMapper("alt_digitalformtypecode", CrmPropertyType.OptionSet)]
        public int? DigitalFormType
        {
            get
            {
                return digitalFormType;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormType = value;
            }
        }

        /// <summary>
        /// סטטוס קליטת נתונים
        /// </summary>
        private int? dataReceptionStatusCode;
        [CrmEntityMapper("alt_datareceptionstatuscode", CrmPropertyType.OptionSet)]
        public int? DataReceptionStatusCode
        {
            get
            {
                return dataReceptionStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.dataReceptionStatusCode = value;
            }
        }

        /// <summary>
        /// סטטוס שידור לאאוטססטם (פנימי)
        /// </summary>
        private int? transferToOutSystemStatusCode;
        [CrmEntityMapper("alt_transfertooutsystemstatuscode", CrmPropertyType.OptionSet)]
        public int? TransferToOutSystemStatusCode
        {
            get
            {
                return transferToOutSystemStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.transferToOutSystemStatusCode = value;
            }
        }

        private int? sourceSystemCode;
        /// <summary>
        /// מערכת מקור
        /// </summary>
        [CrmEntityMapper("alt_sourcesystemcode", CrmPropertyType.OptionSet)]
        public int? SourceSystemCode
        {
            get
            {
                return sourceSystemCode;
            }
            set
            {
                this.SetProperty(value);
                this.sourceSystemCode = value;
            }
        }

        private string customerIdentityNumber;
        /// <summary>
        /// מספר זיהוי לקוח
        /// </summary>
        [CrmEntityMapper("alt_customeridentitynumber", CrmPropertyType.String)]
        public string CustomerIdentityNumber
        {
            get
            {
                return customerIdentityNumber;
            }
            set
            {
                this.SetProperty(value);
                this.customerIdentityNumber = value;
            }
        }

        private bool? abandonedProcessBit;
        /// <summary>
        /// התהליך ננטש?
        /// </summary>
        [CrmEntityMapper("alt_abandonedprocessbit", CrmPropertyType.Bool)]
        public bool? AbandonedProcessBit
        {
            get
            {
                return abandonedProcessBit;
            }
            set
            {
                this.SetProperty(value);
                this.abandonedProcessBit = value;
            }
        }

        private string abandonmentPage;
        /// <summary>
        /// עמוד הנטישה
        /// </summary>
        [CrmEntityMapper("alt_abandonmentpage", CrmPropertyType.String)]
        public string AbandonmentPage
        {
            get
            {
                return abandonmentPage;
            }
            set
            {
                this.SetProperty(value);
                this.abandonmentPage = value;
            }
        }

        private string transferToOutSystemErrorDescription;
        /// <summary>
        /// פירוט כישלון שידור לאאוטססטם
        /// </summary>
        [CrmEntityMapper("alt_transfertooutsystemerrordescription", CrmPropertyType.String)]
        public string TransferToOutSystemErrorDescription
        {
            get
            {
                return transferToOutSystemErrorDescription;
            }
            set
            {
                this.SetProperty(value);
                this.transferToOutSystemErrorDescription = value;
            }
        }

        private bool? sentSecondAbandonmentNoticeBit;
        /// <summary>
        /// האם נשלחה התראה שניה על נטישה (פנימי)
        /// </summary>
        [CrmEntityMapper("alt_sentsecondabandonmentnoticebit", CrmPropertyType.Bool)]
        public bool? SentSecondAbandonmentNoticeBit
        {
            get
            {
                return sentSecondAbandonmentNoticeBit;
            }
            set
            {
                this.SetProperty(value);
                this.sentSecondAbandonmentNoticeBit = value;
            }
        }

        private ApiDigitalFormVerification digitalFormVerification;
        public ApiDigitalFormVerification JoiningForm
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

        private string digitalFormStatusCode;
        /// <summary>
        /// קוד סטטוס טופס דיגיטלי (פנימי)
        /// </summary>
        [CrmEntityMapper("alt_digitalformstatuscode", CrmPropertyType.String)]
        public string DigitalFormStatusCode
        {
            get
            {
                return digitalFormStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormStatusCode = value;
            }
        }

        private ApiIncident regardingIncident;
        public ApiIncident RegardingIncident
        {
            get
            {
                return regardingIncident;
            }
            set
            {
                this.SetProperty(value);
                this.regardingIncident = value;
            }
        }

        private List<ApiActivityParty> customers;
        [CrmEntityMapper("customers", CrmPropertyType.ActivityParty, true, false)]
        public List<ApiActivityParty> Customers
        {
            get
            {
                return customers;
            }
            set
            {
                this.SetProperty(value);
                customers = value;
            }
        }

        private ApiDigitalFormTemplate digitalFormTemplate;
        [CrmEntityMapper("alt_digitalformtemplateid", CrmPropertyType.EntityReference)]
        public ApiDigitalFormTemplate DigitalFormTemplate
        {
            get
            {
                return digitalFormTemplate;
            }
            set
            {
                this.SetProperty(value);
                this.digitalFormTemplate = value;
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
            get
            {
                return externalIdentifier;
            }
            set
            {
                this.SetProperty(value);
                this.externalIdentifier = value;
                this.SetEntityKeys("alt_externalidentifier", value);
            }
        }

        public override string ToString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
