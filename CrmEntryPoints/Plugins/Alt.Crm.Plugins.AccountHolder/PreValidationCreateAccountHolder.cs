using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class PreValidationCreateAccountHolder : PluginBase
    {
        public PreValidationCreateAccountHolder(string unsecure, string secure) : base(typeof(PreValidationCreateAccountHolder)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();

            AccountHolderBL accountHolderBl = new AccountHolderBL(localContext.ToGlobal());
            accountHolderBl.SetDefaultOwner(targetAccountHolder);
        }
    }
}