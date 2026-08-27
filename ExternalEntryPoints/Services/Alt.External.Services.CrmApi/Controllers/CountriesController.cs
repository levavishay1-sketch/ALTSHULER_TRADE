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
    public class CountriesController : BaseController
    {
        public IHttpActionResult Get()
        {
            CountryBL countryBl = new CountryBL(base.ThirdPartyBase.GlobalContext);
            ActionResult actionResult = countryBl.Get();

            return base.HandleGenerateResponse(actionResult);
        }
    }
}