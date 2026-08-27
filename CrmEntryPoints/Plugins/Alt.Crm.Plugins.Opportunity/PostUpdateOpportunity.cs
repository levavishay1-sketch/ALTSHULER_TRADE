using Alt.Framework.EntryPoints.Crm;
using Alt.BusinessLogicLayer.Crm;

namespace Alt.Crm.Plugins.Opportunity
{
    public class PostUpdateOpportunity : PluginBase
    {
        public PostUpdateOpportunity(string unsecure, string secure) : base(typeof(PostUpdateOpportunity))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();
            DataModel.Crm.Entities.Opportunity postOpportunity = localContext.PostEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBl = new OpportunityBL(localContext.ToGlobal());
            opportunityBl.HandleCloseOpportunity(targetOpportunity, postOpportunity);
        }
    }
}
