using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PopulationRegistryCustomerVerification
{
    public class PreUpdatePopulationRegistryCustomerVerification : PluginBase
    {
        public PreUpdatePopulationRegistryCustomerVerification(string unsecure, string secure)
      : base(typeof(PreUpdatePopulationRegistryCustomerVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPopulationRegistryCustomerVerification = localContext.TargetEntity?.ToEntity<alt_PopulationRegistryCustomerVerification>();

            PopulationRegistryCustomerVerificationBL populationRegistryCustomerVerificationBl = new PopulationRegistryCustomerVerificationBL(localContext.ToGlobal());
            populationRegistryCustomerVerificationBl.HandleTransferStatusCode(targetPopulationRegistryCustomerVerification);
        }
    }
}
