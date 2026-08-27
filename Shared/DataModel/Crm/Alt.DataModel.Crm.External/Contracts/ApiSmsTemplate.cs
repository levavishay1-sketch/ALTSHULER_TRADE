using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiSmsTemplate : ApiEntity
    {
        public const string EntityLogicalName = "alt_smstemplate";

        public ApiSmsTemplate() : base(EntityLogicalName)
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

        private string sendBy;
        [StringLength(11)]
        [CrmEntityMapper("alt_sendby", CrmPropertyType.String)]
        public string SendBy
        {
            get { return sendBy; }
            set
            {
                this.SetProperty(value);
                sendBy = value;
            }
        }

        private string schemaName;
        [StringLength(11)]
        [CrmEntityMapper("alt_schemaname", CrmPropertyType.String)]
        public string SchemaName
        {
            get
            {
                return schemaName;
            }
            set
            {
                this.SetProperty(value);
                this.schemaName = value;
            }
        }
    }
}
