using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class CustomerOperationRequestBL : ExternalBLBase, ICrmOutgoing<ApiCustomerOperationRequest>
    {
        public CustomerOperationRequestBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiCustomerOperationRequest> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult apiActionResult = new ActionResult();

            CustomerOperationRequestDAL customerOperationRequestDal = new CustomerOperationRequestDAL(this.GlobalContext); ;
            var retrievedCustomerOperationRequest = customerOperationRequestDal.Get(apiContext.Target.Id.Value, new string[] { "statuscode" });
            if (retrievedCustomerOperationRequest.StatusCode.Value == (int)CustomerOperationRequestStatusCode.Sending)
            {
                try
                {
                    apiActionResult = this.ExecuteOperationRequest(apiContext);
                }
                catch (Exception ex)
                {
                    this.GlobalContext.Log.Critical(ex);
                    apiActionResult.SetToFailedActionResult(ex.Message);
                }
                finally
                {
                    this.CompliteOperationRequest(apiContext.MergedTarget, apiActionResult);
                }
            }
            else
            {
                apiActionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForRunningScheduledOperation,
                           new string[] { Enum.GetName(typeof(CustomerOperationRequestStatusCode), retrievedCustomerOperationRequest.StatusCode.Value) });
            }
            return apiActionResult;
        }

        private ActionResult ExecuteOperationRequest(ApiContext<ApiCustomerOperationRequest> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult apiActionResult;
            var related = apiContext.MergedTarget.RelatedRecordId;
            switch (related?.LogicalName)
            {
                case ApiAccountHolder.EntityLogicalName:
                    {
                        AccountHolderBL accountHolderBl = new AccountHolderBL(this.GlobalContext);
                        apiActionResult = accountHolderBl.HandleCustomerOperationRequest(apiContext);
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException($"Not Imlemented Customer Operation Request Logic for Entity ({related.LogicalName})");
                    }

            }
            return apiActionResult;
        }

        private void CompliteOperationRequest(ApiCustomerOperationRequest customerOperationRequest, ActionResult apiActionResult)
        {
            this.GlobalContext.LogEntry();

            CustomerOperationRequestDAL customerOperationRequestDal = new CustomerOperationRequestDAL(this.GlobalContext); ;
            ApiCustomerOperationRequest apiCustomerOperationRequestToUpdate = new ApiCustomerOperationRequest() { Id = customerOperationRequest.Id };

            if (apiActionResult.IsSuccess
                && customerOperationRequest.DeleteIfSuccessfulBit.HasValue
                && customerOperationRequest.DeleteIfSuccessfulBit.Value)
            {
                customerOperationRequestDal.Delete(apiCustomerOperationRequestToUpdate.Id.Value);
            }
            else
            {
                apiCustomerOperationRequestToUpdate.StateCode = apiActionResult.IsSuccess
                    ? (int)CustomStateCode.Inactive : (int)CustomStateCode.Active;
                apiCustomerOperationRequestToUpdate.StatusCode = apiActionResult.IsSuccess
                    ? (int)CustomerOperationRequestStatusCode.SentSuccessful : (int)CustomerOperationRequestStatusCode.Fail;
                apiCustomerOperationRequestToUpdate.SendResult = this.GenerateSendResult(apiActionResult, customerOperationRequestDal);

                customerOperationRequestDal.Update(apiCustomerOperationRequestToUpdate);
            }
        }

        private string GenerateSendResult(ActionResult apiActionResult, CustomerOperationRequestDAL customerOperationRequestDal)
        {
            this.GlobalContext.LogEntry();

            string sendResult = null;
            if (!apiActionResult.IsSuccess)
            {
                sendResult = apiActionResult.Error?.Message;
            }
            else if (this.ApiConfiguration?.DebugMode != null
                && this.ApiConfiguration.DebugMode.Value)
            {
                sendResult = CustomErrorCodes.GetErrorMessage(CustomErrorCodes.ApiIsInDebugMode);
            }
            else
            {
                sendResult = apiActionResult.ReturnObject?.ToString();
            }
            return sendResult?.SubstringByLength(customerOperationRequestDal.GetSendResultAttributeMaxLength());
        }
    }
}
