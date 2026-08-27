using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    public class HealthController : BaseController
    {
        public string Get()
        {
            
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.ConnectionString = ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
            telemetryClient.TrackTrace("ok");
            return "ok";
        }
    }
}