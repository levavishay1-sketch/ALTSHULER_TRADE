using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;
using System;

namespace Alt.Crm.Plugins.PhoneCall
{
    public class PreCreatePhoneCall : PluginBase
    {
        public PreCreatePhoneCall(string unsecure, string secure) : base(typeof(PreCreatePhoneCall)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPhoneCall = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.PhoneCall>();

            PhoneCallBL phoneCallBL = new PhoneCallBL(localContext.ToGlobal());
            phoneCallBL.HandleSubjectByCreationMethod(targetPhoneCall);
        }
    }
}
