using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.External.Services.CrmApi.Framework;
using System;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    [GlobalContextManagerAttribute]
    public class SystemUsersController : BaseController
    {
        [Route("api/systemusers/{systemUserId}")]
        public IHttpActionResult Get(Guid systemUserId)
        {
            SystemUserBL systemUserBl = new SystemUserBL(base.ThirdPartyBase.GlobalContext);
            ActionResult actionResult = systemUserBl.Get(systemUserId);

            return base.HandleGenerateResponse(actionResult);
        }
    }
}