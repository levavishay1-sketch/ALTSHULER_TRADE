using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalFormVerification
{
    public class PreCreateDigitalFormVerification : PluginBase
    {
        public PreCreateDigitalFormVerification(string unsecure, string secure) : base(typeof(PreCreateDigitalFormVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalFormVerification targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();

            DigitalFormVerificationBL digitalFormVerificationBl = new DigitalFormVerificationBL(localContext.ToGlobal());
            digitalFormVerificationBl.SetVerificationReceivedDate(targetDigitalFormVerification);
            digitalFormVerificationBl.SetLeadValuesByLinkedDigitalForm(targetDigitalFormVerification);
            digitalFormVerificationBl.SetManagerVerificationRequiredCode(targetDigitalFormVerification);
            digitalFormVerificationBl.SetEncouragingDepositSystemUserByRelatedOpportunityOwner(targetDigitalFormVerification);
        }
    }
}