//using Alt.BusinessLogicLayer.Crm;
//using Alt.DataModel.Crm.Entities;
//using Alt.Framework.EntryPoints.Crm;

//namespace Alt.Crm.Plugins.SMS
//{
//    public class AsyncCreateSms : PluginBase
//    {
//        public AsyncCreateSms(string unsecure, string secure) : base(typeof(AsyncCreateSms)) { }

//        protected override void ExecuteCrmPlugin(LocalContext localcontext)
//        {
//            alt_SMS targetSms = localcontext.TargetEntity?.ToEntity<alt_SMS>();
//            SmsBL smsBL = new SmsBL(localcontext.ToGlobal());

//            smsBL.HandleSendSmsRequest(targetSms);
//        }
//    }
//}
