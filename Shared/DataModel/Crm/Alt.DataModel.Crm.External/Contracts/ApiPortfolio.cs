using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiPortfolio : ApiEntity
    {
        public const string EntityLogicalName = "alt_portfolio";

        public ApiPortfolio() : base(EntityLogicalName)
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

        private string shenhavAccountNumber;
        /// <summary>
        ///מספר חשבון שנהב 
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_shenhavaccountnumber", CrmPropertyType.String)]
        public string ShenhavAccountNumber
        {
            get
            {
                return shenhavAccountNumber;
            }
            set
            {
                this.SetProperty(value);
                this.shenhavAccountNumber = value;
                this.SetEntityKeys("alt_shenhavaccountnumber", value);
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
    }
}
