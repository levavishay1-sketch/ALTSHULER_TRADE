using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
    public class PreCreateLead : PluginBase
    {
        public PreCreateLead(string unsecure, string secure) : base(typeof(PreCreateLead)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity != null ?
                 localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;

            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandleIdentityNumber(targetLead);
            leadBl.SetReferralSource(targetLead);
            leadBl.SetRefferingCustomerCalculatedAccountNumber(targetLead);
            leadBl.SetDefaultTreatmentStatus(targetLead);
        }
    }
}
