
using System;
using System.Text.Json.Serialization;

namespace Alt.DataModel.Crm.Core.Errors
{
    public class Error
    {
        public int Code { get; set; }

        [JsonIgnore]
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Environment.NewLine}Error Message: {Message}{Environment.NewLine}Error Code: {Code}{Environment.NewLine}";
        }
    }
}
