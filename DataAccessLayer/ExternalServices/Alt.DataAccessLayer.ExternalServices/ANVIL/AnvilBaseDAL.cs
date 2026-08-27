using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ANVIL;
using Alt.Framework;
using Alt.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Alt.DataAccessLayer.ExternalServices.ANVIL
{
    public abstract class AnvilBaseDAL<T> : ExternalServicesBaseDAL<AnvilGeneralRequest, T> where T : ApiEntity
    {
        protected ApiPDFProductionTemplate pdfTemplate;
        protected string pdfParsedData;


        public AnvilBaseDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration, ApiPDFProductionTemplate pdfTemplate, string pdfParsedData = null)
            : base(globalContext, apiConfiguration)
        {
            this.pdfTemplate = pdfTemplate;
            this.pdfParsedData = pdfParsedData;
        }

        protected override AnvilGeneralRequest MapApiEntityToTargetModel(T apiEntity)
        {
            this.GlobalContext.LogEntry();
            AnvilGeneralRequest anvilGeneralRequest;
            if (!string.IsNullOrWhiteSpace(pdfParsedData))
            {
                anvilGeneralRequest = JsonSerializer.Deserialize<AnvilGeneralRequest>(pdfParsedData);
            }
            else
            {
                anvilGeneralRequest = JsonSerializer.Deserialize<AnvilGeneralRequest>(pdfTemplate.JsonData);
                anvilGeneralRequest.data = this.GeneratePdfData(apiEntity);
            }
            return anvilGeneralRequest;
        }

        protected override void AddRequestHeadersHandler(HttpRequestMessage request)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, string> headers = ApiConfigurationHeaders;
            foreach (var header in headers)
            {
                if (request.Headers.Contains(header.Key))
                {
                    request.Headers.Remove(header.Key);
                }
                request.Headers.Add(header.Key, header.Value);
            }
        }

        protected override ActionResult CreateActionResultByHttpResponseMessage(HttpResponseMessage response)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    base.LogResponse(response, content);
                    actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidResponseError, new[] { content });
                }
                else
                {
                    Stream stream = response.Content.ReadAsStreamAsync().Result;
                    var responseContent = FileUtils.ConvertStreamToBase64(stream);
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidResponseContentError, new[] { responseContent });
                    }
                    else
                    {
                        actionResult.ReturnObject = responseContent;
                    }
                }
            }
            catch (Exception ex)
            {
                actionResult.SetToFailedActionResult(ex.HResult, new string[] { ex.ToString() });
            }

            return actionResult;
        }

        protected override HttpRequestMessage GenerateRequest(HttpMethod httpMethod, AnvilGeneralRequest targetModel)
        {
            this.GlobalContext.LogEntry();

            HttpRequestMessage request = new HttpRequestMessage(httpMethod, $"{this.ApiConfiguration.Url}{pdfTemplate.ExternalKeyName}");

            string content = SerializeRequestContent(targetModel);
            if (!string.IsNullOrWhiteSpace(content))
            {
                request.Content = new StringContent(content, null, requestMimeType);
            }
            this.LogRequest(request, content);      
            this.AddRequestHeadersHandler(request);

            return request;
        }

        protected override string SerializeRequestContent(AnvilGeneralRequest targetModel)
        {
            return JsonSerializer.Serialize(targetModel, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        protected abstract dynamic GeneratePdfData(T apiEntity);
    }
}
