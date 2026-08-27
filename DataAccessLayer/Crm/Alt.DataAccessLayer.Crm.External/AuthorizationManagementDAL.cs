using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;


namespace Alt.DataAccessLayer.Crm.External
{
    public class AuthorizationManagementDAL : CrmExternalBaseDAL<ApiAuthorizationManagement>
    {
        public AuthorizationManagementDAL(GlobalContext globalContext) : base(globalContext, ApiAuthorizationManagement.EntityLogicalName) { }
    }
}
