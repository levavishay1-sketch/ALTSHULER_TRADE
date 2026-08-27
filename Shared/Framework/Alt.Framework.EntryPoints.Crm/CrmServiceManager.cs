using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;

namespace Alt.Framework.EntryPoints.Crm
{
    internal class CrmServiceManager : ICrmServiceManager
    {
        private IOrganizationService organizationService;
        public CrmServiceManager(IOrganizationService organizationService)
        {
            this.organizationService = organizationService;
        }
        public IOrganizationService GetService()
        {
            return this.organizationService;
        }
    }
}
