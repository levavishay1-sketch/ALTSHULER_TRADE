using Alt.DataModel.Crm.Core.Contracts;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Framework.EntryPoints.Crm
{
    public abstract class ExternalServiceActionBase : PluginBase
    {
        public ExternalServiceActionBase(Type childClassName, bool runByCallingUser = true) : base(childClassName, runByCallingUser)
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            GlobalContext globalContext = localContext.ToGlobal();
            globalContext.Log.Info($"Request Content: {globalContext.Content}");
            ActionResult actionResult = new ActionResult();
            try
            {
                actionResult = this.ExecuteCustomApiBusinessLogic(globalContext);
            }
            catch (Exception ex)
            {
                actionResult.SetToFailedActionResult(ex.Message);
                globalContext.Log.Critical(ex.ToString());
            }
            this.HandleBusinessLogicResultResponse(globalContext, localContext.PluginExecutionContext, actionResult);
        }

        protected virtual void HandleBusinessLogicResultResponse(GlobalContext globalContext, IPluginExecutionContext pluginExecutionContext, ActionResult actionResult)
        {
            pluginExecutionContext.OutputParameters["IsSuccess"] = actionResult.IsSuccess;
            if (!actionResult.IsSuccess)
            {
                pluginExecutionContext.OutputParameters["Message"] = actionResult.Error?.ToString();
                globalContext.Log.Error($"Response:{actionResult}");
            }
            else
            {
                pluginExecutionContext.OutputParameters["Message"] = actionResult.ReturnObject;
                globalContext.Log.Info($"Response:{actionResult}");
            }
        }

        protected abstract ActionResult ExecuteCustomApiBusinessLogic(GlobalContext globalContext);
    }
}
