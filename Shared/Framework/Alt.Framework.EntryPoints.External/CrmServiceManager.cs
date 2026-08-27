using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;

namespace Alt.Framework.EntryPoints.External
{
    public class CrmServiceManager : ICrmServiceManager
    {        
        public string CrmConnectionString { get; private set; }

        public CrmServiceManager(string crmConnectionString)
        {
            this.CrmConnectionString = crmConnectionString;
        }

        private IOrganizationService InitializeConnection()
        {
            var crmServiceClient = new CrmServiceClient(this.CrmConnectionString);
            if (crmServiceClient != null && crmServiceClient.IsReady)
            {
                crmServiceClient.EnableAffinityCookie = false;
            }
            else
            {
                throw new Exception($"Failed to Successfully Create CrmServiceClient: {crmServiceClient.LastCrmError}", crmServiceClient.LastCrmException);
            }
            return crmServiceClient;
        }

        public IOrganizationService GetService()
        {
            return this.InitializeConnection();
        }
    }
}
