using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.CustomAPIs.CrmApi
{
    public class CrmApi : ExternalServiceActionBase
    {
        public CrmApi(string unsecure, string secure) : base(typeof(CrmApi)) { }
        protected override ActionResult ExecuteCustomApiBusinessLogic(GlobalContext globalContext)
        {
            BusinessLogicFactory businessLogicStrategy = new BusinessLogicFactory(globalContext);
            ActionResult actionResult = businessLogicStrategy.Execute();

            return actionResult;
        }
    }
}
