using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiDigitalFormTemplate : ApiEntity
    {
        public const string EntityLogicalName = "alt_digitalformtemplate";
        public ApiDigitalFormTemplate() : base(EntityLogicalName)
        {
        }

        private string code;
        [CrmEntityMapper("alt_code", CrmPropertyType.Int)]
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

        private string mappedEntityLogicalName;
        [CrmEntityMapper("alt_mappedentitylogicalname", CrmPropertyType.String)]
        public string MappedEntityLogicalName
        {
            get
            {
                return mappedEntityLogicalName;
            }
            set
            {
                this.SetProperty(value);
                this.mappedEntityLogicalName = value;
            }
        }

        private string configurations;
        /// <summary>
        /// הגדרות
        /// </summary>
        [CrmEntityMapper("alt_configurations", CrmPropertyType.String)]
        public string Configurations
        {
            get
            {
                return configurations;
            }
            set
            {
                this.SetProperty(value);
                this.configurations = value;
            }
        }

        private string mappingConfigurations;
        /// <summary>
        /// הגדרות מיפוי בין ישויות
        /// </summary>
        [CrmEntityMapper("alt_mappingconfigurations", CrmPropertyType.String)]
        public string MappingConfigurations
        {
            get
            {
                return mappingConfigurations;
            }
            set
            {
                this.SetProperty(value);
                this.mappingConfigurations = value;
            }
        }
    }
}