//using Alt.External.Services.CrmApi.Controllers;
//using System.Web.Http;
//using System.Web.Http.Controllers;

//namespace Alt.External.Services.CrmApi.Framework
//{
//    public class BearerAuthorizationAttribute : AuthorizeAttribute
//    {
//        public override void OnAuthorization(HttpActionContext actionContext)
//        {         
//            ExternalEntryPointManager.Connect(actionContext);        
//            var controller = actionContext.ControllerContext.Controller as BaseController;

//            if (!base.IsAuthorized(actionContext))
//            {
//                controller.ThirdPartyBase.GlobalContext.Log.Critical("Authorization has been denied for this request");
//                ExternalEntryPointManager.LogRequest(actionContext);
//                controller.ThirdPartyBase.Dispose();
//            }
//            else
//            {
//                actionContext.Request.Headers.Remove("Authorization");
//            }
//            HandleUnauthorizedRequest(actionContext);
//        }
//    }
//}