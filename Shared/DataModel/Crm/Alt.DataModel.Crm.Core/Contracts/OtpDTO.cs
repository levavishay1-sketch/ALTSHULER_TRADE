
using System.Text.Json;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class OtpDTO
    {
        public int ActivityTempateType { get; set; }
        public int TemplateCode { get; set; }
        public string ParserCustomEntryPoint { get; set; }
        public string RegardingObjectId { get; set; }
        public string ContactId { get; set; }
        public string To { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
