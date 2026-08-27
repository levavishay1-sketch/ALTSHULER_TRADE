using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AuthorizationManagement
{
    public class AsyncCreateAuthorizationManagement : PluginBase
    {
        public AsyncCreateAuthorizationManagement(string unsecure, string secure) : base(typeof(AsyncCreateAuthorizationManagement)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AuthorizationManagement targetAuthorizationManagement = localContext.TargetEntity?.ToEntity<alt_AuthorizationManagement>();
            AuthorizationManagementBL authorizationManagementBL = new AuthorizationManagementBL(localContext.ToGlobal());
            authorizationManagementBL.UpdateDigitalFormVerificationRequirementsByRiskLevel(targetAuthorizationManagement);
            authorizationManagementBL.UpdateAccountHolder(targetAuthorizationManagement);
        }
    }
}