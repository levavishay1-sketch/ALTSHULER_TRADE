using Alt.External.Services.CrmApi.App_Start;
using Alt.External.Services.CrmApi.Cache;
using System.Web.Http;

namespace Alt.External.Services.CrmApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //ControllersAvailableDataCache.DeserializeXml();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
