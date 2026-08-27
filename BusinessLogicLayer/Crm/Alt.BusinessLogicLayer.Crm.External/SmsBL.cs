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

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class SmsBL : ExternalBLBase, ICrmOutgoing<ApiSms>
    {
        private const string useWhiteListVariableName = "alt_UseWhiteList";
        private const string whiteListVariableName = "WhiteList";
        public SmsBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiSms> apiContext)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            if (this.ApiConfiguration != null)
            {
                actionResult = this.SendSmsHandler(apiContext.Target);
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }
            return actionResult;
        }

        private ActionResult SendSmsHandler(ApiSms apiSms)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            SmsDAL smsDal = new SmsDAL(this.GlobalContext);
            ApiSms retrievedSms = smsDal.GetSmsDetails(apiSms.Id.Value);
            if (retrievedSms.StatusCode == (int)SmsStatusCode.SendingNow)
            {
                if (this.IsUseWhiteList() && !this.IsInWhiteList(retrievedSms.MobilePhone))
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.MobilePhoneNotInWhiteList, new string[] { retrievedSms.MobilePhone });
                    this.CancelSmsSending(retrievedSms, actionResult.Error.Message);
                }
                else
                {
                    ESBSmsDAL esbSmsDal = new ESBSmsDAL(this.GlobalContext, this.ApiConfiguration);
                    actionResult = esbSmsDal.ExecuteRequest(retrievedSms);
                    this.HandleSendResult(apiSms, actionResult);
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForSendToExternalService, new string[] { Enum.GetName(typeof(SmsStatusCode), retrievedSms.StatusCode.Value) });
            }
            return actionResult;
        }

        private void HandleSendResult(ApiSms apiSms, ActionResult actionResult)
        {
            this.GlobalContext.LogEntry();

            ApiSms apiSmsToUpdate = new ApiSms { Id = apiSms.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;
            string sendResult = null;
            if (actionResult.IsSuccess)
            {
                var response = base.GetDeserializedContent<ESBSmsResponse>(actionResult.ReturnObject.ToString());
                resultStatus = response.ResultStatusCode;
                sendResult = response.StatusMessage;
                if (resultStatus == null)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { response.StatusCode });
                }
                else if(resultStatus != ESBResultStatusCode.Success)
                {
                    actionResult.SetToFailedActionResult(sendResult);
                }
            }

            apiSmsToUpdate.StatusCode = resultStatus  != null && resultStatus == ESBResultStatusCode.Success
                ? (int)SmsStatusCode.SentSuccessfully : (int)SmsStatusCode.Failed;
            apiSmsToUpdate.StateCode = apiSmsToUpdate.StatusCode.Value == (int)SmsStatusCode.SentSuccessfully ?
                (int)CustomActivityEntityState.Completed : (int)CustomActivityEntityState.Open;
            apiSmsToUpdate.SendResult = !actionResult.IsSuccess && actionResult.Error != null ?
                actionResult.Error.Message : sendResult; ;

            SmsDAL smsDal = new SmsDAL(this.GlobalContext);
            smsDal.Update(apiSmsToUpdate);
        }

        private void CancelSmsSending(ApiSms apiSms, string errorMessage)
        {
            this.GlobalContext.LogEntry();
            SmsDAL smsDAL = new SmsDAL(this.GlobalContext);
            apiSms.StatusCode = (int)SmsStatusCode.Canceled;
            apiSms.StateCode = (int)CustomActivityEntityState.Canceled;
            apiSms.SendResult = errorMessage;

            smsDAL.Update(apiSms);
        }

        private bool IsUseWhiteList()
        {
            this.GlobalContext.LogEntry();

            var result = this.GlobalContext.CacheManager.GetEnvironmentVariable(useWhiteListVariableName);
            return result.ToString() == "yes";
        }

        private bool IsInWhiteList(string phoneNumber)
        {
            this.GlobalContext.LogEntry();

            string recipients = this.GlobalContext.CacheManager.GetGlobalParameter<string>(whiteListVariableName);
            this.GlobalContext.Log.Info($"{whiteListVariableName}: {recipients}");

            bool isInList = false;
            string[] phoneNumbers = recipients.Split(',');
            for (int i = 0; i < phoneNumbers.Length; i++)
            {
                if (phoneNumbers[i].Trim() == phoneNumber.Trim())
                {
                    isInList = true;
                    break;
                }
            }
            return isInList;
        }
    }
}
