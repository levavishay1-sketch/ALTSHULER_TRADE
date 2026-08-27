using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Lead
{
    public class PreQualifyLead : PluginBase
    {
        public PreQualifyLead(string unsecure, string secure)
      : base(typeof(PreQualifyLead))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            LeadBL leadBl = new LeadBL(localContext.ToGlobal());
            leadBl.HandlePreQualifyLead(localContext.PluginExecutionContext.InputParameters, localContext.PluginExecutionContext.ParentContext);
        }
    }
}
