using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiCountry: ApiEntity
    {
        public const string EntityLogicalName = "alt_country";
        public ApiCountry() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם מדינה 
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

        private string code;
        /// <summary>
        ///קוד מדינה
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_code", CrmPropertyType.String)]
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

        private string countryAlpha3CodeISO;
        /// <summary>
        ///תקן מדינה (ISO)
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_countryalpha3codeiso", CrmPropertyType.String)]
        public string CountryAlpha3CodeISO
        {
            get
            {
                return countryAlpha3CodeISO;
            }
            set
            {
                this.SetProperty(value);
                this.countryAlpha3CodeISO = value;
            }
        }

        private bool? moneyLaunderingRiskBit;
        /// <summary>
        ///שם מדינה 
        /// </summary>
        [CrmEntityMapper("alt_moneylaunderingriskbit", CrmPropertyType.Bool)]
        public bool? MoneyLaunderingRiskBit
        {
            get
            {
                return moneyLaunderingRiskBit;
            }
            set
            {
                this.SetProperty(value);
                this.moneyLaunderingRiskBit = value;
            }
        }

        private string countryEnglishName;
        /// <summary>
        ///שם מדינה באנגלית 
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_countryenglishname", CrmPropertyType.String)]
        public string CountryEnglishName
        {
            get
            {
                return countryEnglishName;
            }
            set
            {
                this.SetProperty(value);
                this.countryEnglishName = value;
            }
        }

    }
}
