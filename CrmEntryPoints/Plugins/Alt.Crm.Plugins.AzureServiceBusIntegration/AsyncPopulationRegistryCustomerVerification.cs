using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncPopulationRegistryCustomerVerification: PluginBase
    {
        private Guid serviceEndpointId;

        public AsyncPopulationRegistryCustomerVerification(string unsecure, string secure) : base(typeof(AsyncPopulationRegistryCustomerVerification))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPopulationRegistryCustomerVerification = localContext.TargetEntity?.ToEntity<alt_PopulationRegistryCustomerVerification>();
            if (targetPopulationRegistryCustomerVerification.alt_TransferStatusCode != null
               && targetPopulationRegistryCustomerVerification.alt_TransferStatusCode.Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.PopulationRegisterVerification);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
