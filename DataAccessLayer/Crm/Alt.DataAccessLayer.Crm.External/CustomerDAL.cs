using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class CustomerDAL : CrmExternalBaseDAL<ApiEntityBase>
    {
        public CustomerDAL(GlobalContext globalContext, string entityLogicalName) : base(globalContext, entityLogicalName) { }


        public string GetCustomerArchiveIdentifier(ApiCustomer apiCustomer)
        {
            this.GlobalContext.LogEntry();

            string customerIdentity = null;
            if ((!string.IsNullOrWhiteSpace(apiCustomer.LogicalName) && apiCustomer.LogicalName == ApiContact.EntityLogicalName)
                || (apiCustomer.CustomerTypeCode == (int)CustomerTypeCode.Contact))
            {
                ContactDAL contacDAL = new ContactDAL(this.GlobalContext);
                ApiContact retrievedContact = contacDAL.Get(apiCustomer.Id.Value, new[] { "governmentid" });
                retrievedContact.CustomerIdentity = retrievedContact.GovernmentId.GetPadedLeftZeroString(9);
                customerIdentity = retrievedContact.CustomerIdentity;
            }
            else
            {
                AccountDAL accountDAL = new AccountDAL(this.GlobalContext);
                ApiAccount retrievedAccount = accountDAL.Get(apiCustomer.Id.Value, new[] { "accountnumber" });
                retrievedAccount.CustomerIdentity = retrievedAccount.AccountNumber.GetPadedLeftZeroString();
                customerIdentity = retrievedAccount.CustomerIdentity;
            }

            return customerIdentity;
        }
    }
}
