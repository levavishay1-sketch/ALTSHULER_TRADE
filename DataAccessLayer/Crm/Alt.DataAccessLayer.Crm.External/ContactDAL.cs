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
    public class ContactDAL: CrmExternalBaseDAL<ApiContact>
    {
        public ContactDAL(GlobalContext globalContext) : base(globalContext, ApiContact.EntityLogicalName) { }

        public ApiContact GetByGovernmentId(string governmentId, string[] select = null)
        {
            this.GlobalContext.LogEntry();
            return base.GetFirstOrDefaultByAttribute("alt_internalgovernmentid", governmentId.GetPadedLeftZeroString(), select ?? new[] { "contactid" });
        }
    }
}
