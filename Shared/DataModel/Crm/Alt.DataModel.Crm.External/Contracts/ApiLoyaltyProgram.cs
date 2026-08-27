using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;


namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiLoyaltyProgram: ApiEntity
    {
        public const string EntityLogicalName = "alt_loyaltyprogram";
        public ApiLoyaltyProgram() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם מדינה 
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

        private int? code;
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.SetEntityKeys("alt_codeint", value);
                this.SetProperty(value);
                this.code = value;
            }
        }
    }
}
