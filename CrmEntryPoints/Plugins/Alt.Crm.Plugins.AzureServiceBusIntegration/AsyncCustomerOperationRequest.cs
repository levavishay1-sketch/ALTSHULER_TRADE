using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncCustomerOperationRequest : PluginBase
    {
        private Guid serviceEndpointId;
        public AsyncCustomerOperationRequest(string unsecure, string secure)
            : base(typeof(AsyncCustomerOperationRequest))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetCustomerOperationRequest = localContext.TargetEntity?.ToEntity<alt_CustomerOperationRequest>();

            if (targetCustomerOperationRequest.StatusCode != null
                && targetCustomerOperationRequest.StatusCode.Value == (int)CustomerOperationRequestStatusCode.Sending)
            {
                var preCustomerOperationRequest = localContext.PreEntity?.ToEntity<alt_CustomerOperationRequest>();
                int? apiConfigurationCode = preCustomerOperationRequest != null
                    ? preCustomerOperationRequest.alt_ApiConfigurationCodeInt :
                    targetCustomerOperationRequest.alt_ApiConfigurationCodeInt;

                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), apiConfigurationCode);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
