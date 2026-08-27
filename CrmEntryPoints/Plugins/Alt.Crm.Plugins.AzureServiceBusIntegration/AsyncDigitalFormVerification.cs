using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncDigitalFormVerification : PluginBase
    {
        private Guid serviceEndpointId;
        public AsyncDigitalFormVerification(string unsecure, string secure)
            : base(typeof(AsyncDigitalFormVerification))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();
            if (targetDigitalFormVerification.alt_TransferToShenhavStatusCode != null
                    && targetDigitalFormVerification.alt_TransferToShenhavStatusCode.Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.OpenPortfolioInShenhav);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
