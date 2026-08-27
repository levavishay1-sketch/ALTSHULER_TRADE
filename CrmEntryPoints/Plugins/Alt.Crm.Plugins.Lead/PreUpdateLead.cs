using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
    public class PreUpdateLead : PluginBase
    {
        public PreUpdateLead(string unsecure, string secure) : base(typeof(PreUpdateLead))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity != null ?
                 localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;
            DataModel.Crm.Entities.Lead preLead = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.Lead>();

            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandleDisqualifyLead(targetLead, localContext.PluginExecutionContext.ParentContext);
            leadBl.HandleIdentityNumber(targetLead, preLead);
            leadBl.HandleLeadSourceCode(targetLead, preLead);
            leadBl.HandleClosedOnDate(targetLead, preLead);
            leadBl.HandleTreatmentStatusByAssignee(targetLead, preLead);
        }
    }
}
