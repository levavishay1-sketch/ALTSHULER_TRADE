using System.Text.Json.Serialization;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBGovernmentDataRequest : ExternalEntityBase
    {
        [JsonPropertyName("type")]
        public string GovernmentDataType { get; set; }

        /// <summary>
        /// Date Format "yyyy-MM-dd"
        /// </summary>
        [JsonPropertyName("date")]
        public string FromDate { get; set; }
    }
}
