using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;


namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiPDFProductionTemplate : ApiEntity
    {
        public const string EntityLogicalName = "alt_pdfproductiontemplate";
        public ApiPDFProductionTemplate() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם תבנית 
        /// </summary>
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

        private string fileName;
        /// <summary>
        ///שם קובץ 
        /// </summary>
        [CrmEntityMapper("alt_filename", CrmPropertyType.String)]
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                this.SetProperty(value);
                this.fileName = value;
            }
        }

        private string externalKeyName;
        /// <summary>
        ///שם מפתח ציצוני 
        /// </summary>
        [CrmEntityMapper("alt_externalkeyname", CrmPropertyType.String)]
        public string ExternalKeyName
        {
            get
            {
                return externalKeyName;
            }
            set
            {
                this.SetProperty(value);
                this.externalKeyName = value;
            }
        }

        /// <summary>
        /// קוד תבנית
        /// </summary>
        private int? codeInt;
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get
            {
                return codeInt;
            }
            set
            {
                this.SetProperty(value);
                this.codeInt = value;
            }
        }

        /// <summary>
        /// תוכן תבנית
        /// </summary>
        private string jsonData;
        [CrmEntityMapper("alt_jsondata", CrmPropertyType.String)]
        public string JsonData
        {
            get
            {
                return jsonData;
            }
            set
            {
                this.SetProperty(value);
                this.jsonData = value;
            }
        }

        /// <summary>
        /// הגדרת API
        /// </summary>
        private ApiConfiguration apiConfigurationId;
        [CrmEntityMapper("alt_apiconfigurationid", CrmPropertyType.EntityReference)]
        public ApiConfiguration ApiConfigurationId
        {
            get
            {
                return apiConfigurationId;
            }
            set
            {
                this.SetProperty(value);
                this.apiConfigurationId = value;
            }
        }

        private bool? useValueParserBit;
        [CrmEntityMapper("alt_usevalueparserbit", CrmPropertyType.Bool)]
        public bool? UseValueParserBit
        {
            get
            {
                return useValueParserBit;
            }
            set
            {
                this.SetProperty(value);
                this.useValueParserBit = value;
            }
        }
    }
}
