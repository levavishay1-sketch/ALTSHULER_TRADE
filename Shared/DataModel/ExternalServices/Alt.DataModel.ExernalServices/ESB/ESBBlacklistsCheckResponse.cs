using System;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBBlacklistsCheckResponse
    {
        public string sessionId { get; set; }
        public int? statusCode { get; set; }
        public DateTime? createdAt { get; set; }
    }
}
