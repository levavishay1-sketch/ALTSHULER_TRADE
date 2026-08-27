using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.PopulationRegistryCustomerVerification
{
    public class PreValidationCreatePopulateionRegistryCustomerVerification : PluginBase
    {
        public PreValidationCreatePopulateionRegistryCustomerVerification(string unsecure, string secure)
          : base(typeof(PreValidationCreatePopulateionRegistryCustomerVerification)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetPopulationRegistryCustomerVerification = localContext.TargetEntity?.ToEntity<alt_PopulationRegistryCustomerVerification>();

            PopulationRegistryCustomerVerificationBL populationRegistryCustomerVerificationBl = new PopulationRegistryCustomerVerificationBL(localContext.ToGlobal());
            populationRegistryCustomerVerificationBl.Validate(targetPopulationRegistryCustomerVerification);
        }
    }
}
