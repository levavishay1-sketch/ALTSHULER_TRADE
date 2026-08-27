using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiBranch: ApiEntity
    {
        public const string EntityLogicalName = "alt_branch";
        public ApiBranch() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        /// מספר סניף - שם סניף
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

        private string code;
        /// <summary>
        /// קוד
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_code", CrmPropertyType.String)]
        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                this.SetProperty(value);
                this.code = value;
                this.SetEntityKeys("alt_code", value);
            }
        }

        private string branchNumber;
        /// <summary>
        /// מספר סניף
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_branchnumber", CrmPropertyType.String)]
        public string BranchNumber
        {
            get
            {
                return branchNumber;
            }
            set
            {
                this.SetProperty(value);
                this.branchNumber = value;

            }
        }

        private ApiBank bank;
        /// <summary>
        /// שם הבנק
        /// </summary>
        [CrmEntityMapper("alt_bankid", CrmPropertyType.EntityReference)]
        public ApiBank Bank
        {
            get
            {
                return this.bank;
            }
            set
            {
                this.SetProperty(value);
                this.bank = value;
            }
        }
    

        private string branchName;
        /// <summary>
        /// שם סניף
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_branchname", CrmPropertyType.String)]
        public string BranchName
        {
            get
            {
                return branchName;
            }
            set
            {
                this.SetProperty(value);
                this.branchName = value;
            }
        }

        private string branchAddress;
        /// <summary>
        /// כתובת
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_branchaddress", CrmPropertyType.String)]
        public string BranchAddress
        {
            get
            {
                return branchAddress;
            }
            set
            {
                this.SetProperty(value);
                this.branchAddress = value;
            }
        }

        private ApiCity city;
        /// <summary>
        /// עיר 
        /// </summary>
        [CrmEntityMapper("alt_cityid", CrmPropertyType.EntityReference)]
        public ApiCity City
        {
            get { return city; }
            set
            {
                this.SetProperty(value);
                city = value;
            }
        }

        private string fax;
        /// <summary>
        /// פקס
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_fax", CrmPropertyType.String)]
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

        private string phoneNumber;
        /// <summary>
        /// טלפון
        /// </summary>
        [CrmEntityMapper("alt_phonenumber", CrmPropertyType.String)]
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set
            {
                this.SetProperty(value);
                phoneNumber = value;
            }
        }

        private string zipCode;
        /// <summary>
        /// מיקוד
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_zipcode", CrmPropertyType.String)]
        public string ZipCode
        {
            get { return zipCode; }
            set
            {
                this.SetProperty(value);
                zipCode = value;
            }
        }
    }
}
