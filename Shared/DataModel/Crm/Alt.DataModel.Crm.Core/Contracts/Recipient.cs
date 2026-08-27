using Microsoft.Xrm.Sdk;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class Recipient
    {
        public EntityReference CustomerId { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
    }
}
