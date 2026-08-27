using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.External.Services.CrmApi.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    [GlobalContextManagerAttribute]
    public class OccupationsController : BaseController
    {
        public IHttpActionResult Get()
        {
            OccupationBL occupationBl = new OccupationBL(base.ThirdPartyBase.GlobalContext);
            ActionResult actionResult = occupationBl.Get();

            return base.HandleGenerateResponse(actionResult);
        }
    }
}