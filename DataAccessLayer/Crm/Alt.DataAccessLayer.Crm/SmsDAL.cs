using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.DataAccessLayer.Crm
{
    public class SmsDAL : CrmBaseDAL<alt_SMS>
    {
        public SmsDAL(GlobalContext globalContext) : base(globalContext, alt_SMS.EntityLogicalName) { }

        public void CallSmsOutgoingCustomAPI(Guid id)
        {
            OrganizationRequest request = new OrganizationRequest("alt_SmsOutgoingAPI")
            {
                ["Content"] = $"{{\"Id\": \"{id}\"}} ",
                ["ApiConfigurationCode"] = 2
            };
            base.Execute(request);
        }
    }
}
