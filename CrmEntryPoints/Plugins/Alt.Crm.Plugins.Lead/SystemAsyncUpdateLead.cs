using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;
namespace Alt.Crm.Plugins.Lead
{
    public class SystemAsyncUpdateLead : PluginBase
    {
        public SystemAsyncUpdateLead(string unsecure, string secure)
          : base(typeof(SystemAsyncUpdateLead), false)
        {
        }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity != null ?
               localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;

            DataModel.Crm.Entities.Lead preLead = localContext.PreEntity != null ?
                 localContext.PreEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;

            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandleRepresentativeRewardCreate(targetLead, preLead);
        }
    }
}
