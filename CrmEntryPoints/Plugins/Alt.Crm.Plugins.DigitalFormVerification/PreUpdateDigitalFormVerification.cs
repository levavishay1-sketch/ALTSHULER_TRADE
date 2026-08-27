using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalFormVerification
{
    public class PreUpdateDigitalFormVerification : PluginBase
    {
        public PreUpdateDigitalFormVerification(string unsecure, string secure) : base(typeof(PreUpdateDigitalFormVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalFormVerification targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();
            alt_DigitalFormVerification preDigitalFormVerification = localContext.PreEntity?.ToEntity<alt_DigitalFormVerification>();

            DigitalFormVerificationBL digitalFormVerificationBl = new DigitalFormVerificationBL(localContext.ToGlobal());
            digitalFormVerificationBl.SetFormStatusCode(targetDigitalFormVerification, preDigitalFormVerification);
            digitalFormVerificationBl.HandleLinkedPortfolioId(targetDigitalFormVerification);
            digitalFormVerificationBl.SetManagerVerificationRequiredCode(targetDigitalFormVerification, preDigitalFormVerification);
            digitalFormVerificationBl.SetTransferToShenhavStatusCode(targetDigitalFormVerification, preDigitalFormVerification);
            digitalFormVerificationBl.SetCommissionClientType(targetDigitalFormVerification);

            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());
            managerControlChangeTrackingBL.TrackChanges(targetDigitalFormVerification, preDigitalFormVerification);

        }
    }
}