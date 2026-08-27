using Alt.Framework.EntryPoints.Crm;
using Alt.BusinessLogicLayer.Crm;

namespace Alt.Crm.Plugins.Opportunity
{
    public class PreCreateOpportunity : PluginBase
    {
        public PreCreateOpportunity(string unsecure, string secure) : base(typeof(PreCreateOpportunity))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBl = new OpportunityBL(localContext.ToGlobal());
            opportunityBl.SetOpportunityName(targetOpportunity, targetOpportunity);
        }
    }
}
