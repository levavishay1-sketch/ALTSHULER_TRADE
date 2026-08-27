using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
    public class SystemAsyncCreateLead : PluginBase
    {
        public SystemAsyncCreateLead(string unsecure, string secure) 
            : base(typeof(SystemAsyncCreateLead), false)
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>();

            LeadBL leadBL = new LeadBL(localContext.ToGlobal());
            leadBL.HandleSystemAsyncCreateLead(targetLead);
            leadBL.HandleJoiningProcessSummary(targetLead);
        }
    }
}