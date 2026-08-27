using Alt.DataModel.ExernalServices;
using System.Text.Json.Serialization;

namespace Alt.DataModel.ExternalServices.ESB
{
    public class ESBFeezbackLinkResponse : ExternalEntityBase
    {
        private string depositPageId;
        [JsonPropertyName("DepositPageId")]
        public string DepositPageId
        {
            get => depositPageId;
            set
            {
                this.SetProperty(value);
                depositPageId = value;
            }
        }
    }
}
