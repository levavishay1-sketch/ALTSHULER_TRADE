using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PopulationRegistryCustomerVerification
{
    public class PreCreatePopulationRegistryCustomerVerification : PluginBase
    {
        public PreCreatePopulationRegistryCustomerVerification(string unsecure, string secure)
            : base(typeof(PreCreatePopulationRegistryCustomerVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPopulationRegistryCustomerVerification = localContext.TargetEntity?.ToEntity<alt_PopulationRegistryCustomerVerification>();

            PopulationRegistryCustomerVerificationBL populationRegistryCustomerVerificationBl = new PopulationRegistryCustomerVerificationBL(localContext.ToGlobal());
            populationRegistryCustomerVerificationBl.SetName(targetPopulationRegistryCustomerVerification);
            populationRegistryCustomerVerificationBl.SetContactByIdentityNumber(targetPopulationRegistryCustomerVerification);
            populationRegistryCustomerVerificationBl.HandleTransferStatusCode(targetPopulationRegistryCustomerVerification);
            populationRegistryCustomerVerificationBl.SetCompareDataBit(targetPopulationRegistryCustomerVerification);
        }
    }
}
