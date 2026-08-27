using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiTeam : ApiEntity
    {
        public const string EntityLogicalName = "team";

        public ApiTeam() : base(EntityLogicalName)
        {
        }

        private int? code;
        [CrmEntityMapper("alt_teamcodeint", CrmPropertyType.Int)]
        public int? Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.SetEntityKeys("alt_teamcodeint", value);
                this.SetProperty(value);
                this.code = value;
            }
        }
    }
}
