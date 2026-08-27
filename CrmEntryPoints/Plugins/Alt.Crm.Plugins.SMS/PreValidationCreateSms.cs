using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.SMS
{
    public class PreValidationCreateSms : PluginBase
    {
        public PreValidationCreateSms(string unsecure, string secure): base(typeof(PreValidationCreateSms)) { }


        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_SMS targetSms = localContext.TargetEntity?.ToEntity<alt_SMS>();

            SmsBL smsBl = new SmsBL(localContext.ToGlobal());
            smsBl.HandleSetSmsMobilePhoneByContact(targetSms);
            smsBl.HandleSmsSendStatus(targetSms, targetSms);
            smsBl.SetOwner(targetSms);
        }
    }
}
