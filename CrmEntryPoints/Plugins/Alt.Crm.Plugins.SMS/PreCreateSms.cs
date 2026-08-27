using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.SMS
{
    public class PreCreateSms : PluginBase
    {
        public PreCreateSms(string unsecure, string secure): base(typeof(PreCreateSms)) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            alt_SMS targetSms = localcontext.TargetEntity?.ToEntity<alt_SMS>();
            SmsBL smsBL = new SmsBL(localcontext.ToGlobal());

            smsBL.HandleSMSSender(targetSms);
            smsBL.HandleSMSAutomaticCreationByTemplateCode(targetSms);
        }
    }
}
