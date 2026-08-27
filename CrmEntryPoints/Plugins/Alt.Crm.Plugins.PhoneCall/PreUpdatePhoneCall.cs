using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PhoneCall
{
    public class PreUpdatePhoneCall : PluginBase
    {
        public PreUpdatePhoneCall(string unsecure, string secure) : base(typeof(PreUpdatePhoneCall)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPhoneCall = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();
            var prePhoneCall = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();

            PhoneCallBL phoneCallBL = new PhoneCallBL(localContext.ToGlobal());
            phoneCallBL.ValidateFieldsBeforeCompletion(targetPhoneCall, prePhoneCall);
        }
    }
}
