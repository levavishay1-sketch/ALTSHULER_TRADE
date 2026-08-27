using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.CustomAPIs.DigitalForm
{
    public class DigitalForm : ExternalServiceActionBase
    {
        public DigitalForm(string unsecure, string secure): base(typeof(DigitalForm)) { }



        protected override ActionResult ExecuteCustomApiBusinessLogic(GlobalContext globalContext)
        {
            ICrmOutgoing<ApiDigitalForm> crmOutgoingBL = new DigitalFormBL(globalContext);
            ActionResult actionResult = crmOutgoingBL.ExecuteOutgoingLogicHandler(null);
            return actionResult;            
        }
    }
}
