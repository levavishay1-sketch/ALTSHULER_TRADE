using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.External.Services.CrmApi.Cache;
using Alt.External.Services.CrmApi.Controllers;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Alt.External.Services.CrmApi.Framework
{
    public class GlobalContextManagerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            ExternalEntryPointManager.Connect(actionContext);
            ExternalEntryPointManager.LogRequest(actionContext);

            HttpParameterDescriptor parameter = actionContext.ActionDescriptor.GetParameters()?.FirstOrDefault();
            if (parameter != null)
            {
                var apiEntity = actionContext.ActionArguments != null && actionContext.ActionArguments.Any() ?
                    actionContext.ActionArguments.First().Value as ApiEntityBase : null;

                var apiConfiguration = ControllersAvailableDataCache.GetApiConfigurationByCode(actionContext, apiEntity?.ApiConfigurationCode);
                if (apiConfiguration != null && apiConfiguration.RequestProcessingTypeCode != null
                    && apiConfiguration.RequestProcessingTypeCode.Value == (int)RequestProcessingTypeCode.Async)
                {
                    var baseController = actionContext.ControllerContext.Controller as BaseController;
                    var response = baseController.RedirectIncomingRequest(apiEntity);
                    actionContext.Response = response;
                    this.OnActionExecuted(new HttpActionExecutedContext(actionContext, null));
                }
                else
                {
                    base.OnActionExecuting(actionContext);
                }
            }
            else
            {
                base.OnActionExecuting(actionContext);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception == null)// dispose if no exception occurred  in action level
            {
                var response = actionExecutedContext.Response.ToString();
                var content = actionExecutedContext?.Response?.Content?.ReadAsStringAsync()?.Result ?? string.Empty;

                string responseDetails = $"\nResponse:\n{response}\nContent:\n{content}";

                var baseController = (actionExecutedContext.ActionContext.ControllerContext.Controller as BaseController);
                baseController.ThirdPartyBase.GlobalContext.Log.Info(responseDetails);
                (actionExecutedContext.ActionContext.ControllerContext.Controller as BaseController)?.ThirdPartyBase?.Dispose();
            }
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}