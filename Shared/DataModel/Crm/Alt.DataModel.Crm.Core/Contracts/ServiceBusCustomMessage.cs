using System;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class ServiceBusCustomMessage
    {
        public int? ApiConfigurationCode { get; set; }
        public string Body { get; set; }
        public string ActionType { get; set; }
        public string PrimaryEntityName { get; set; }
        public Guid? PrimaryEntityId { get; set; }
        public Guid? RequestId { get; set; }
    }
}
