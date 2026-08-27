using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.KYC
{
    public class SystemAsyncCreateKYC : PluginBase
    {
        public SystemAsyncCreateKYC(string unsecure, string secure) : base(typeof(SystemAsyncCreateKYC),false)
        {
        }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_KYC targetKYC = localContext.TargetEntity?.ToEntity<alt_KYC>();

            ManagerControlChangeTrackingBL managerControlChangeTrackingBL =
                new ManagerControlChangeTrackingBL(localContext.ToGlobal());

            managerControlChangeTrackingBL.TrackChanges(targetKYC);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetKYC);

        }
    }
}