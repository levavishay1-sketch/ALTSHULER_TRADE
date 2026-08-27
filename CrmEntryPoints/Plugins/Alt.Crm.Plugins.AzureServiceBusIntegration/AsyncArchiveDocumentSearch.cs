using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncArchiveDocumentSearch : PluginBase
    {
        private Guid serviceEndpointId;

        public AsyncArchiveDocumentSearch(string unsecure, string secure) : base(typeof(AsyncArchiveDocumentSearch))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetArchiveDocumentSearch = localContext.TargetEntity?.ToEntity<alt_ArchiveDocumentSearch>();
            this.HandleDocumentSearch(localContext, targetArchiveDocumentSearch);
        }

        private void HandleDocumentSearch(LocalContext localContext, alt_ArchiveDocumentSearch targetArchiveDocumentSearch)
        {
            if (targetArchiveDocumentSearch.Contains(alt_ArchiveDocumentSearch.Fields.alt_SearchFromArchiveStatusCode) &&
                targetArchiveDocumentSearch.GetAttributeValue<OptionSetValue>(alt_ArchiveDocumentSearch.Fields.alt_SearchFromArchiveStatusCode).Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.DocumentSearch);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
