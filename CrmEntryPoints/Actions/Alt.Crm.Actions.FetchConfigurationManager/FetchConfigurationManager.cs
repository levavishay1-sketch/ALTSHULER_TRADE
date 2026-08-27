using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework;
using Alt.Framework.EntryPoints.Crm;
using System;

namespace Alt.Crm.Actions.FetchConfigurationManager
{
    public class FetchConfigurationManager : PluginBase
    {
        public FetchConfigurationManager(string unsecure, string secure) : base(typeof(FetchConfigurationManager)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            ActionResult actionResult = new ActionResult();
            GlobalContext globalContext = localContext.ToGlobal();
            try
            {
                FetchConfigurationManagerBL fetchConfigurationManagerBL = new FetchConfigurationManagerBL(localContext.ToGlobal());
                actionResult = fetchConfigurationManagerBL.FetchRecords(localContext.PluginExecutionContext.InputParameters);
            }
            catch (Exception ex)
            {
                actionResult.SetToFailedActionResult(ex.Message);
                throw;
            }
            finally
            {
                localContext.PluginExecutionContext.OutputParameters["IsSuccess"] = actionResult.IsSuccess;
                localContext.PluginExecutionContext.OutputParameters["ReturnObject"] = actionResult.ReturnObject?.ToString();

                globalContext.Log.Info(actionResult.ReturnObject?.ToString());
            }
        }
    }
}
