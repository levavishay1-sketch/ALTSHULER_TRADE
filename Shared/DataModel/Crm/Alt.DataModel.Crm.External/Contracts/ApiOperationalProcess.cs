using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiOperationalProcess : ApiEntity
    {
        public ApiOperationalProcess(string entityLogicalName)
            : base(entityLogicalName) { }

        private string name;
        /// <summary>
        ///שם 
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

        private ApiCustomer customer;
        [CrmEntityMapper("alt_customerid", CrmPropertyType.EntityReference)]
        public ApiCustomer Customer
        {
            get
            {
                return this.customer;
            }
            set
            {
                this.SetProperty(value);
                this.customer = value;
            }
        }

        private ApiPortfolio portfolioId;
        /// <summary>
        /// חשבון שנהב
        /// </summary>
        [CrmEntityMapper("alt_portfolioid", CrmPropertyType.EntityReference)]
        public ApiPortfolio Portfolio
        {
            get
            {
                return portfolioId;
            }
            set
            {
                this.SetProperty(value);
                this.portfolioId = value;
            }
        }

    }
}
