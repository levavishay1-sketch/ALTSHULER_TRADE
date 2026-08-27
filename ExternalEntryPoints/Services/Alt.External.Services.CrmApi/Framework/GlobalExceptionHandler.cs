using Alt.External.Services.CrmApi.Controllers;
using Alt.Framework.EntryPoints.External;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace Alt.External.Services.CrmApi.Framework
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            HttpControllerContext controllerContext = context.ExceptionContext.ControllerContext;
            ThirdPartyBase thirdPartyBase = null;
            var baseController = controllerContext != null ? (controllerContext.Controller as BaseController) : null;
            try
            {
                if (baseController != null)
                {
                    ExternalEntryPointManager.Connect(baseController.ActionContext);
                    baseController.ThirdPartyBase?.GlobalContext?.Log.Critical(context.Exception.ToString());
                }
                else
                {
                    thirdPartyBase = ExternalEntryPointManager.Connect(context);
                    thirdPartyBase.GlobalContext.Log.Critical(context.Exception.ToString());
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(ex.ToString(), context.Exception);
                WriteToApplicationInsights(ex2);
            }
            finally
            {
                context.Result = new ResponseMessageResult(context.Request.CreateResponse(HttpStatusCode.InternalServerError));
                baseController?.ThirdPartyBase?.Dispose();
                thirdPartyBase?.Dispose();
            }
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }
        protected void WriteToApplicationInsights(Exception ex)
        {
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.ConnectionString = ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
            telemetryClient.TrackException(ex);
        }
    }
}