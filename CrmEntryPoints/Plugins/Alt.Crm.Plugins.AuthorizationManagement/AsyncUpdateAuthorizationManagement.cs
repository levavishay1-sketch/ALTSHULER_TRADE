using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AuthorizationManagement
{
    public class AsyncUpdateAuthorizationManagement : PluginBase
    {
        public AsyncUpdateAuthorizationManagement(string unsecure, string secure) : base(typeof(AsyncUpdateAuthorizationManagement)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AuthorizationManagement targetAuthorizationManagement = localContext.TargetEntity?.ToEntity<alt_AuthorizationManagement>();
            alt_AuthorizationManagement preAuthorizationManagement = localContext.PreEntity?.ToEntity<alt_AuthorizationManagement>();
            AuthorizationManagementBL authorizationManagementBl = new AuthorizationManagementBL(localContext.ToGlobal());
            authorizationManagementBl.UpdateDigitalFormVerificationRequirementsByRiskLevel(targetAuthorizationManagement, preAuthorizationManagement);
            authorizationManagementBl.UpdateAccountHolder(targetAuthorizationManagement, preAuthorizationManagement);
            authorizationManagementBl.HandleBackControlReasonAnnotaionInDigitalFormVerification(targetAuthorizationManagement, preAuthorizationManagement);
        }
    }
}