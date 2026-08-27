using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class AsyncCreateAccountHolder : PluginBase
    {
        public AsyncCreateAccountHolder(string unsecure, string secure) : base(typeof(AsyncCreateAccountHolder)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();

            AccountHolderBL accountHolderBl = new AccountHolderBL(localContext.ToGlobal());           
            accountHolderBl.UpdateDigitalFormVerification(targetAccountHolder, targetAccountHolder);
            accountHolderBl.UpdateSpouseAccountHolder(targetAccountHolder);
            accountHolderBl.HandleAutomaticMailing(targetAccountHolder, targetAccountHolder);
            accountHolderBl.RelateBeneficiaryToOwnerAccountByDigitalFormVerification(targetAccountHolder);
            accountHolderBl.HandleCustomerOperationRequests(targetAccountHolder);
        }
    }
}