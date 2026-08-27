using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class PostBlacklistsCheck : PluginBase
    {
        private Guid ServiceEndpointId;
        public PostBlacklistsCheck(string unsecure, string secure) : base(typeof(PostBlacklistsCheck))
        {
            if (string.IsNullOrWhiteSpace(secure) || !Guid.TryParse(secure, out ServiceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_BlacklistsCheck targetBlacklistsCheck = localContext.TargetEntity?.ToEntity<alt_BlacklistsCheck>();

            if (targetBlacklistsCheck.StatusCode?.Value == (int)BlacklistsCheckStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.BlacklistsCheck);
                localContext.ExecuteCloudService(ServiceEndpointId);
            }
        }
    }
}