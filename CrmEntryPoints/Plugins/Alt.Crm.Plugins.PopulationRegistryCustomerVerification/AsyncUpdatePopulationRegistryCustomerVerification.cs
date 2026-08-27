using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PopulationRegistryCustomerVerification
{
    public class AsyncUpdatePopulationRegistryCustomerVerification : PluginBase
    {
        public AsyncUpdatePopulationRegistryCustomerVerification(string unsecure, string secure)
            : base(typeof(AsyncUpdatePopulationRegistryCustomerVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPopulationRegistryCustomerVerification = localContext.TargetEntity?.ToEntity<alt_PopulationRegistryCustomerVerification>();
            var prePopulationRegistryCustomerVerification = localContext.PreEntity?.ToEntity<alt_PopulationRegistryCustomerVerification>();

            PopulationRegistryCustomerVerificationBL populationRegistryCustomerVerificationBl = new PopulationRegistryCustomerVerificationBL(localContext.ToGlobal());
            populationRegistryCustomerVerificationBl.HandlePopulationRegisterCustomerVerificationAsyncUpdate(targetPopulationRegistryCustomerVerification, prePopulationRegistryCustomerVerification);
        }
    }
}
