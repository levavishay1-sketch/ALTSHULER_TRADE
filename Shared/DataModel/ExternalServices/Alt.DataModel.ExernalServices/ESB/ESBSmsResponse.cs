
using Alt.DataModel.ExernalServices.Enums;
using System;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBSmsResponse
    {
        private string statusCode;
        public string StatusCode
        {
            get => statusCode;
            set
            {
                statusCode = value;
                if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int intValue) 
                    && Enum.IsDefined(typeof(ESBResultStatusCode), intValue))
                {
                    this.ResultStatusCode = (ESBResultStatusCode)intValue;
                }
            }
        }
        public string StatusMessage { get; set; }

        public ESBResultStatusCode? ResultStatusCode { get; private set; }
    }
}
