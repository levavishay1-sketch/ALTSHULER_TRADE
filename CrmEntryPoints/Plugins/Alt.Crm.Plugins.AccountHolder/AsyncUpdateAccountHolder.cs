using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class AsyncUpdateAccountHolder : PluginBase
    {
        public AsyncUpdateAccountHolder(string unsecure, string secure) : base(typeof(AsyncUpdateAccountHolder)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();
            alt_AccountHolder preAccountHolder = localContext.PreEntity?.ToEntity<alt_AccountHolder>();

            AccountHolderBL accountHolderBl = new AccountHolderBL(localContext.ToGlobal());
            accountHolderBl.UpdateDigitalFormVerification(targetAccountHolder, preAccountHolder);
            accountHolderBl.HandleCustomerOperationRequests(targetAccountHolder, preAccountHolder);
            accountHolderBl.HandleBeneficiarySpouseAccountHolderOnBeneficiaryStateChange(targetAccountHolder, preAccountHolder);
            accountHolderBl.CancelKYCOnAccountHolderInactive(targetAccountHolder);
            accountHolderBl.HandleTradingCourseMailing(targetAccountHolder, preAccountHolder);
        }
    }
}