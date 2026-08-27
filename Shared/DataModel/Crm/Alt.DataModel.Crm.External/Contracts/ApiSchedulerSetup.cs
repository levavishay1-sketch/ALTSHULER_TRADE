using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Extensions;
using Alt.Framework.Mapper;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiSchedulerSetup : ApiEntity
    {
        public const string EntityLogicalName = "alt_schedulersetup";
        public ApiSchedulerSetup() : base(EntityLogicalName)
        {
        }

        private string developmentSettings;
        /// <summary>
        /// הגדרות
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

        private bool? sendEmailWithExecutionResultBit;
        /// <summary>
        /// האם לשלוח תוצאת ריצה במייל
        /// </summary>
        [CrmEntityMapper("alt_sendemailwithexecutionresultbit", CrmPropertyType.Bool)]
        public bool? SendEmailWithExecutionResultBit
        {
            get
            {
                return this.sendEmailWithExecutionResultBit;
            }
            set
            {
                this.SetProperty(value);
                this.sendEmailWithExecutionResultBit = value;
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
