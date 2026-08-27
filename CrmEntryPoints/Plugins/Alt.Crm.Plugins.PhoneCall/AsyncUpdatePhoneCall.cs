using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PhoneCall
{
    public class AsyncUpdatePhoneCall : PluginBase
    {
        public AsyncUpdatePhoneCall(string unsecure, string secure) : base(typeof(AsyncUpdatePhoneCall)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.PhoneCall targetPhoneCall = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();
            DataModel.Crm.Entities.PhoneCall prePhoneCall = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();

            PhoneCallBL phoneCallBL = new PhoneCallBL(localContext.ToGlobal());
            phoneCallBL.HandleLeadScheduledTreatmentDate(targetPhoneCall, prePhoneCall);
            phoneCallBL.HandleRegardingObjectUpdate(targetPhoneCall, prePhoneCall);
            phoneCallBL.HandleCallbackPhoneCallForLead(targetPhoneCall, prePhoneCall);
        }
    }
}