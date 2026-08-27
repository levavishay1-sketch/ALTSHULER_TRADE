using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.ExernalServices;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework;
using Alt.Framework.External.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

namespace Alt.DataAccessLayer.ExternalServices
{
    public abstract class ExternalServicesBaseDAL<TTargetModel, TApiEntity>
          where TTargetModel : ExternalEntityBase
          where TApiEntity : ApiEntityBase
    {
        const string valueToReplace = "!!!This Value Removed from Log based on Api Configuration Development Settings!!!";
        protected string requestMimeType = "application/json";

        public GlobalContext GlobalContext { get; private set; }

        private ApiConfiguration apiConfiguration;
        protected ApiConfiguration ApiConfiguration
        {
            get
            {
                return this.apiConfiguration;
            }
            private set
            {
                this.apiConfiguration = value;
                this.ApiConfigurationHeaders = !string.IsNullOrWhiteSpace(value?.HttpHeaders) ?
                    JsonSerializer.Deserialize<Dictionary<string, string>>(value.HttpHeaders)
                    : null;
            }
        }

        protected Dictionary<string, string> ApiConfigurationHeaders { get; private set; }

        protected RequestOperator RequestOperator { get; private set; }

        public ExternalServicesBaseDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration)
        {
            this.GlobalContext = globalContext;
            this.ApiConfiguration = apiConfiguration;
        }

        protected abstract TTargetModel MapApiEntityToTargetModel(TApiEntity apiEntity);

        public virtual ActionResult ExecuteRequest(TApiEntity entity)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (this.ApiConfiguration.MethodCode != null)
            {
                HttpMethodCode methodCode = (HttpMethodCode)this.ApiConfiguration.MethodCode.Value;
                switch (methodCode)
                {
                    case HttpMethodCode.POST:
                        {
                            actionResult = this.Post(entity);
                            break;
                        }
                    case HttpMethodCode.PUT:
                        {
                            actionResult = this.Put(entity);
                            break;
                        }
                    case HttpMethodCode.GET:
                        {
                            actionResult = this.Get(entity);
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationMethodCodeNotDefind);
            }
            return actionResult;
        }

        public virtual ActionResult Post(TApiEntity entity)
        {
            this.GlobalContext.LogEntry();
            TTargetModel targetModel = this.MapApiEntityToTargetModel(entity);
            return this.BuildAndSendRequestHandler(HttpMethod.Post, targetModel);
        }

        public virtual ActionResult Put(TApiEntity entity)
        {
            this.GlobalContext.LogEntry();
            TTargetModel targetModel = this.MapApiEntityToTargetModel(entity);
            return this.BuildAndSendRequestHandler(HttpMethod.Put, targetModel);
        }

        public virtual ActionResult Get(TApiEntity entity)
        {
            this.GlobalContext.LogEntry();
            TTargetModel targetModel = this.MapApiEntityToTargetModel(entity);
            return this.BuildAndSendRequestHandler(HttpMethod.Get, targetModel);
        }

        public virtual ActionResult Delete(TApiEntity entity)
        {
            this.GlobalContext.LogEntry();
            throw new NotImplementedException();
        }

        private ActionResult SendRequestHandler(HttpMethod httpMethod, TTargetModel targetModel)
        {
            this.GlobalContext.LogEntry();

            ActionResult dalActionResult = new ActionResult();
            HttpRequestMessage request = this.GenerateRequest(httpMethod, targetModel);

            if (!this.ApiConfiguration.DebugMode.Value)
            {
                dalActionResult = this.ApiConfiguration.UseSertificates.Value ?
                    this.SendRequestToExternalServiceWithSertificate(request) : this.SendRequestToExternalService(request);
                this.ExternalEndpointRsponseErrorLogHandler(dalActionResult);
            }
            else
            {
                dalActionResult.ReturnObject = this.GetDebugModeResponse();
                if (dalActionResult.ReturnObject == null)
                {
                    dalActionResult.SetToFailedActionResult(CustomErrorCodes.DebugModeResponseContentError);
                }
            }
            return dalActionResult;
        }

        protected virtual string SerializeRequestContent(TTargetModel targetModel)
        {
            return JsonSerializer.Serialize(targetModel, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        protected virtual HttpRequestMessage GenerateRequest(HttpMethod httpMethod, TTargetModel targetModel)
        {
            this.GlobalContext.LogEntry();

            string url = httpMethod == HttpMethod.Get ?
                $"{this.ApiConfiguration.Url}?{targetModel?.QueryParams}"
                : this.ApiConfiguration.Url;
            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);

            if (!string.IsNullOrWhiteSpace(this.ApiConfiguration.HttpHeaders)
                && ApiConfigurationHeaders.ContainsKey("Content-Type"))
            {
                this.requestMimeType = ApiConfigurationHeaders["Content-Type"];
            }
            if (httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Post)
            {
                string content = SerializeRequestContent(targetModel);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    request.Content = new StringContent(content, Encoding.UTF8, requestMimeType);
                }
                this.LogRequest(request, content);
            }
            else if (httpMethod == HttpMethod.Get)
            {
                this.LogRequest(request);
            }
            else
            {
                throw new Exception(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidHttpRequestMethod));
            }
            this.AddRequestHeadersHandler(request);

            return request;
        }

        protected virtual void AddRequestHeadersHandler(HttpRequestMessage request)
        {
            this.GlobalContext.LogEntry();

            request.Headers.Add("RequestId", this.GlobalContext.RequestId.ToString());
            request.Headers.Add("Token", "A46AC1A9-F108-42A0-B15A-D188E7BF7325");
            request.Headers.Add("Username", "AltTradeTest");
            request.Headers.Add("dataColumnName", "response_status");
            if (!string.IsNullOrWhiteSpace(this.ApiConfiguration.HttpHeaders))
            {
                Dictionary<string, string> headers = ApiConfigurationHeaders;
                if (headers.ContainsKey("Content-Type"))
                {
                    headers.Remove("Content-Type");
                }
                foreach (var header in headers)
                {
                    if (request.Headers.Contains(header.Key))
                    {
                        request.Headers.Remove(header.Key);
                    }
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }

        protected virtual void ExternalEndpointRsponseErrorLogHandler(ActionResult dalActionResult)
        {
        }

        protected virtual ActionResult BuildAndSendRequestHandler(HttpMethod httpMethod, TTargetModel targetModel)
        {
            this.GlobalContext.LogEntry();
            ActionResult dalActionResult = null;

            try
            {
                dalActionResult = this.SendRequestHandler(httpMethod, targetModel);
            }
            catch (ArgumentNullException ex)
            {
                this.GlobalContext.Log.Critical(ex);
            }
            catch (InvalidOperationException ex)
            {
                this.GlobalContext.Log.Critical(ex);
            }
            catch (HttpRequestException ex)
            {
                this.GlobalContext.Log.Critical(ex);
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Critical(ex);
            }

            if (dalActionResult == null)
            {
                dalActionResult = new ActionResult();
                dalActionResult.SetToFailedActionResult(CustomErrorCodes.InternalServerError);
            }

            return dalActionResult;
        }

        public ActionResult SendRequestToExternalService(HttpRequestMessage requestToExecute)
        {
            this.GlobalContext.LogEntry();
            ActionResult dalActionResult = null;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var handler = new WebRequestHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
            };

            handler.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain,
                SslPolicyErrors sslPolicyErrors) =>
            { return true; };

            using (HttpClient client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                var response = client.SendAsync(requestToExecute).Result;
                dalActionResult = this.CreateActionResultByHttpResponseMessage(response);
            }
            return dalActionResult;
        }

        public ActionResult SendRequestToExternalServiceWithSertificate(HttpRequestMessage request)
        {
            this.GlobalContext.LogEntry();
            this.RequestOperator = new RequestOperator(this.GlobalContext);
            return this.RequestOperator.SendRequestToEsb(request);
        }

        protected virtual ActionResult CreateActionResultByHttpResponseMessage(HttpResponseMessage response)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            this.LogResponse(response, responseContent);
            if (!response.IsSuccessStatusCode)
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidResponseError, new[] { responseContent });
            }
            else if (string.IsNullOrWhiteSpace(responseContent))
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidResponseContentError, new[] { responseContent });
            }
            actionResult.ReturnObject = responseContent;
            return actionResult;
        }

        internal void WriteToFile(string content, string header)
        {
            this.GlobalContext.LogEntry();
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "ESBModels.txt"), true))
            {
                outputFile.WriteLine();
                outputFile.WriteLine(header);
                outputFile.WriteLine(content);
            }
        }

        protected virtual void AddRequestHeadersHandler(HttpClient httpClient)
        {
            this.GlobalContext.LogEntry();

            httpClient.DefaultRequestHeaders.Add("RequestId", this.GlobalContext.RequestId.ToString());
            if (!string.IsNullOrWhiteSpace(this.ApiConfiguration.HttpHeaders))
            {
                Dictionary<string, string> headers = ApiConfigurationHeaders;
                headers.Remove("Content-Type");
                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        protected virtual ActionResult PostFile(ByteArrayContent fileContent)
        {
            this.GlobalContext.LogEntry();
            ActionResult dalActionResult = new ActionResult();

            using (var client = new HttpClient())
            {
                this.AddRequestHeadersHandler(client);
                client.DefaultRequestHeaders.Add("DataColumnName", "json");
                client.DefaultRequestHeaders.Add("ContentStorage", "jsondata");

                using (var content = new MultipartFormDataContent())
                {
                    var response = client.PostAsync(this.ApiConfiguration.Url, fileContent).Result;
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    this.LogResponse(response, responseContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        dalActionResult.SetToFailedActionResult(CustomErrorCodes.EsbInvalidResponseError, new[] { responseContent });
                    }
                    dalActionResult.ReturnObject = responseContent;
                }
            }
            return dalActionResult;
        }

        protected virtual object GetDebugModeResponse()
        {
            this.GlobalContext.Log.Warning($"{Environment.NewLine}!!!Api is in DebugMode!!!{Environment.NewLine}");

            string debugModeResponse = null;
            if (this.ApiConfiguration != null
                && this.ApiConfiguration.TryGetSettingsItemValue<string>(nameof(debugModeResponse), out debugModeResponse)
                && debugModeResponse != null)
            {
                LogResponse(null, debugModeResponse);
            }
            return debugModeResponse;
        }

        protected virtual void LogRequest(HttpRequestMessage request, string content = null)
        {
            this.GlobalContext.LogEntry();

            string contentToLog = this.ReplaceContentPropertiesValueDuePropertiesToIgnore(content);
            this.GlobalContext.Log.Info(string.Format("{0}Request: {1}{0}RequestId: {2}{0}CorrelationId: {3}{0}Request Body: {4}",
               Environment.NewLine, request, this.GlobalContext.RequestId, this.GlobalContext.CorrelationId, contentToLog));
        }

        protected void LogResponse(HttpResponseMessage response, string responseContent)
        {
            this.GlobalContext.LogEntry();

            string contentToLog = this.ReplaceContentPropertiesValueDuePropertiesToIgnore(responseContent);
            string logMessage = $"{Environment.NewLine}Response: {response}{Environment.NewLine}Response-Content: {contentToLog}";

            this.GlobalContext.Log.Info(logMessage);
        }

        protected virtual string ReplaceContentPropertiesValueDuePropertiesToIgnore(string content)
        {
            this.GlobalContext.LogEntry();

            string contentToLog = content;
            if (!string.IsNullOrWhiteSpace(content))
            {
                string propertiesToIgnoreInLog;
                if (this.ApiConfiguration != null
                    && this.ApiConfiguration.TryGetSettingsItemValue<string>(nameof(propertiesToIgnoreInLog), out propertiesToIgnoreInLog)
                    && !string.IsNullOrWhiteSpace(propertiesToIgnoreInLog))
                {
                    List<string> properties = JsonSerializer.Deserialize<List<string>>(propertiesToIgnoreInLog);
                    contentToLog = content.ReplaceContentPropertiesValue(properties, valueToReplace);
                }
            }
            return contentToLog;
        }
    }
}
