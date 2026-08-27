using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework.EntryPoints.Crm;
using System;

namespace Alt.Crm.Actions.OTPManager
{
    public class OTPManager : PluginBase
    {

        public OTPManager(string unsecure, string secure)
     : base(typeof(OTPManager)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                OTPManagerBL otpManagerBl = new OTPManagerBL(localContext.ToGlobal());
                actionResult = otpManagerBl.SendOTP(localContext.PluginExecutionContext.InputParameters);
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
            }
        }
    }
}

