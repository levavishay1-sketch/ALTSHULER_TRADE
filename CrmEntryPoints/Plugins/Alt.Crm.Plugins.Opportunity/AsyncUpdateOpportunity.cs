using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Opportunity
{
    public class AsyncUpdateOpportunity : PluginBase
    {
        public AsyncUpdateOpportunity(string unsecure, string secure) : base(typeof(AsyncUpdateOpportunity)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();
            DataModel.Crm.Entities.Opportunity preOpportunity = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBL = new OpportunityBL(localContext.ToGlobal());
            opportunityBL.HandleCloseRelatedActivitiesOnOpportunityClosed(targetOpportunity);
            opportunityBL.HandleEncouragingDepositSystemUserInRelatedDigitalFormVerification(targetOpportunity, preOpportunity);
        }
    }
}
