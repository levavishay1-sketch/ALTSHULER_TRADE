using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.SMS
{
    public class PreValidationUpdateSms : PluginBase
    {
        public PreValidationUpdateSms(string unsecure, string secure) : base(typeof(PreValidationUpdateSms)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_SMS targetSms = localContext.TargetEntity?.ToEntity<alt_SMS>();
            alt_SMS preSms = localContext.PreEntity?.ToEntity<alt_SMS>();

            SmsBL smsBL = new SmsBL(localContext.ToGlobal());
            smsBL.HandleSmsSendStatus(targetSms, preSms);
        }
    }
}
