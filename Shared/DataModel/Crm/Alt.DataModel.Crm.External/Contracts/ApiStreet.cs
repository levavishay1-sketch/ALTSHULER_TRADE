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
    public class ApiStreet: ApiEntity
    {
        public const string EntityLogicalName = "alt_street";
        public ApiStreet() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם רחוב
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
        ///קוד
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

        private string streetCode;
        /// <summary>
        ///סמל רחוב
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_streetcode", CrmPropertyType.String)]
        public string StreetCode
        {
            get
            {
                return streetCode;
            }
            set
            {
                this.SetProperty(value);
                this.streetCode = value;
            }
        }

        private ApiCity city;
        /// <summary>
        /// שם עיר
        /// </summary>
        [CrmEntityMapper("alt_cityid", CrmPropertyType.EntityReference)]
        public ApiCity City
        {
            get { return city; }
            set
            {
                this.SetProperty(value);
                city = value;
            }
        }
    }
}
