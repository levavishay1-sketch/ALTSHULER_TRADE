using System;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPopulationRegistryCustomerVerification : ExternalEntityBase
    {
        public int? Population { get; set; }
        public int? CompanyCode { get; set; }
        public string IdNumber { get; set; }
        public DateTime? TaarichLeda { get; set; }
        public DateTime? IdIssueDate { get; set; }
        public Guid? SystemUserId {get; set;}

    }
}
