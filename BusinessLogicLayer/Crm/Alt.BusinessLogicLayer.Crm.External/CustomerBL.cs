using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class CustomerBL : ExternalBLBase
    {
        public CustomerBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ApiCustomer GetCustomerByCustomerIdentifier(ApiCustomer customer)
        {
            string internalCustomerIdentity = customer.CustomerIdentity.GetPadedLeftZeroString();
            ApiCustomer retrievedCustomer;
            if (customer.CustomerTypeCode == (int)CustomerTypeCode.Account)
            {
                AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                retrievedCustomer = accountDal.GetFirstOrDefaultByAttribute("alt_internalaccountnumber",
                    internalCustomerIdentity, new[] { "accountid" });
            }
            else
            {
                ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                retrievedCustomer = contactDal.GetFirstOrDefaultByAttribute("alt_internalgovernmentid",
                    internalCustomerIdentity, new[] { "contactid" });
            }

            return retrievedCustomer;
        }
    }
}
