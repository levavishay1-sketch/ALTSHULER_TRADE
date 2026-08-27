using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class PreValidationUpdateAccountHolder : PluginBase
    {
        public PreValidationUpdateAccountHolder(string unsecure, string secure) : base(typeof(PreValidationUpdateAccountHolder)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();
            alt_AccountHolder preAccountHolder = localContext.PreEntity?.ToEntity<alt_AccountHolder>();

            AccountHolderBL accountHolderBl = new AccountHolderBL(localContext.ToGlobal());
            accountHolderBl.SetStateCodeByStatusCode(targetAccountHolder, preAccountHolder);
            accountHolderBl.HanldeAccountHolderStateCode(targetAccountHolder, preAccountHolder);
        }
    }
}