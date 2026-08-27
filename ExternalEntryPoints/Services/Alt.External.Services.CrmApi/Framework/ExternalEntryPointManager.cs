using Alt.External.Services.CrmApi.Controllers;
using Alt.Framework.EntryPoints.External;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;

namespace Alt.External.Services.CrmApi.Framework
{
    public class ExternalEntryPointManager
    {
        private static ConnectionQueue connectionQueue = new ConnectionQueue();

        public static void Connect(HttpActionContext actionContext)
        {
            var baseController = (actionContext.ControllerContext.Controller as BaseController);
            if (baseController.ThirdPartyBase?.GlobalContext == null)
            {

                if (!actionContext.Request.Headers.TryGetValues("RequestId", out IEnumerable<string> headerValues)
                    || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault())
                    || !Guid.TryParse(headerValues.FirstOrDefault(), out Guid requestId))
                {
                    requestId = Guid.NewGuid();
                }
                var crmService = connectionQueue.GetConnection();
                if (crmService != null)
                {
                    ThirdPartyHandler(crmService, baseController, requestId, actionContext);
                }
                else
                {
                    throw new Exception("Can not Initialize or Get Crm Connection");
                }
            }
        }

        private static void ThirdPartyHandler(CrmServiceManager crmService, BaseController baseController, Guid requestId, HttpActionContext actionContext)
        {
            baseController.ThirdPartyBase = new ThirdPartyBase(crmService, actionContext.ControllerContext.Controller.GetType(),
                requestId, actionContext.ActionDescriptor.ActionName);

        }


        public static ThirdPartyBase Connect(ExceptionHandlerContext context)
        {
            Type childClassName = typeof(BaseController);
            string customTitle = string.Empty;
            if (!context.Request.Headers.TryGetValues("RequestId", out IEnumerable<string> headerValues)
                   || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault())
                   || !Guid.TryParse(headerValues.FirstOrDefault(), out Guid requestId))
            {
                requestId = Guid.NewGuid();
            }

            var crmService = connectionQueue.GetConnection();
            ThirdPartyBase thirdPartyBase = new ThirdPartyBase(crmService, childClassName, requestId, customTitle);
            return thirdPartyBase;
        }

        public static void LogRequest(HttpActionContext actionContext)
        {
            var baseController = (actionContext.ControllerContext.Controller as BaseController);
            var request = actionContext.Request.ToString();
            string bodyContent = actionContext.Request.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrWhiteSpace(bodyContent))
            {
                var stream = new MemoryStream();
                var inSteam = HttpContext.Current.Request.InputStream;
                inSteam.Seek(0, SeekOrigin.Begin);
                inSteam.CopyTo(stream);
                bodyContent = Encoding.UTF8.GetString(stream.ToArray());
            }

            RequestContentLogResolver requestContentLogResolver = new RequestContentLogResolver(baseController.ThirdPartyBase.GlobalContext);
            string requestBodyContetntToLog = requestContentLogResolver.GetRequestBodyToLog(actionContext, bodyContent);
            string requestDetails = $"\nRequest:\n{request}\nContent:\n{requestBodyContetntToLog}";
            baseController.ThirdPartyBase.GlobalContext.Log.Info(requestDetails);
        }
    }
}