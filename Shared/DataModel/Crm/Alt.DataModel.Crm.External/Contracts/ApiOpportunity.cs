using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiOpportunity : ApiEntity
    {
        public const string EntityLogicalName = "opportunity";

        public ApiOpportunity() : base(EntityLogicalName) { }

        private string opportunityIdentityNumber;
        /// <summary>
        /// מזהה הזדמנות
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_opportunityidentitynumber", CrmPropertyType.String, mappToCrm: false, mappFromCrm: true)]
        public string OpportunityIdentityNumber
        {
            get { return opportunityIdentityNumber; }
            set
            {
                this.SetProperty(value);
                opportunityIdentityNumber = value;
                this.SetEntityKeys("alt_opportunityidentitynumber", value);
            }
        }

        private ApiCustomer customerId;
        /// <summary>
        /// לקוח
        /// </summary>
        [CrmEntityMapper("customerid", CrmPropertyType.EntityReference)]
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

        private string emailAddress;
        /// <summary>
        /// דואר אלקטרוני
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("emailaddress", CrmPropertyType.String)]
        public string EmailAddress
        {
            get
            {
                return emailAddress;
            }
            set
            {
                this.SetProperty(value);
                this.emailAddress = value;
            }
        }

        private string mobilePhone;
        /// <summary>
        /// טלפון נייד
        /// </summary>
        [CrmEntityMapper("alt_mobilephone", CrmPropertyType.String)]
        public string MobilePhone
        {
            get { return mobilePhone; }
            set
            {
                this.SetProperty(value);
                mobilePhone = value;
            }
        }
    }
}
