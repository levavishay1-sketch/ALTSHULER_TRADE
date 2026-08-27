using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class BlacklistsCheckDAL : CrmExternalBaseDAL<ApiBlacklistsCheck>
    {
        public BlacklistsCheckDAL(GlobalContext globalContext) : base(globalContext, ApiBlacklistsCheck.EntityLogicalName) { }
    }
}
