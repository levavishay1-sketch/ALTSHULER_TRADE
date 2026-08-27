using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.CustomAPIs.SmsOutgoing
{
    public class SmsOutgoing: ExternalServiceActionBase
    {
        public SmsOutgoing(string unsecure, string secure) : base(typeof(SmsOutgoing)) { }
        protected override ActionResult ExecuteCustomApiBusinessLogic(GlobalContext globalContext)
        {
            ICrmOutgoing<ApiSms> crmOutgoingBL = new SmsBL(globalContext);
            ActionResult actionResult = crmOutgoingBL.ExecuteOutgoingLogicHandler(null);
            return actionResult;
        }
    }
}
