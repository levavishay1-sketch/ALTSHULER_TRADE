using Alt.DataModel.ExernalServices.Enums;
using System;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBResponse<T>
    {
        private int? errorCode;
        public int? ErrorCode
        {
            get => errorCode;
            set
            {
                errorCode = value;
                if (Enum.IsDefined(typeof(ESBResultStatusCode), value))
                {
                    this.ResultStatusCode = (ESBResultStatusCode)value;
                }
            }
        }
        public string ErrorMessage { get; set; }
        public T ResponseData { get; set; }

        public ESBResultStatusCode? ResultStatusCode { get; private set; }
    }
}
