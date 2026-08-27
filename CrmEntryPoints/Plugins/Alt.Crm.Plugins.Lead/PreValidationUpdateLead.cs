using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
   public class PreValidationUpdateLead : PluginBase
    {
        public PreValidationUpdateLead(string unsecure, string secure) 
            : base(typeof(PreValidationUpdateLead)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity != null ?
                 localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;

            DataModel.Crm.Entities.Lead preLead = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.Lead>();

            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandleLeadStateCode(targetLead);
        }
    }
}
