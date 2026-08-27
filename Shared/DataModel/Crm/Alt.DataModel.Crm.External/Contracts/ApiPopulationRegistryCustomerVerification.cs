using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiPopulationRegistryCustomerVerification: ApiEntity
    {
        public const string EntityLogicalName = "alt_populationregistrycustomerverification";
        public ApiPopulationRegistryCustomerVerification() : base(EntityLogicalName)
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

        private string identityNumber;
        /// <summary>
        /// מספר זהות
        /// </summary>
        [CrmEntityMapper("alt_identitynumber", CrmPropertyType.String)]
        [StringLength(9)]
        public string IdentityNumber
        {
            get
            {
                return identityNumber;
            }
            set
            {
                this.SetProperty(value);
                this.identityNumber = value;
            }
        }

        private string errorMessageDetails;
        /// <summary>
        /// פירוט הודעות שגיאה
        /// </summary>
        [CrmEntityMapper("alt_errormessagedetails", CrmPropertyType.String)]
        public string ErrorMessageDetails
        {
            get
            {
                return errorMessageDetails;
            }
            set
            {
                this.SetProperty(value);
                this.errorMessageDetails = value;
            }
        }

        private string discrepancyDetails;
        /// <summary>
        /// פרטי אי התאמה
        /// </summary>
        [CrmEntityMapper("alt_discrepancydetails", CrmPropertyType.String)]
        public string DiscrepancyDetails
        {
            get
            {
                return discrepancyDetails;
            }
            set
            {
                this.SetProperty(value);
                this.discrepancyDetails = value;
            }
        }

        private string responseDitailsToDisplay;
        /// <summary>
        /// פרטי התגובה לתצוגה
        /// </summary>
        [CrmEntityMapper("alt_responseditailstodisplay", CrmPropertyType.String)]
        public string ResponseDitailsToDisplay
        {
            get
            {
                return responseDitailsToDisplay;
            }
            set
            {
                this.SetProperty(value);
                this.responseDitailsToDisplay = value;
            }
        }

        private string responseDetails;
        /// <summary>
        ///פרטי תשובה (פנימי)
        /// </summary>
        [CrmEntityMapper("alt_responsedetails", CrmPropertyType.String)]
        public string ResponseDetails
        {
            get
            {
                return responseDetails;
            }
            set
            {
                this.SetProperty(value);
                this.responseDetails = value;
            }
        }

        private string joiningProcessNumber;
        /// <summary>
        /// מספר תהליך הצטרפות
        /// </summary>
        [CrmEntityMapper("alt_joiningprocessnumber", CrmPropertyType.String)]
        [StringLength(100)]
        public string JoiningProcessNumber
        {
            get
            {
                return joiningProcessNumber;
            }
            set
            {
                this.SetProperty(value);
                this.joiningProcessNumber = value;
            }
        }

        private int? companyCodeInt;
        /// <summary>
        ///קוד חברה 
        /// </summary>
        [CrmEntityMapper("alt_companycodeint", CrmPropertyType.Int)]
        public int? CompanyCodeInt
        {
            get
            {
                return companyCodeInt;
            }
            set
            {
                this.SetProperty(value);
                this.companyCodeInt = value;
            }
        }

        private bool? compareDataBit;
        /// <summary>
        /// האם להשוואת נתונים
        /// </summary>
        [CrmEntityMapper("alt_comparedatabit", CrmPropertyType.Bool)]
        public bool? CompareDataBit
        {
            get
            {
                return compareDataBit;
            }
            set
            {
                this.SetProperty(value);
                this.compareDataBit = value;
            }
        }

        private int? transferStatusCode;
        /// <summary>
        ///סטטוס שידור
        /// </summary>
        [CrmEntityMapper("alt_transferstatuscode", CrmPropertyType.OptionSet)]
        public int? TransferStatusCode
        {
            get
            {
                return transferStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.transferStatusCode = value;
            }
        }

        private int? populationTypeCode;
        /// <summary>
        /// סוג אוכלוסייה
        /// </summary>
        [CrmEntityMapper("alt_populationtypecode", CrmPropertyType.OptionSet)]
        public int? PopulationTypeCode
        {
            get
            {
                return populationTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.populationTypeCode = value;
            }
        }

        private int? verificationResultCode;
        /// <summary>
        ///תוצאת אימות
        /// </summary>
        [CrmEntityMapper("alt_verificationresultcode", CrmPropertyType.OptionSet)]
        public int? VerificationResultCode
        {
            get
            {
                return verificationResultCode;
            }
            set
            {
                this.SetProperty(value);
                this.verificationResultCode = value;
            }
        }

        private int? iDIssuanceDateVerificationResultCode;
        /// <summary>
        ///תוצאת אימות תאריך הנפקת תעודת זהות
        /// </summary>
        [CrmEntityMapper("alt_idissuancedateverificationresultcode", CrmPropertyType.OptionSet)]
        public int? IDIssuanceDateVerificationResultCode
        {
            get
            {
                return iDIssuanceDateVerificationResultCode;
            }
            set
            {
                this.SetProperty(value);
                this.iDIssuanceDateVerificationResultCode = value;
            }
        }

        private int? dataComparisonStatusCode;
        /// <summary>
        ///סטטוס השוואת נתונים
        /// </summary>
        [CrmEntityMapper("alt_datacomparisonstatuscode", CrmPropertyType.OptionSet)]
        public int? DataComparisonStatusCode
        {
            get
            {
                return dataComparisonStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.dataComparisonStatusCode = value;
            }
        }

        private DateTime? iDIssuanceDate;
        /// <summary>
        ///תאריך הנפקת תעודת זהות 
        /// </summary>
        [CrmEntityMapper("alt_idissuancedate", CrmPropertyType.DateTime)]
        public DateTime? IDIssuanceDate
        {
            get
            {
                return iDIssuanceDate;
            }
            set
            {
                this.SetProperty(value);
                this.iDIssuanceDate = value;
            }
        }

        private DateTime? birthDate;
        /// <summary>
        ///תאריך לידה 
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

        private ApiContact contactId;
        /// <summary>
        /// לקוח
        /// </summary>
        [CrmEntityMapper("alt_contactid", CrmPropertyType.EntityReference)]
        public ApiContact ContactId
        {
            get
            {
                return contactId;
            }
            set
            {
                this.SetProperty(value);
                this.contactId = value;
            }
        }

        private ApiEntity relatedRecordId;
        /// <summary>
        /// RelatedRecord
        /// </summary>
        [CrmEntityMapper("alt_relatedrecordid", CrmPropertyType.EntityReference)]
        public ApiEntity RelatedRecordId
        {
            get
            {
                return relatedRecordId;
            }
            set
            {
                this.SetProperty(value);
                this.relatedRecordId = value;
            }
        }
    }
}
