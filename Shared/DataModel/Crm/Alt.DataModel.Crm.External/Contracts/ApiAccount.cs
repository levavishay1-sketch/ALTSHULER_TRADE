using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiAccount : ApiCustomer
    {
        public const string EntityLogicalName = "account";
        public ApiAccount() : base(EntityLogicalName)
        {
        }

        private string name;
        [CrmEntityMapper("name", CrmPropertyType.String)]
        [StringLength(100)]
        public string Name
        {
            get { return name; }
            set
            {
                this.SetProperty(value);
                name = value;
            }
        }

        private string accountNumber;
        [CrmEntityMapper("accountnumber", CrmPropertyType.String)]
        [StringLength(20)]
        public string AccountNumber
        {
            get { return accountNumber; }
            set
            {
                this.SetProperty(value);
                accountNumber = value;
            }
        }

        private int? businessTypeCode;
        [CrmEntityMapper("businesstypecode", CrmPropertyType.OptionSet)]
        [Range(1, 6)]
        public int? BusinessTypeCode
        {
            get { return businessTypeCode; }
            set
            {
                this.SetProperty(value);
                businessTypeCode = value;
            }
        }

        private string city;
        [CrmEntityMapper("address1_city", CrmPropertyType.String)]
        [StringLength(50)]
        public string City
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

        private string street;
        [CrmEntityMapper("address1_line1", CrmPropertyType.String)]
        [StringLength(50)]
        public string Street
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

        private string houseNumber;
        [CrmEntityMapper("address1_line2", CrmPropertyType.String)]
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

        private string country;
        [CrmEntityMapper("address1_country", CrmPropertyType.String)]
        [StringLength(80)]
        public string Country
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


        private string emailAddress1;
        [CrmEntityMapper("emailaddress1", CrmPropertyType.String)]
        [StringLength(50)]
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


        private string postOfficeBox;
        [CrmEntityMapper("address1_postofficebox", CrmPropertyType.String)]
        [StringLength(20)]
        public string PostOfficeBox
        {
            get
            {
                return postOfficeBox;
            }
            set
            {
                this.SetProperty(value);
                this.postOfficeBox = value;
            }
        }


        private string postalCode;
        [CrmEntityMapper("address1_postalcode", CrmPropertyType.String)]
        [StringLength(20)]
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


        private string fax;
        [CrmEntityMapper("fax", CrmPropertyType.String)]
        [StringLength(100)]
        public string Fax
        {
            get
            {
                return fax;
            }
            set
            {
                this.SetProperty(value);
                this.fax = value;
            }
        }




        private ApiTeam owningTeam;
        [CrmEntityMapper("ownerid", CrmPropertyType.EntityReference)]
        public ApiTeam OwningTeam
        {
            get
            {
                return this.owningTeam;
            }
            set
            {
                this.SetProperty(value);
                this.owningTeam = value;
            }
        }

        private string address1_Country;
        [CrmEntityMapper("address1_country", CrmPropertyType.String)]
        public string Address1_Country
        {
            get
            {
                return this.address1_Country;
            }
            set
            {
                this.SetProperty(value);
                this.address1_Country = value;
            }
        }

        public override ApiCustomer ConvetToCustomer()
        {
            base.CustomerIdentity = this.AccountNumber;
            base.CustomerName = this.Name;
            var customer = new ApiCustomer();
            customer.Id = this.Id;
            customer.LogicalName = this.LogicalName;
            customer.CustomerName = this.CustomerName;
            customer.CustomerIdentity = this.CustomerIdentity;
            var modifiedProperties = new List<PropertyInfo>(this.GetType().GetProperties()).Where(p => this.Contains(p.Name));
            var apiCustomerType = customer.GetType();
            foreach (var prop in modifiedProperties)
            {
                if (apiCustomerType.GetMethod(prop.Name) != null)
                {
                    prop.SetValue(customer, this.ModifiedProperties[prop.Name]);
                }
            }
            return customer;
        }
    }
}
