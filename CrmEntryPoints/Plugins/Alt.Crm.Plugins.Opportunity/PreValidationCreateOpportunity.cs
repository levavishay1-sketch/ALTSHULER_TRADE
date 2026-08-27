using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Opportunity
{
    public class PreValidationCreateOpportunity: PluginBase
    {
        public PreValidationCreateOpportunity(string unsecure, string secure) : base(typeof(PreValidationCreateOpportunity))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBl = new OpportunityBL(localContext.ToGlobal());
            opportunityBl.SetOwner(targetOpportunity);
        }
    }
}
