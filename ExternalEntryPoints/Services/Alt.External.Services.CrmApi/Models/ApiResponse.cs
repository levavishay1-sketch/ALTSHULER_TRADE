using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Alt.External.Services.CrmApi.Models
{
    internal class ApiResponse
    {
        private Error error;
        public Error Error { get => error; private set => error = value; }

        private object responseData;
        public object ResponseData { get => responseData; private set => responseData = value; }

        public ApiResponse(int errorCode)
        {
            this.Error = new Error() { Code = errorCode };
            this.ResponseData = null;
        }

        public ApiResponse(ActionResult actionResult)
        {
            this.Initialize(actionResult);
        }

        private void Initialize(ActionResult actionResult)
        {
            this.Error = actionResult.Error != null ?
              new Error() { Code = actionResult.Error.Code } : null;
            this.ResponseData = actionResult.ReturnObject is IEnumerable<ApiEntityBase>
                ? ((IEnumerable<ApiEntityBase>)actionResult.ReturnObject)
                    .Select(a => a.ModifiedProperties).ToList()
                : actionResult.ReturnObject is ApiEntityBase
                    ? ((ApiEntityBase)actionResult.ReturnObject).ModifiedProperties
                    : actionResult.ReturnObject;
        }


        public object Generate()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var result = System.Text.Json.JsonSerializer.Serialize(this, options);
            return JsonConvert.DeserializeObject(result);
        }
    }
}