using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiBlacklistsCheck : ApiEntity
    {
        public const string EntityLogicalName = "alt_blacklistscheck";
        public ApiBlacklistsCheck() : base(EntityLogicalName)
        {
        }

        private string identityNumber;
        /// <summary>
        /// מספר זיהוי
        /// </summary>
        [CrmEntityMapper("alt_identitynumber", CrmPropertyType.String)]
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

        private string firstName;
        /// <summary>
        /// שם פרטי
        /// </summary>
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

        private string externalIdentifier;
        /// <summary>
        /// מזיי חיצוני
        /// </summary>
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

        private ApiCountry countryId;
        /// <summary>
        /// מדינה מנפיקה ת.ז
        /// </summary>
        [CrmEntityMapper("alt_countryid", CrmPropertyType.EntityReference)]
        public ApiCountry CountryId
        {
            get
            {
                return countryId;
            }
            set
            {
                this.SetProperty(value);
                this.countryId = value;
            }
        }

        private string failureDetails;
        /// <summary>
        /// פירוט כישלון
        /// </summary>
        [CrmEntityMapper("alt_failuredetails", CrmPropertyType.String)]
        public string FailureDetails
        {
            get
            {
                return failureDetails;
            }
            set
            {
                this.SetProperty(value);
                this.failureDetails = value;
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


        private int? appearsInBlacklistsCode;
        /// <summary>
        /// מופיע ברשימות שחורות
        /// </summary>
        [CrmEntityMapper("alt_appearsinblacklistscode", CrmPropertyType.OptionSet)]
        public int? AppearsInBlacklistsCode
        {
            get
            {
                return appearsInBlacklistsCode;
            }
            set
            {
                this.SetProperty(value);
                this.appearsInBlacklistsCode = value;
            }
        }

    }
}
