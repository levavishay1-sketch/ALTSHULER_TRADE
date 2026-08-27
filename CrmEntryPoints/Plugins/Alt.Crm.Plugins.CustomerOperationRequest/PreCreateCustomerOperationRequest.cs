using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Crm.Plugins.CustomerOperationRequest
{
    public class PreCreateCustomerOperationRequest : PluginBase
    {
        public PreCreateCustomerOperationRequest(string unsecure, string secure)
        : base(typeof(PreCreateCustomerOperationRequest)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_CustomerOperationRequest targetCustomerOperationRequest = localContext.TargetEntity?.ToEntity<alt_CustomerOperationRequest>();
            CustomerOperationRequestBL customerOperationRequestBl = new CustomerOperationRequestBL(localContext.ToGlobal());

            customerOperationRequestBl.SetAttributesValueByCustomerOperationTemplate(targetCustomerOperationRequest);
            customerOperationRequestBl.HandleSendRequest(targetCustomerOperationRequest);
        }
    }
}
