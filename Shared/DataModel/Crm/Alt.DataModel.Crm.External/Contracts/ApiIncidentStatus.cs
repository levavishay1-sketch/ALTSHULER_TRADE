using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiIncidentStatus: ApiEntity
    {
        public const string EntityLogicalName = "alt_incidentstatus";

        public ApiIncidentStatus() : base(EntityLogicalName)
        {
        }

        private int? code;
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.SetProperty(value);
                this.SetEntityKeys("alt_codeint", value);
                this.code = value;
            }
        }
    }
}
