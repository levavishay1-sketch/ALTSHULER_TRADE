using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PhoneCall
{
    public class AsyncCreatePhoneCall : PluginBase
    {
        public AsyncCreatePhoneCall(string unsecure, string secure) : base(typeof(AsyncCreatePhoneCall)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.PhoneCall targetPhoneCall = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();

            PhoneCallBL phoneCallBL = new PhoneCallBL(localContext.ToGlobal());
            phoneCallBL.HandlePhoneCallsCountForAndScheduledTreatmentDateRelatedLeadOrOpportunity(targetPhoneCall);
            phoneCallBL.HandleRegardingObjectUpdate(targetPhoneCall);
            phoneCallBL.HandleCallbackPhoneCallForLead(targetPhoneCall);
        }
    }
}