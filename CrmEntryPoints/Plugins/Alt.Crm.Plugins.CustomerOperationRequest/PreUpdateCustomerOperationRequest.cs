using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.CustomerOperationRequest
{
    public class PreUpdateCustomerOperationRequest : PluginBase
    {
        public PreUpdateCustomerOperationRequest(string unsecure, string secure)
    : base(typeof(PreUpdateCustomerOperationRequest)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_CustomerOperationRequest targetCustomerOperationRequest = localContext.TargetEntity?.ToEntity<alt_CustomerOperationRequest>();
            CustomerOperationRequestBL customerOperationRequestBL = new CustomerOperationRequestBL(localContext.ToGlobal());
            customerOperationRequestBL.HandleSendRequest(targetCustomerOperationRequest);
        }
    }
}
