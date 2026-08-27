using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncDocument : PluginBase
    {
        private Guid serviceEndpointId;

        public AsyncDocument(string unsecure, string secure) : base(typeof(AsyncDocument))
        {
            if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetDocument = localContext.TargetEntity?.ToEntity<alt_Document>();

            this.HandleDocumentUpload(localContext, targetDocument);
            this.HandleDocumentDownload(localContext, targetDocument);
            this.HandleDocumentUpdate(localContext, targetDocument);
        }

        private void HandleDocumentUpload(LocalContext localContext, alt_Document targetDocument)
        {
            if (targetDocument.Contains(alt_Document.Fields.alt_ArchiveTransferStatusCode) &&
                targetDocument.GetAttributeValue<OptionSetValue>(alt_Document.Fields.alt_ArchiveTransferStatusCode).Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.DocumentUpload);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }

        private void HandleDocumentDownload(LocalContext localContext, alt_Document targetDocument)
        {
            if (targetDocument.Contains(alt_Document.Fields.alt_ArchiveDownloadStatusCode) &&
                targetDocument.GetAttributeValue<OptionSetValue>(alt_Document.Fields.alt_ArchiveDownloadStatusCode).Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.DocumentDownload);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }

        private void HandleDocumentUpdate(LocalContext localContext, alt_Document targetDocument)
        {
            if (targetDocument.Contains(alt_Document.Fields.alt_ArchiveUpdateStatusCode) &&
                targetDocument.GetAttributeValue<OptionSetValue>(alt_Document.Fields.alt_ArchiveUpdateStatusCode).Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.DocumentFilingUpdate);
                localContext.ExecuteCloudService(this.serviceEndpointId);
            }
        }
    }
}
