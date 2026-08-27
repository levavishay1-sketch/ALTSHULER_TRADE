using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalFormVerification
{
    public class AsyncUpdateDigitalFormVerification : PluginBase
    {
        public AsyncUpdateDigitalFormVerification(string unsecure, string secure) : base(typeof(AsyncUpdateDigitalFormVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalFormVerification targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();
            alt_DigitalFormVerification preDigitalFormVerification = localContext.PreEntity?.ToEntity<alt_DigitalFormVerification>();

            DigitalFormVerificationBL digitalFormVerificationBl = new DigitalFormVerificationBL(localContext.ToGlobal());
            digitalFormVerificationBl.HandleFormStatusChanged(targetDigitalFormVerification, preDigitalFormVerification);
        }
    }
}