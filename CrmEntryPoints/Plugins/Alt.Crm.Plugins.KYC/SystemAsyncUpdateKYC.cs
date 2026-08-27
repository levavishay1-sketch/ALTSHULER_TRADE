using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.KYC
{
    public class SystemAsyncUpdateKYC : PluginBase
    {
        public SystemAsyncUpdateKYC(string unsecure, string secure)
            : base(typeof(SystemAsyncUpdateKYC), false) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_KYC targetKYC = localContext.TargetEntity?.ToEntity<alt_KYC>();
            alt_KYC preKYC = localContext.PreEntity?.ToEntity<alt_KYC>();

            KYCBL kycBl = new KYCBL(localContext.ToGlobal());
            kycBl.HandlelScoresSectionInternalBit(targetKYC, preKYC);
            kycBl.UpdateDigitalFormVerification(targetKYC, preKYC);

            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());

            managerControlChangeTrackingBL.TrackChanges(targetKYC, preKYC);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetKYC, preKYC);
        }
    }
}
