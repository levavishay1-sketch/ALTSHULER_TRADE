using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    public class AsyncDigitalForm : PluginBase
    {
        private Dictionary<string, Guid> serviceBusEndpointsIds;
        public AsyncDigitalForm(string unsecure, string secure)
            : base(typeof(AsyncDigitalForm))
        {
            if (String.IsNullOrEmpty(secure))
            {
                throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
            }

            serviceBusEndpointsIds = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Guid>>(secure);

        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetDigitalForm = localContext.TargetEntity?.ToEntity<alt_DigitalForm>();

            this.HandleDigitalFormCreateInOutSystem(localContext, targetDigitalForm);
            this.HandleRedirectOutSystemRequest(localContext, targetDigitalForm);
        }

        private void HandleDigitalFormCreateInOutSystem(LocalContext localContext, alt_DigitalForm targetDigitalForm)
        {

            if (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_TransferToOutSystemStatusCode)
                && targetDigitalForm.alt_TransferToOutSystemStatusCode.Value == (int)TransferStatusCode.Sending)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.CreateDigitalFormInOutSystem);
                Guid serviceEndpointId = serviceBusEndpointsIds["CrmOutgoingQueue"];
                localContext.ExecuteCloudService(serviceEndpointId);
            }
        }

        private void HandleRedirectOutSystemRequest(LocalContext localContext, alt_DigitalForm targetDigitalForm)
        {
            Guid serviceEndpointId = serviceBusEndpointsIds["DigitalFormQueue"];

            if (targetDigitalForm.AttributeHasValue<string>(alt_DigitalForm.Fields.alt_DigitalFormDetails)
                && !targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_DataReceptionStatusCode))
            {
                localContext.ExecuteCloudService(serviceEndpointId);
            }
            else if (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_DataReceptionStatusCode)
                               && targetDigitalForm.alt_DataReceptionStatusCode.Value == (int)DataReceptionStatusCode.UnderConstruction)
            {
                localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.JoiningFormDataRecipient);
                localContext.ExecuteCloudService(serviceEndpointId);
            }
        }
    }
}
