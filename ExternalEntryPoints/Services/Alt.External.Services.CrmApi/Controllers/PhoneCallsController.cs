using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External;
using Alt.External.Services.CrmApi.Framework;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    public class PhoneCallsController : BaseController
    {
        [GlobalContextManagerAttribute]
        public IHttpActionResult Post([FromBody] ApiPhoneCallList CallAttempts)
        {
            PhoneCallBL phoneCallBL = new PhoneCallBL(ThirdPartyBase.GlobalContext);
            ActionResult actionResult = phoneCallBL.CreatePhoneCallsAndUpdateLeads(CallAttempts);

            return base.HandleGenerateResponse(actionResult);
        }
    }
}