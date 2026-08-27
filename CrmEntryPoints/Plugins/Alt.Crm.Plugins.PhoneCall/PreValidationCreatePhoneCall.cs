using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PhoneCall
{
    public class PreValidationCreatePhoneCall : PluginBase
    {
        public PreValidationCreatePhoneCall(string unsecure, string secure) : base(typeof(PreValidationCreatePhoneCall)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.PhoneCall targetPhoneCall = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();

            PhoneCallBL phoneCallBL = new PhoneCallBL(localContext.ToGlobal());
            phoneCallBL.SetStatusCodeByCallStatusCode(targetPhoneCall);
            phoneCallBL.SetStateCodeByStatusCode(targetPhoneCall);
        }
    }
}








