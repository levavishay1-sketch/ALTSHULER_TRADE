using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class PreUpdateAccountHolder : PluginBase
    {
        public PreUpdateAccountHolder(string unsecure, string secure) : base(typeof(PreUpdateAccountHolder)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();
            alt_AccountHolder preAccountHolder = localContext.PreEntity?.ToEntity<alt_AccountHolder>();

            AccountHolderBL accountHolderBl = new AccountHolderBL(localContext.ToGlobal());

            accountHolderBl.SetAlternateKey(targetAccountHolder, preAccountHolder);
            accountHolderBl.SetAccountHolderName(targetAccountHolder, preAccountHolder);
            accountHolderBl.HandleCheckTerrorOrganizationCode(targetAccountHolder, localContext.PluginExecutionContext.Depth);
            accountHolderBl.HandleBeneficiaryDeclarationControlCode(targetAccountHolder);
            accountHolderBl.HandleTradeOneUserNameUpdateFromShenhav(targetAccountHolder, preAccountHolder);
            accountHolderBl.HandleIdentificationNumberComparison(targetAccountHolder, preAccountHolder);
        }
    }
}