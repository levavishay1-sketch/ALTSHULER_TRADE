using Alt.DataModel.Crm.Core.Errors;
using Alt.External.Services.CrmApi.Controllers;
using Alt.External.Services.CrmApi.Models;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Alt.External.Services.CrmApi.Framework
{
    public class ExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            ExternalEntryPointManager.Connect(context.ActionContext);

            var baseControlloer = context.ActionContext.ControllerContext.Controller as BaseController;
            this.SetResponseByExceptionType(context, baseControlloer.ThirdPartyBase.GlobalContext);
            baseControlloer.ThirdPartyBase.Dispose();
        }

        private void SetResponseByExceptionType(HttpActionExecutedContext context, GlobalContext globalContext)
        {
            HttpResponseMessage response = null;
            if (context.Exception is FaultException<OrganizationServiceFault> faultExeption)
            {
                var errorCode = faultExeption?.Detail?.ErrorCode;
                if (errorCode != null)
                {
                    response = this.GenerateResponseByCrmError(context, errorCode, globalContext, faultExeption);
                }
            }
            else if (context.Exception is NotImplementedException)
            {
                globalContext.Log.Critical(context.Exception);
                response = context.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else if (context.Exception is OperationCanceledException || context.Exception is TaskCanceledException)
            {
                globalContext.Log.Warning(context.Exception);
                response = context.Request.CreateResponse(HttpStatusCode.GatewayTimeout);
            }
            else if (context.Exception is HttpResponseException ex)
            {
                globalContext.Log.Warning($"Error : {ex.Response.ToString()} ");
                response = context.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                globalContext.Log.Critical(context.Exception);
                response = context.Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            context.Response = response ?? context.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        private HttpResponseMessage GenerateResponseByCrmError(HttpActionExecutedContext context, int? errorCode, GlobalContext globalContext, FaultException<OrganizationServiceFault> ex)
        {
            HttpResponseMessage response;
            globalContext.Log.Error($"Error Code : {errorCode}");
            switch (errorCode.Value)
            {
                case CrmErrorCodes.ObjectDoesNotExist:
                case CrmErrorCodes.DuplicateRecordEntityKey:
                case CrmErrorCodes.ContactDoesNotExist:
                case CrmErrorCodes.AccountDoesNotExist:
                case CrmErrorCodes.EntityLoopBeingCreated:
                case CrmErrorCodes.RecordNotFoundByEntityKey:
                case CrmErrorCodes.InvalidArgument:
                    {
                        globalContext.Log.Error(ex);
                        response = CreateResponse(context, HttpStatusCode.BadRequest, errorCode);
                        //response = context.Request.CreateResponse(HttpStatusCode.BadRequest, errorCode);
                        break;
                    }
                case CrmErrorCodes.IsvAborted:
                    {
                        var innerSubError = ex?.Detail?.ErrorDetails?["SubErrorCode"] as Nullable<int>;
                        if (innerSubError != null && CustomErrorCodes.ContainsCode(innerSubError.Value))
                        {
                            globalContext.Log.Warning(ex, message: $"ResponseErrorCode : {innerSubError.Value}");
                            response = CreateResponse(context, HttpStatusCode.BadRequest, errorCode);
                            //response = context.Request.CreateResponse(HttpStatusCode.BadRequest, errorCode);
                        }
                        else
                        {
                            globalContext.Log.Critical(ex);
                            response = CreateResponse(context, HttpStatusCode.InternalServerError);
                            //response = context.Request.CreateResponse(HttpStatusCode.InternalServerError);
                        }
                        break;
                    }
                case CrmErrorCodes.CancelActiveChildCaseFirst:
                case CrmErrorCodes.CloseActiveChildCaseFirst:
                case CrmErrorCodes.CannotUpdateBecauseItIsReadOnly:
                    {
                        globalContext.Log.Error(ex);
                        response = CreateResponse(context, HttpStatusCode.BadRequest, CrmErrorCodes.IsvAborted);
                        //response = context.Request.CreateResponse(HttpStatusCode.BadRequest, CrmErrorCodes.IsvAborted);
                        break;
                    }
                default:
                    {
                        globalContext.Log.Critical(ex);
                        response = CreateResponse(context, HttpStatusCode.InternalServerError);
                        //response = context.Request.CreateResponse(HttpStatusCode.InternalServerError);
                        break;
                    }
            }
            return response;
        }

        private HttpResponseMessage CreateResponse(HttpActionExecutedContext context, HttpStatusCode httpStatusCode, int? errorCode = null)
        {
            return errorCode == null ? context.Request.CreateResponse(httpStatusCode) :
                 context.Request.CreateResponse(httpStatusCode, (new ApiResponse(errorCode.Value)).Generate());
        }
    }
}