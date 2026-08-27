using Alt.BusinessLogicLayer.Crm.External.DigitalJoiningForms;
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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class DigitalFormBL : ExternalBLBase, ICrmOutgoing<ApiDigitalForm>
    {

        public DigitalFormBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult CreateDigitalForm(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            DigitalFormDAL digitalFormDAL = new DigitalFormDAL(this.GlobalContext);
            var retrievedDigitalForm = digitalFormDAL.GetActiveByAttribute("alt_digitalformidentitynumber", apiDigitalForm.DigitalFormIdentityNumber, new[] { "activityid" })?.FirstOrDefault();
            if (retrievedDigitalForm == null)
            {
                DigitalJoiningFormBaseBL joiningFormBl = DigitalJoiningFormBaseBL.JoiningFormFactory(apiDigitalForm.DigitalFormType, this.GlobalContext, this.ApiConfiguration);
                if (joiningFormBl != null)
                {
                    joiningFormBl.HandleDefaultOwner(apiDigitalForm);
                }
                var id = digitalFormDAL.Create(apiDigitalForm);
                actionResult.ReturnObject = new ApiDigitalForm() { Id = id };
            }
            else
            {
                actionResult.SetToFailedActionResult(CrmErrorCodes.DuplicateRecordEntityKey);
                actionResult.ReturnObject = new ApiDigitalForm() { Id = retrievedDigitalForm.Id };
            }
            return actionResult;
        }

        public ActionResult UpdateDigitalForm(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();

            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            digitalFormDal.Update(apiDigitalForm);
            return new ActionResult();
        }

        public ActionResult HandleDigitalFormPost(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            return ExecuteIncomingLogicHandler(apiDigitalForm);
        }

        public ActionResult HandleDigitalFormPut(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            return ExecuteIncomingLogicHandler(apiDigitalForm);
        }

        public ActionResult ExecuteIncomingLogicHandler(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            this.SetApiConfiguration(apiDigitalForm);

            return this.ApiConfiguration != null
                  && this.ApiConfiguration.RequestProcessingTypeCode != null
                  && this.ApiConfiguration.RequestProcessingTypeCode == (int)RequestProcessingTypeCode.AsyncViaCrm ?
                    this.ExecuteRequestForProcessingAsynchronously(apiDigitalForm)
                    : this.ExecuteIncomingRequestLogic(apiDigitalForm);
        }

        private ActionResult ExecuteIncomingRequestLogic(ApiDigitalForm apiDigitalForm, bool isRedirected = false)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult;

            this.SetApiConfiguration(apiDigitalForm);
            bool isAsynchronousRequest = this.ApiConfiguration.RequestProcessingTypeCode == (int)RequestProcessingTypeCode.Async;
            DigitalJoiningFormBaseBL joiningFormBl = DigitalJoiningFormBaseBL.JoiningFormFactory(apiDigitalForm.DigitalFormType, this.GlobalContext, this.ApiConfiguration);
            if (joiningFormBl != null)
            {
                actionResult = ApiConfiguration.MethodCode == (int)HttpMethodCode.POST
                    && (!isRedirected || isAsynchronousRequest) ?
                           joiningFormBl.HandleJoiningFormCreate(apiDigitalForm)
                           : joiningFormBl.HandleJoiningFormUpdate(apiDigitalForm);
            }
            else
            {
                actionResult = ApiConfiguration.MethodCode == (int)HttpMethodCode.POST
                     && (!isRedirected || isAsynchronousRequest) ?
                            this.CreateDigitalForm(apiDigitalForm)
                            : this.UpdateDigitalForm(apiDigitalForm);
            }
            return actionResult;
        }

        private ActionResult ExecuteRequestForProcessingAsynchronously(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult;
            DigitalJoiningFormBaseBL joiningFormBl = DigitalJoiningFormBaseBL.JoiningFormFactory(apiDigitalForm.DigitalFormType, this.GlobalContext, this.ApiConfiguration);
            if (joiningFormBl != null)
            {
                joiningFormBl.SetJoiningForm(apiDigitalForm);
                if (joiningFormBl.JoiningForm != null)
                {
                    actionResult = joiningFormBl.ValidateJoiningForm();
                    if (!actionResult.IsSuccess)
                    {
                        return actionResult;
                    }
                }
            }
            ApiDigitalForm digitalForm = this.GenerateDigitalFormForProcessingAsynchronously(apiDigitalForm);
            actionResult = this.ApiConfiguration.MethodCode == (int)HttpMethodCode.POST ?
                 this.CreateDigitalForm(digitalForm)
                 : this.UpdateDigitalForm(digitalForm);
            return actionResult;
        }

        private ApiDigitalForm GenerateDigitalFormForProcessingAsynchronously(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();

            ApiDigitalForm digitalForm = this.ApiConfiguration.MethodCode == (int)HttpMethodCode.POST ?
                apiDigitalForm : new ApiDigitalForm { Id = apiDigitalForm.Id, ApiConfigurationCode = apiDigitalForm.ApiConfigurationCode };

            // Add request content
            digitalForm.DigitalFormDetails = apiDigitalForm.ToString();
            // Remove digital form status for restrict running Synchronous Post Operation plugins
            if (apiDigitalForm.DigitalFormStatus != null)
            {
                if (digitalForm.ModifiedProperties.TryRemove(nameof(digitalForm.DigitalFormStatus), out object value))
                {
                    digitalForm.DigitalFormStatus = null;
                }
            }
            return digitalForm;
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiDigitalForm> apiContext)
        {
            this.GlobalContext.LogEntry();

            return this.ApiConfiguration != null && this.ApiConfiguration.ApiTypeCode == (int)ApiTypeCode.Outgoing ?
                this.ExecuteOutgoingLogic(apiContext) :
                this.ExecuteRedirectedIncomingRequest(apiContext);
        }

        private ActionResult ExecuteRedirectedIncomingRequest(ApiContext<ApiDigitalForm> apiContext)
        {
            this.GlobalContext.LogEntry();

            ApiDigitalForm apiDigitalForm = this.GetDigitalFormDitails(apiContext);
            return this.ExecuteIncomingRequestLogic(apiDigitalForm, true);
        }

        private ActionResult ExecuteOutgoingLogic(ApiContext<ApiDigitalForm> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult;

            switch (this.ApiConfiguration.Code)
            {
                case (int)ApiConfigurationCode.CreateDigitalFormInOutSystem:
                    {
                        actionResult = this.CreateJoiningFormInOutsystem(apiContext.Target);
                        break;
                    }
                default:
                    throw new NotImplementedException($"Not Emplemented Logic for Api Configuration Code {this.ApiConfiguration.Code}");
            }

            return actionResult;
        }

        private ActionResult CreateJoiningFormInOutsystem(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            ApiDigitalForm retrievedDigitalForm = digitalFormDal.Get(apiDigitalForm.Id.Value, new[] { "alt_transfertooutsystemstatuscode", "regardingobjectid", "alt_digitalformidentitynumber", "modifiedby", "alt_digitalformtypecode" });

            if ((TransferStatusCode)retrievedDigitalForm.TransferToOutSystemStatusCode == TransferStatusCode.Sending)
            {
                ESBDigitalFormDAL digitalFormDAL = new ESBDigitalFormDAL(this.GlobalContext, this.ApiConfiguration);
                this.GetRegardingObjectDetails(retrievedDigitalForm);
                actionResult = digitalFormDAL.ExecuteRequest(retrievedDigitalForm);
                this.HandleCreateDigitalFormInOutsystemResult(actionResult, apiDigitalForm);
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForSendToExternalService, new[] { ((DigitalFormStatusCode)retrievedDigitalForm.StatusCode).ToString() });
            }
            return actionResult;
        }

        private void HandleCreateDigitalFormInOutsystemResult(ActionResult actionResult, ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ApiDigitalForm digitalFormToUpdate = new ApiDigitalForm() { Id = apiDigitalForm.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;
            string errorMessage;
            if (actionResult.IsSuccess)
            {
                var digitalFormResponse = JsonSerializer.Deserialize<ESBResponse<ESBDigitalFormResponse>>(actionResult.ReturnObject.ToString());
                errorMessage = digitalFormResponse.ErrorMessage;
                resultStatus = digitalFormResponse.ResultStatusCode;
                if (resultStatus == null)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { digitalFormResponse.ErrorCode?.ToString() });
                }
                else if (resultStatus == ESBResultStatusCode.Success)
                {
                    digitalFormToUpdate.DigitalFormLink = digitalFormResponse.ResponseData.URL;
                }
                else
                {
                    actionResult.SetToFailedActionResult(errorMessage);
                }
            }
            else
            {
                errorMessage = actionResult.Error?.Message;
            }
            digitalFormToUpdate.TransferToOutSystemStatusCode = actionResult.IsSuccess
                && resultStatus != ESBResultStatusCode.Error ?
                (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;
            digitalFormToUpdate.TransferToOutSystemErrorDescription = errorMessage;

            DigitalFormDAL digitalFormDAL = new DigitalFormDAL(this.GlobalContext);
            digitalFormDAL.Update(digitalFormToUpdate);
        }

        private ApiDigitalForm GetDigitalFormDitails(ApiContext<ApiDigitalForm> apiContext)
        {
            this.GlobalContext.LogEntry();
            ApiDigitalForm apiDigitalForm;
            if (!apiContext.IsContextContainsTarget)
            {
                DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
                apiDigitalForm = digitalFormDal.Get(apiContext.Target.Id.Value, null);
            }
            else
            {
                apiDigitalForm = apiContext.MergedTarget;
            }
            return apiDigitalForm;
        }

        private void GetRegardingObjectDetails(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (apiDigitalForm.RegardingObject != null)
            {
                switch (apiDigitalForm.RegardingObject.LogicalName)
                {
                    case ApiLead.EntityLogicalName:
                        {
                            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                            apiDigitalForm.RegardingObject = leadDal.Get(apiDigitalForm.RegardingObject.Id.Value, new[] { "mobilephone", "alt_leadidentitynumber" });
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void SetApiConfiguration(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (this.ApiConfiguration == null)
            {
                int? code = null;
                if (apiDigitalForm.ApiConfigurationCode != null)
                {
                    code = apiDigitalForm.ApiConfigurationCode;
                }
                else if (!string.IsNullOrWhiteSpace(apiDigitalForm.DigitalFormDetails))
                {
                    code = (this.GetDeserializedContent<ApiDigitalForm>(apiDigitalForm.DigitalFormDetails)).ApiConfigurationCode;
                    this.GlobalContext.Log.Info($"Code: {code}");
                }
                base.GetAndSetApiConfiguration(code);
            }
        }
    }
}
