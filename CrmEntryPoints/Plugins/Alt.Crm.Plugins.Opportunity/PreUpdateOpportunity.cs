using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;

namespace Alt.Crm.Plugins.Opportunity
{
    public class PreUpdateOpportunity : PluginBase
    {
        public PreUpdateOpportunity(string unsecure, string secure) : base(typeof(PreUpdateOpportunity))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Opportunity targetOpportunity = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();
            DataModel.Crm.Entities.Opportunity preOpportunity = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.Opportunity>();

            OpportunityBL opportunityBl = new OpportunityBL(localContext.ToGlobal());
            opportunityBl.SetOpportunityName(targetOpportunity, preOpportunity);
            opportunityBl.ResetOportunityOperation(targetOpportunity, preOpportunity);
        }
    }
}
