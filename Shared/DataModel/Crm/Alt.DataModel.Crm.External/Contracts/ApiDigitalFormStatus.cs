using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiDigitalFormStatus : ApiEntity
    {
        public const string EntityLogicalName = "alt_digitalformstatus";
        public ApiDigitalFormStatus() : base(EntityLogicalName)
        {
        }

        private string code;
        /// <summary>
        /// קוד
        /// </summary>
        [StringLength(40)]
        [CrmEntityMapper("alt_code", CrmPropertyType.String)]
        public string Code
        {
            get { return code; }
            set
            {
                this.SetProperty(value);
                code = value;
                this.SetEntityKeys("alt_code", value);
            }
        }
    }
}
