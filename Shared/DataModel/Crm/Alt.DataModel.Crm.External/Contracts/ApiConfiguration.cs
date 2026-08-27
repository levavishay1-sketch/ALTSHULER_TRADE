using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Extensions;
using Alt.Framework.Mapper;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiConfiguration : ApiEntity
    {

        public const string EntityLogicalName = "alt_apiconfiguration";
        public ApiConfiguration() : base(EntityLogicalName) { }

        private int? apiTypeCode;
        /// <summary>
        /// סוג ממשק
        /// </summary>
        [CrmEntityMapper("alt_apitypecode", CrmPropertyType.OptionSet)]
        public int? ApiTypeCode
        {
            get { return apiTypeCode; }
            set
            {
                this.SetProperty(value);
                apiTypeCode = value;
            }
        }

        private int? code;
        /// <summary>
        /// קוד
        /// </summary>
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get { return code; }
            set
            {
                this.SetProperty(value);
                code = value;
            }
        }

        private string developmentSettings;
        /// <summary>
        /// הגדרות פיתוח
        /// </summary>
        [CrmEntityMapper("alt_developmentsettings", CrmPropertyType.String)]
        public string DevelopmentSettings
        {
            get { return developmentSettings; }
            set
            {
                this.SetProperty(value);
                developmentSettings = value;
            }
        }

        private string xmlValidationModel;
        /// <summary>
        /// מודל אימות
        /// </summary>
        [CrmEntityMapper("alt_xmlvalidationmodel", CrmPropertyType.String)]
        public string XmlValidationModel
        {
            get { return xmlValidationModel; }
            set
            {
                this.SetProperty(value);
                xmlValidationModel = value;
            }
        }

        private string httpHeaders;
        /// <summary>
        /// כותרות HTTP
        /// </summary>
        [CrmEntityMapper("alt_httpheaders", CrmPropertyType.String)]
        public string HttpHeaders
        {
            get { return httpHeaders; }
            set
            {
                this.SetProperty(value);
                httpHeaders = value;
            }
        }

        private string url;
        /// <summary>
        /// כתובת אתר
        /// </summary>
        [CrmEntityMapper("alt_url", CrmPropertyType.String)]
        public string Url
        {
            get { return url; }
            set
            {
                this.SetProperty(value);
                url = value;
            }
        }

        private string description;
        [CrmEntityMapper("alt_description", CrmPropertyType.String)]
        public string Description
        {
            get { return description; }
            set
            {
                this.SetProperty(value);
                description = value;
            }
        }

        private int? destinationSystemCode;
        /// <summary>
        /// מערכת יעד
        /// </summary>
        [CrmEntityMapper("alt_destinationsystemcode", CrmPropertyType.OptionSet)]
        public int? DestinationSystemCode
        {
            get { return destinationSystemCode; }
            set
            {
                this.SetProperty(value);
                destinationSystemCode = value;
            }
        }

        private int? sourceSystemCode;
        /// <summary>
        /// מערכת מקור
        /// </summary>
        [CrmEntityMapper("alt_sourcesystemcode", CrmPropertyType.OptionSet)]
        public int? SourceSystemCode
        {
            get { return sourceSystemCode; }
            set
            {
                this.SetProperty(value);
                sourceSystemCode = value;
            }
        }

        private int? methodCode;
        /// <summary>
        /// שיטת בקשה
        /// </summary>
        [CrmEntityMapper("alt_requestmethodcode", CrmPropertyType.OptionSet)]
        public int? MethodCode
        {
            get { return methodCode; }
            set
            {
                this.SetProperty(value);
                methodCode = value;
            }
        }

        private int? requestProcessingTypeCode;
        /// <summary>
        /// תצורת עיבוד בקשה
        /// </summary>
        [CrmEntityMapper("alt_requestprocessingtypecode", CrmPropertyType.OptionSet)]
        public int? RequestProcessingTypeCode
        {
            get { return requestProcessingTypeCode; }
            set
            {
                this.SetProperty(value);
                requestProcessingTypeCode = value;
            }
        }

        private bool? debugMode;
        /// <summary>
        /// האם מצב Debug
        /// </summary>
        [CrmEntityMapper("alt_debugmodebit", CrmPropertyType.Bool)]
        public bool? DebugMode
        {
            get { return debugMode; }
            set
            {
                this.SetProperty(value);
                debugMode = value;
            }
        }

        private bool? useSertificates;
        /// <summary>
        /// האם להשתמש בתעודות
        /// </summary>
        [CrmEntityMapper("alt_usesertificatesbit", CrmPropertyType.Bool)]
        public bool? UseSertificates
        {
            get { return useSertificates; }
            set
            {
                this.SetProperty(value);
                useSertificates = value;
            }
        }

        private bool? redirectBit;
        /// <summary>
        /// האם לנתב את הבקשה - לא בשימוש
        /// </summary>
        [CrmEntityMapper("alt_redirectbit", CrmPropertyType.Bool)]
        public bool? RedirectBit
        {
            get { return redirectBit; }
            set
            {
                this.SetProperty(value);
                redirectBit = value;
            }
        }

        private bool? useOutgoingObjectValidationBit;
        /// <summary>
        /// האם לאמת אובייקט יוצא
        /// </summary>
        [CrmEntityMapper("alt_useoutgoingobjectvalidationbit", CrmPropertyType.Bool)]
        public bool? UseOutgoingObjectValidationBit
        {
            get { return useOutgoingObjectValidationBit; }
            set
            {
                this.SetProperty(value);
                useOutgoingObjectValidationBit = value;
            }
        }

        public Dictionary<string, object> DevelopmentSettingsDictionary { get; set; }

        public bool TryGetSettingsItemValue<T>(string key, out T value)
        {
            bool isSucces = false;
            value = (T)(null as object);

            if (!string.IsNullOrWhiteSpace(this.DevelopmentSettings))
            {
                if (this.DevelopmentSettingsDictionary == null)
                {
                    this.DevelopmentSettingsDictionary = this.DevelopmentSettings.ToDictionary<string, object>();
                }
                if (this.DevelopmentSettingsDictionary.ContainsKey(key))
                {
                    var settingsValue = this.DevelopmentSettingsDictionary[key];
                    if (settingsValue != null)
                    {
                        string strValue = settingsValue.ToString();
                        value = strValue.TryParseValue<T>();
                    }
                    isSucces = true;
                }
            }     
            return isSucces;
        }
    }
}
