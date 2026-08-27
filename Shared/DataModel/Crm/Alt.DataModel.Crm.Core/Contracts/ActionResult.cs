using Alt.DataModel.Crm.Core.Errors;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class ActionResult
    {
        public virtual bool IsSuccess { get; set; } = true;

        public virtual Error Error { get; set; }

        public virtual object ReturnObject { get; set; }

        public ActionResult()
        {
        }

        public ActionResult(int errorCode)
        {
            this.Error = new Error() { Code = errorCode, Message = this.GetErrorMessage(errorCode) };
            this.IsSuccess = false;
        }

        public virtual void SetToFailedActionResult(string customMessage)
        {
            this.SetToFailedActionResult(-1, null, customMessage);
        }

        public virtual void SetToFailedActionResult(int errorCode, string[] stringFormtValues = null, string customMessage = null)
        {
            this.IsSuccess = false;
            if (stringFormtValues != null)
            {
                this.Error = new Error() { Code = errorCode, Message = (customMessage ?? string.Format(this.GetErrorMessage(errorCode), stringFormtValues)) };
            }
            else
            {
                this.Error = new Error() { Code = errorCode, Message = (customMessage ?? this.GetErrorMessage(errorCode)) };
            }
        }

        private string GetErrorMessage(int errorCode)
        {
            return errorCode > -2140000000 ? 
                CustomErrorCodes.GetErrorMessage(errorCode) : CrmErrorCodes.GetErrorMessage(errorCode);
        }

        public override string ToString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
