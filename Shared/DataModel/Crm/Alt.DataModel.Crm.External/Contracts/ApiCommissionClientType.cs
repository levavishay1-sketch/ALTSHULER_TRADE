using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiCommissionClientType : ApiEntity
    {
        public const string EntityLogicalName = "alt_commissionclienttype";

        public ApiCommissionClientType() : base(EntityLogicalName)
        {
        }
        private string name;
        /// <summary>
        ///שם עמלה 
        /// </summary>
        [StringLength(250)]
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

        private string tradeOneGroup;
        /// <summary>
        ///קבוצה בTrade1
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_tradeonegroup", CrmPropertyType.String)]
        public string TradeOneGroup
        {
            get
            {
                return tradeOneGroup;
            }
            set
            {
                this.SetProperty(value);
                this.tradeOneGroup = value;
            }
        }
    }
}
