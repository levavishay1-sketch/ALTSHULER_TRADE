using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Opportunity
{
    public class SystemAsyncCreateOpportunity : PluginBase
    {
        public SystemAsyncCreateOpportunity(string unsecure, string secure) 
            : base(typeof(SystemAsyncCreateOpportunity),false)
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBl = new OpportunityBL(localContext.ToGlobal());
            opportunityBl.HandleRepresentativeRewardCreate(targetOpportunity);
            opportunityBl.HandleJoiningProcessSummary(targetOpportunity);
            opportunityBl.LinkOpportunityToDigitalFormVerification(targetOpportunity);
        }
    }
}