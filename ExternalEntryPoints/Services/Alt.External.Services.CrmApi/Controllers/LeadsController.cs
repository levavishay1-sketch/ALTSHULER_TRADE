using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Framework;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    [GlobalContextManagerAttribute]
    public class LeadsController : BaseController
    {
        public IHttpActionResult Post([FromBody] ApiLead leadApi)
        {
            LeadBL leadBl = new LeadBL(ThirdPartyBase.GlobalContext);

            ActionResult actionResult = leadBl.HandleCreateLead(leadApi);
            return base.HandleGenerateResponse(actionResult);
        }

        public IHttpActionResult Put([FromBody] ApiLead leadApi)
        {
            LeadBL leadBl = new LeadBL(ThirdPartyBase.GlobalContext);

            ActionResult actionResult = leadBl.HandleUpdateLead(leadApi);
            return base.HandleGenerateResponse(actionResult);
        }
    }
}