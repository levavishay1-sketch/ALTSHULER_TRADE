using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class SystemUserDAL : CrmExternalBaseDAL<ApiSystemUser>
    {
        public SystemUserDAL(GlobalContext globalContext) : base(globalContext, ApiSystemUser.EntityLogicalName) { }
    }
}
