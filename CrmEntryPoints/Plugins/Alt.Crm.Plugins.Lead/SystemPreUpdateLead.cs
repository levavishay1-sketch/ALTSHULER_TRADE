using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
    public class SystemPreUpdateLead : PluginBase
    {
        public SystemPreUpdateLead(string unsecure, string secure) : base(typeof(SystemPreUpdateLead), false) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Lead targetLead = localContext.TargetEntity != null ?
                localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Lead>() : null;

            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandleAssignToMe(targetLead, localContext.PluginExecutionContext.InitiatingUserId);
        }
    }
}
