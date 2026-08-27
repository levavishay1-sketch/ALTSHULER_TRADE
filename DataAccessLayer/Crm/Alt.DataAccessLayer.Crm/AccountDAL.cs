using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
   public class AccountDAL : CrmBaseDAL<Account>
    {
        public AccountDAL(GlobalContext globalContext) : base(globalContext, Account.EntityLogicalName)
        {
        }

        public Account GetByAccountNumber(string accountNumber, string[] columns = null)
        {
            this.GlobalContext.LogEntry();
            return this.GetFirstOrDefaultByAttribute(Account.Fields.alt_InternalAccountNumber, accountNumber.GetPadedLeftZeroString(),
                columns ?? new[] { Account.Fields.AccountId, Account.Fields.Name });
        }
    }
}
