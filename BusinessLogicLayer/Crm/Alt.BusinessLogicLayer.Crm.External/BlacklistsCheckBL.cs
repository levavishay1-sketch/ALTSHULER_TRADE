using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class BlacklistsCheckBL : ExternalBLBase, ICrmOutgoing<ApiBlacklistsCheck>
    {
        public BlacklistsCheckBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult Update(ApiBlacklistsCheck apiBlacklistsCheck)
        {
            this.GlobalContext.LogEntry();

            ActionResult apiActionResult = new ActionResult();

            CommonDAL commonBl = new CommonDAL(this.GlobalContext, ApiBlacklistsCheck.EntityLogicalName);
            commonBl.Update(apiBlacklistsCheck);

            return apiActionResult;
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiBlacklistsCheck> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult apiActionResult = new ActionResult();

            BlacklistsCheckDAL blacklistsCheckDal = new BlacklistsCheckDAL(this.GlobalContext);
            var retrievedBlacklistsCheck = blacklistsCheckDal
                .GetByAttribute($"{ApiBlacklistsCheck.EntityLogicalName}id", apiContext.Target.Id.Value, new string[] { "statuscode" })
                .FirstOrDefault();
            if (retrievedBlacklistsCheck != null)
            {
                if (retrievedBlacklistsCheck.StatusCode != null
                && retrievedBlacklistsCheck.StatusCode.Value == (int)BlacklistsCheckStatusCode.Sending)
                {
                    try
                    {
                        apiActionResult = this.CheckAgainstBlacklists(apiContext);
                    }
                    catch (Exception ex)
                    {
                        this.GlobalContext.Log.Critical(ex);
                        apiActionResult.SetToFailedActionResult(ex.Message);
                    }
                    finally
                    {
                        this.ExecuteFinally(apiContext.MergedTarget, apiActionResult);
                    }
                }
                else
                {
                    apiActionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForSendToExternalService,
                               new string[] { Enum.GetName(typeof(BlacklistsCheckStatusCode), retrievedBlacklistsCheck.StatusCode.Value) });
                }
            }
            else
            {
                apiActionResult.SetToFailedActionResult("Target not found.");
            }
            return apiActionResult;
        }

        private ActionResult CheckAgainstBlacklists(ApiContext<ApiBlacklistsCheck> apiContext)
        {
            this.GlobalContext.LogEntry();


            ESBBlacklistsCheckDAL eSBBlacklistsCheckDal = new ESBBlacklistsCheckDAL(this.GlobalContext, this.ApiConfiguration);
            ActionResult apiActionResult = eSBBlacklistsCheckDal.ExecuteRequest(apiContext.Target);

            return apiActionResult;
        }

        private void ExecuteFinally(ApiBlacklistsCheck blacklistsCheck, ActionResult apiActionResult)
        {
            this.GlobalContext.LogEntry();

            CommonDAL commonDal = new CommonDAL(this.GlobalContext, ApiBlacklistsCheck.EntityLogicalName); ;
            ApiBlacklistsCheck blacklistsCheckToUpdate = new ApiBlacklistsCheck() { Id = blacklistsCheck.Id };

            if (apiActionResult.IsSuccess)
            {
                var response = JsonSerializer.Deserialize<ESBResponse<ESBBlacklistsCheckResponse>>(apiActionResult.ReturnObject.ToString());

                ESBResultStatusCode? resultStatus = response.ResultStatusCode;
                if (resultStatus == null)
                {
                    apiActionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { response.ErrorCode?.ToString() });
                }
                else if (resultStatus != ESBResultStatusCode.Success)
                {
                    apiActionResult.SetToFailedActionResult(response.ErrorMessage);
                }
                else
                {
                    string sessionId = this.ApiConfiguration.DebugMode.Value ?
                        blacklistsCheck.Id.ToString() : response.ResponseData?.sessionId;
                    blacklistsCheckToUpdate.ExternalIdentifier = sessionId;
                }
            }
            blacklistsCheckToUpdate.StatusCode = apiActionResult.IsSuccess ?
                    (int)BlacklistsCheckStatusCode.SentRequest : (int)BlacklistsCheckStatusCode.Failed;
            blacklistsCheckToUpdate.FailureDetails = this.GenerateFailureDetails(apiActionResult);
            blacklistsCheckToUpdate.StateCode = apiActionResult.IsSuccess ?
                (int)CustomStateCode.Active : (int)CustomStateCode.Inactive;
            commonDal.Update(blacklistsCheckToUpdate);
        }

        private string GenerateFailureDetails(ActionResult apiActionResult)
        {
            this.GlobalContext.LogEntry();

            string failureDetails = null;
            if (!apiActionResult.IsSuccess)
            {
                failureDetails = apiActionResult.Error?.Message;
            }
            else if (this.ApiConfiguration?.DebugMode != null
                && this.ApiConfiguration.DebugMode.Value)
            {
                failureDetails = CustomErrorCodes.GetErrorMessage(CustomErrorCodes.ApiIsInDebugMode);
            }
            return failureDetails;
        }
    }
}
