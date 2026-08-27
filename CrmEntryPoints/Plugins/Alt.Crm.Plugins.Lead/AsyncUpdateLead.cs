using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
    public class AsyncUpdateLead : PluginBase
    {
        public AsyncUpdateLead(string unsecure, string secure)
            : base(typeof(AsyncUpdateLead))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity != null ?
                 localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;

            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandleUpdateDigitalForm(targetLead);
            leadBl.HandleCloseRelatedActivitiesOnLeadClosed(targetLead);
        }
    }
}
