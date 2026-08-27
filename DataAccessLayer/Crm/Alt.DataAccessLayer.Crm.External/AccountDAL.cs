using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;

namespace Alt.DataAccessLayer.Crm.External
{
    public class AccountDAL : CrmExternalBaseDAL<ApiAccount>
    {
        public AccountDAL(GlobalContext globalContext) : base(globalContext, ApiAccount.EntityLogicalName) { }

        public ApiAccount GetByAccountNumber(string accountNumber, string[] select = null)
        {
            this.GlobalContext.LogEntry();
            return base.GetFirstOrDefaultByAttribute("alt_internalaccountnumber", accountNumber.GetPadedLeftZeroString(), select ?? new[] { "accountid" });
        }
    }
}
