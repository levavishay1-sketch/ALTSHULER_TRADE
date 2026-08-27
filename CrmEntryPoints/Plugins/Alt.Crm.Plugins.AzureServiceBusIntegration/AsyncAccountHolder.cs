using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Plugins.AzureServiceBusIntegration
{
    //public class AsyncAccountHolder : PluginBase
    //{
    //    private Guid serviceEndpointId;
    //    public AsyncAccountHolder(string unsecure, string secure)
    //        : base(typeof(AsyncAccountHolder))
    //    {
    //        if (String.IsNullOrEmpty(secure) || !Guid.TryParse(secure, out serviceEndpointId))
    //        {
    //            throw new InvalidPluginExecutionException("Service endpoint ID should be passed in config.");
    //        }
    //    }

    //    protected override void ExecuteCrmPlugin(LocalContext localContext)
    //    {
    //        var targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();
    //        if (targetAccountHolder.alt_TransferToTradeOneStatusCode != null
    //            && targetAccountHolder.alt_TransferToTradeOneStatusCode.Value == (int)TransferStatusCode.Sending)
    //        {
    //            localContext.PluginExecutionContext.SharedVariables.Add(nameof(ApiConfigurationCode), (int)ApiConfigurationCode.OpenTradeOneUser);
    //            localContext.ExecuteCloudService(this.serviceEndpointId);
    //        }
    //    }
    //}
}
