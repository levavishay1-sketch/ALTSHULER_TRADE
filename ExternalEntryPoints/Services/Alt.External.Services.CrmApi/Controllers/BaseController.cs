using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework.EntryPoints.External;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using Alt.Framework.Azure.ServiceBus;
using Alt.External.Services.CrmApi.Models;

namespace Alt.External.Services.CrmApi.Controllers
{
    public abstract class BaseController : ApiController
    {
        public ThirdPartyBase ThirdPartyBase { get; set; }
        public string DefaultQueueName { get; protected set; }

        protected IHttpActionResult HandleGenerateResponse(ActionResult actionResult)
        {
            ThirdPartyBase.GlobalContext.LogEntry();
            var response = this.GenerateHttpResponseMessage(actionResult);
           // var response = this.GenerateResponseMessage(actionResult);

            return ResponseMessage(response ?? Request.CreateResponse(HttpStatusCode.InternalServerError));
        }

        private HttpResponseMessage GenerateResponseMessage(ActionResult actionResult)
        {
            ThirdPartyBase.GlobalContext.LogEntry();

            HttpResponseMessage response = null;

            if (actionResult.IsSuccess)
            {
                if (this.Request.Method == HttpMethod.Get)
                {
                    if (actionResult.ReturnObject != null)
                    {
                        if (actionResult.ReturnObject is IEnumerable<object>)
                        { // whrap ReturnObject collection with  for security reasons
                            actionResult.ReturnObject = new { DataCollection = actionResult.ReturnObject };
                        }
                        JsonSerializerOptions options = new JsonSerializerOptions()
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };
                        string serializedJsonWithoutNullProperties = System.Text.Json.JsonSerializer.Serialize(actionResult.ReturnObject, options);

                        actionResult.ReturnObject = JsonConvert.DeserializeObject(serializedJsonWithoutNullProperties);
                    }

                    response = (actionResult.ReturnObject == null)
                        ? Request.CreateResponse(HttpStatusCode.OK)
                        : Request.CreateResponse(HttpStatusCode.OK, actionResult.ReturnObject);
                }
                else if (this.Request.Method == HttpMethod.Post)
                {
                    response = Request.CreateResponse(HttpStatusCode.Created, actionResult.ReturnObject);
                }
                else // put, delete ...
                {
                    response = Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            else
            {
                this.ThirdPartyBase.GlobalContext.Log.Warning($"Error Message: {actionResult.Error.Message}, ResponseErrorCode : {actionResult.Error.Code}");
                response = Request.CreateResponse(HttpStatusCode.BadRequest, actionResult.Error.Code);
            }

            return response;
        }

        private HttpResponseMessage GenerateHttpResponseMessage(ActionResult actionResult)
        {
            ThirdPartyBase.GlobalContext.LogEntry();

            object apiResponse = new ApiResponse(actionResult).Generate();
            //object apiResponse = new ApiResponse(actionResult);
            HttpResponseMessage httpResponseMessage;

            if (actionResult.IsSuccess)
            {
                if (this.Request.Method == HttpMethod.Get)
                {
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                }
                else if (this.Request.Method == HttpMethod.Post)
                {
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.Created, apiResponse);
                }
                else
                {
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                }
            }
            else
            {
                this.ThirdPartyBase.GlobalContext.Log.Warning($"Error Message: {actionResult.Error.Message}, ResponseErrorCode : {actionResult.Error.Code}");
                httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest, apiResponse);
            }
            return httpResponseMessage;
        }

        internal HttpResponseMessage RedirectIncomingRequest(ApiEntityBase apiEntity)
        {
            ThirdPartyBase.GlobalContext.LogEntry();

            ServiceBusCustomMessage serviceBusMessage = this.GenerateServiceBusMessage(apiEntity);
            ActionResult actionResult = this.SendMessageToServiceBus(serviceBusMessage);

           // return GenerateResponseMessage(actionResult);
            return this.GenerateHttpResponseMessage(actionResult);
        }

        private ActionResult SendMessageToServiceBus(ServiceBusCustomMessage serviceBusMessage)
        {
            ThirdPartyBase.GlobalContext.LogEntry();

            ServiceBusProducer serviceBusProducer = new ServiceBusProducer(this.DefaultQueueName);
            ActionResult actionResult = serviceBusProducer.SendMessageSync(serviceBusMessage);

            if (actionResult.IsSuccess && this.ActionContext.Request.Method == HttpMethod.Post)
            {
                actionResult.ReturnObject = new { Id = serviceBusMessage.PrimaryEntityId };
            }
            return actionResult;
        }

        private ServiceBusCustomMessage GenerateServiceBusMessage(ApiEntityBase apiEntity)
        {
            ThirdPartyBase.GlobalContext.LogEntry();
            if (this.ActionContext.Request.Method == HttpMethod.Post && apiEntity.Id == null)
            {
                apiEntity.Id = Guid.NewGuid();
            }

            ServiceBusCustomMessage serviceBusMessage = new ServiceBusCustomMessage()
            {
                ActionType = this.ActionContext.Request.Method == HttpMethod.Post ? "create" : "update",
                ApiConfigurationCode = apiEntity.ApiConfigurationCode,
                PrimaryEntityName = apiEntity.LogicalName,
                RequestId = ThirdPartyBase.GlobalContext.RequestId,
                PrimaryEntityId = apiEntity.Id,
                Body = apiEntity.ToString()
            };
            return serviceBusMessage;
        }
    }
}