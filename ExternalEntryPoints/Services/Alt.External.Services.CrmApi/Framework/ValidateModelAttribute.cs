using Alt.DataModel.Crm.Core.Errors;
using Alt.External.Services.CrmApi.Controllers;
using Alt.External.Services.CrmApi.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Alt.External.Services.CrmApi.Framework
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                StringBuilder errorMessages = new StringBuilder();
                errorMessages.AppendLine();

                foreach (var error in actionContext.ModelState.Values)
                {
                    foreach (var innerError in error.Errors)
                    {
                        if (!string.IsNullOrWhiteSpace(innerError.ErrorMessage))
                        {
                            errorMessages.AppendLine($"ErrorMessage : {innerError.ErrorMessage}");
                        }
                        if (innerError.Exception != null)
                        {
                            errorMessages.AppendLine($"Exception:{innerError.Exception}");
                        }
                    }
                }

                ExternalEntryPointManager.Connect(actionContext);
                ExternalEntryPointManager.LogRequest(actionContext);

                errorMessages.AppendLine($"ResponseErrorCode : { CustomErrorCodes.InvalidApiInput}");

                (actionContext.ControllerContext.Controller as BaseController).ThirdPartyBase?.GlobalContext?.Log?.Error(errorMessages.ToString());
                (actionContext.ControllerContext.Controller as BaseController)?.ThirdPartyBase?.Dispose();

                //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, CustomErrorCodes.InvalidApiInput);
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, (new ApiResponse(CustomErrorCodes.InvalidApiInput)).Generate());
            }
        }
    }
}