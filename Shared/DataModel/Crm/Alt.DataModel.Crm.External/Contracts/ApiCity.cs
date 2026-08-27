using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.External.Contracts
{
     public class ApiCity: ApiEntity
    {
        public const string EntityLogicalName = "alt_city";
        public ApiCity() : base(EntityLogicalName)
        {
        }


        private string name;
        /// <summary>
        ///שם עיר 
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
        ///קוד עיר
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

        private ApiCountry country;
        /// <summary>
        ///מדינה 
        /// </summary>
        [CrmEntityMapper("alt_countryid", CrmPropertyType.EntityReference)]
        public ApiCountry Country
        {
            get
            {
                return country;
            }
            set
            {
                this.SetProperty(value);
                this.country = value;
            }
        }

    }
}
