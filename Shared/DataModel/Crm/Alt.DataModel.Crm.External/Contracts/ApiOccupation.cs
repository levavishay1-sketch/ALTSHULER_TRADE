using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiOccupation : ApiEntity
    {
        public const string EntityLogicalName = "alt_occupation";
        public ApiOccupation() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם העיסוק
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


        private int? code;
        /// <summary>
        /// קוד עיסוק
        /// </summary>
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get { return code; }
            set
            {
                this.SetProperty(value);
                code = value;
                this.SetEntityKeys("alt_codeint", value);
            }
        }
    }
}
