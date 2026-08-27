using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncScheduledOperation : PluginBase
    {
        private Guid serviceEndpointId;
        public AsyncScheduledOperation(string unsecure, string secure)
            : base(typeof(AsyncScheduledOperation))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetScheduledOperation = localContext.TargetEntity?.ToEntity<alt_ScheduledOperation>();
            if (targetScheduledOperation.AttributeHasValue<OptionSetValue>(alt_ScheduledOperation.Fields.StatusCode) 
                && targetScheduledOperation.StatusCode.Value == (int)ScheduledOperationStatusCode.Run)
            {
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
