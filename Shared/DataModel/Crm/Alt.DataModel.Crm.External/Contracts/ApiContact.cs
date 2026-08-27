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
    public class ApiContact: ApiCustomer
    {
        public const string EntityLogicalName = "contact";
        public ApiContact() : base(EntityLogicalName)
        {
        }

        private string governmentId;
        [CrmEntityMapper("governmentid", CrmPropertyType.String)]
        public string GovernmentId
        {
            get
            {
                return governmentId;
            }
            set
            {
                this.SetProperty(value);
                this.governmentId = value;
            }
        }

        private string firstName;
        [StringLength(50)]
        [CrmEntityMapper("firstname", CrmPropertyType.String)]
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
        [StringLength(50)]
        [CrmEntityMapper("lastname", CrmPropertyType.String)]
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

        private string fullName;
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

        private string city;
        [StringLength(80)]
        [CrmEntityMapper("address1_city", CrmPropertyType.String)]
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
        [StringLength(250)]
        [CrmEntityMapper("address1_line1", CrmPropertyType.String)]
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

        private string emailAddress1;
        [StringLength(50)]
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

        private string houseNumber;
        [StringLength(250)]
        [CrmEntityMapper("address1_line2", CrmPropertyType.String)]
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

        private string apartmentNumber;
        [StringLength(250)]
        [CrmEntityMapper("address1_line3", CrmPropertyType.String)]
        public string ApartmentNumber
        {
            get
            {
                return apartmentNumber;
            }
            set
            {
                this.SetProperty(value);
                this.apartmentNumber = value;
            }
        }

        private string mobilePhone;
        [StringLength(50)]
        [CrmEntityMapper("mobilephone", CrmPropertyType.String)]
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

        private string postOfficeBox;
        [StringLength(20)]
        [CrmEntityMapper("address1_postofficebox", CrmPropertyType.String)]
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
        [StringLength(20)]
        [CrmEntityMapper("address1_postalcode", CrmPropertyType.String)]
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

        private int? genderCode;
        [Range(1, 2)]
        [CrmEntityMapper("gendercode", CrmPropertyType.OptionSet)]
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

        private DateTime? birthDay;
        [CrmEntityMapper("birthdate", CrmPropertyType.DateTime)]
        public DateTime? BirthDay
        {
            get
            {
                return birthDay;
            }
            set
            {
                var dateOnly = value;
                if (value != null)
                {
                    dateOnly = TimeZoneInfo.ConvertTime((DateTime)value, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time"));
                }

                this.SetProperty(dateOnly);
                this.birthDay = dateOnly;
            }
        }

        private ApiCustomer parentAccount;
        [CrmEntityMapper("parentcustomerid", CrmPropertyType.EntityReference)]
        public ApiCustomer ParentAccount
        {
            get
            {
                return parentAccount;
            }
            set
            {
                this.SetProperty(value);
                parentAccount = value;
            }
        }
    }
}
