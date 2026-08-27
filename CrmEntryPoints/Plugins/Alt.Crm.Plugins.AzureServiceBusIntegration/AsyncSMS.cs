using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncSMS : PluginBase
    {
        private Guid serviceEndpointId;
        public AsyncSMS(string unsecure, string secure)
            : base(typeof(AsyncSMS))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetSms = localContext.TargetEntity?.ToEntity<alt_SMS>();
            if (targetSms.Contains(alt_SMS.Fields.StatusCode)
                && targetSms.GetAttributeValue<OptionSetValue>(alt_SMS.Fields.StatusCode).Value == (int)SmsStatusCode.SendingNow)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.SendSms);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
