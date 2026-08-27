using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Opportunity
{
    public class SystemAsyncUpdateOpportunity : PluginBase
    {
        public SystemAsyncUpdateOpportunity(string unsecure, string secure) 
            : base(typeof(SystemAsyncUpdateOpportunity),false)
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();
            DataModel.Crm.Entities.Opportunity preOpportunity = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBl = new OpportunityBL(localContext.ToGlobal());
            opportunityBl.HandleRepresentativeRewardCreate(targetOpportunity, preOpportunity);
        }
    }
}
