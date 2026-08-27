using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Account
{
    public class PreCreateAccount : PluginBase
    {
        public PreCreateAccount(string unsecure, string secure): base(typeof(PreCreateAccount)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var targetAccount = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Account>();
            AccountBL accountBL = new AccountBL(localContext.ToGlobal());
            accountBL.SetInternalAccountNumberHandler(targetAccount);
        }
    }
}
