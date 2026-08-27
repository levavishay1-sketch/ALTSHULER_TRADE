using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiEmailTemplate: ApiEntity
    {
        public const string EntityLogicalName = "alt_emailtemplate";

        public ApiEmailTemplate() : base(EntityLogicalName)
        {
        }

        private int? templateCode;
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? TemplateCode
        {
            get
            {
                return templateCode;
            }
            set
            {
                this.SetProperty(value);
                this.templateCode = value;
            }
        }
    }
}
