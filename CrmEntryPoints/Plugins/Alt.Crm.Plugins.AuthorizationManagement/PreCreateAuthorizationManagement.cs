using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AuthorizationManagement
{
    public class PreCreateAuthorizationManagement : PluginBase
    {
        public PreCreateAuthorizationManagement(string unsecure, string secure) : base(typeof(PreCreateAuthorizationManagement)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AuthorizationManagement targetAuthorizationManagement = localContext.TargetEntity?.ToEntity<alt_AuthorizationManagement>();
            AuthorizationManagementBL authorizationManagementBl = new AuthorizationManagementBL(localContext.ToGlobal());
            authorizationManagementBl.SetAuthorizationManagementName(targetAuthorizationManagement);
        }
    }
}