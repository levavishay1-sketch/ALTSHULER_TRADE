using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm
{
    public class AccountBL : CrmBaseBL
    {
        public AccountBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void SetInternalAccountNumberHandler(Account targetAccount)
        {
            this.GlobalContext.LogEntry();
            if (targetAccount.Contains(Account.Fields.AccountNumber))
            {
                targetAccount.AccountNumber = !string.IsNullOrWhiteSpace(targetAccount.AccountNumber) ? targetAccount.AccountNumber : null;
                targetAccount.alt_InternalAccountNumber = targetAccount.AccountNumber?.GetPadedLeftZeroString();
            }
        }
    }
}
