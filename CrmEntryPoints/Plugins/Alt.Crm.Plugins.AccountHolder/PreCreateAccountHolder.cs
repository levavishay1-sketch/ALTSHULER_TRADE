using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class PreCreateAccountHolder : PluginBase
    {
        public PreCreateAccountHolder(string unsecure, string secure) : base(typeof(PreCreateAccountHolder)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.alt_AccountHolder>();

            AccountHolderBL accountHolderBl = new AccountHolderBL(localContext.ToGlobal());

            accountHolderBl.SetAlternateKey(targetAccountHolder);
            accountHolderBl.SetCustomerByIdentificationNumber(targetAccountHolder);
            accountHolderBl.SetAccountHolderName(targetAccountHolder);              
            accountHolderBl.HandleShouldSendTradeInterfaceBit(targetAccountHolder);
            accountHolderBl.HandleBeneficiarySigningDeclarationCode(targetAccountHolder);
            accountHolderBl.HandleCheckTerrorOrganizationCode(targetAccountHolder, localContext.PluginExecutionContext.Depth);
            accountHolderBl.HandleBeneficiaryDeclarationControlCode(targetAccountHolder);
            accountHolderBl.HandleIdentificationNumberComparison(targetAccountHolder);
        }
    }
}
