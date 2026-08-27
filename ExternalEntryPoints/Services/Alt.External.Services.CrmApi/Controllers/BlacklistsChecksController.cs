using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Framework;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    [GlobalContextManagerAttribute]
    public class BlacklistsChecksController : BaseController
    {
        public IHttpActionResult Put([FromBody] ApiBlacklistsCheck apiBlacklistsCheck)
        {
            BlacklistsCheckBL blacklistsCheckBl = new BlacklistsCheckBL(ThirdPartyBase.GlobalContext);

            ActionResult actionResult = blacklistsCheckBl.Update(apiBlacklistsCheck);
            return base.HandleGenerateResponse(actionResult);
        }
    }
}