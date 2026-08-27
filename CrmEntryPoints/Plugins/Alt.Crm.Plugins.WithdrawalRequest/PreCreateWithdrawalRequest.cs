using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.WithdrawalRequest
{
    public class PreCreateWithdrawalRequest : PluginBase
    {
        public PreCreateWithdrawalRequest(string unsecure, string secure) : base(typeof(PreCreateWithdrawalRequest)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_WithdrawalRequest targetWithdrawalRequest = localContext.TargetEntity?.ToEntity<alt_WithdrawalRequest>();

            WithdrawalRequestBL withdrawalRequestBL = new WithdrawalRequestBL(localContext.ToGlobal());
            withdrawalRequestBL.SetWithdrawalRequestName(targetWithdrawalRequest);
        }
    }
}
