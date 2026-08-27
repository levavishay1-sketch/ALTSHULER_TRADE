using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AuthorizationManagement
{
    public class SystemPostUpdateAuthorizationManagement : PluginBase
    {
        public SystemPostUpdateAuthorizationManagement(string unsecure, string secure)
            : base(typeof(SystemPostUpdateAuthorizationManagement), false) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AuthorizationManagement targetAuthorizationManagement = localContext.TargetEntity?.ToEntity<alt_AuthorizationManagement>();
            alt_AuthorizationManagement preAuthorizationManagement = localContext.PreEntity?.ToEntity<alt_AuthorizationManagement>();
            AuthorizationManagementBL authorizationManagementBl = new AuthorizationManagementBL(localContext.ToGlobal());
            authorizationManagementBl.HandleNextAuthorizationManagement(targetAuthorizationManagement, preAuthorizationManagement);
        }
    }
}