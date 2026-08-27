using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiBank: ApiEntity
    {
        public const string EntityLogicalName = "alt_bank";
        public ApiBank() : base(EntityLogicalName)
        {
        }
        private string code;
        [StringLength(100)]
        [CrmEntityMapper("alt_code", CrmPropertyType.String)]
        public string Code
        {
            get { return code; }
            set
            {
                this.SetProperty(value);
                this.code = value;
                this.SetEntityKeys("alt_code", value);
            }
        }

        private string name;
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
    }
}
