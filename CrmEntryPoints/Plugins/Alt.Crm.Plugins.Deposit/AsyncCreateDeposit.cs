using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Deposit
{
    public class AsyncCreateDeposit : PluginBase
    {
        public AsyncCreateDeposit(string unsecure, string secure) : base(typeof(AsyncCreateDeposit)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_Deposit targetDeposit = localContext.TargetEntity?.ToEntity<alt_Deposit>();

            DepositBL depositBL = new DepositBL(localContext.ToGlobal());
            depositBL.HandleRelatedDigitalFromVerificationUpdate(targetDeposit);
        }
    }
}
