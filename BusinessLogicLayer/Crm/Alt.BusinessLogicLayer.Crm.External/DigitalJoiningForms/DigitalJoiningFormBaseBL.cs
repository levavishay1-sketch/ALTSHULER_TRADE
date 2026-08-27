using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm.External.DigitalJoiningForms
{
    public abstract class DigitalJoiningFormBaseBL : ExternalBLBase
    {
        public string DigitalFormComplitedStatus { get; set; }
        public string DefaultOwnerParameterKey { get; set; }
        internal ApiEntity JoiningForm { get; set; }
        public DigitalJoiningFormBaseBL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        public static DigitalJoiningFormBaseBL JoiningFormFactory(int? digitalFormType, GlobalContext globalContext, ApiConfiguration apiConfiguration)
        {
            globalContext.LogEntry();

            DigitalJoiningFormBaseBL joiningFormBl = null;
            if (digitalFormType != null)
            {
                DigitalFormTypeCode digitalFormTypeCode = (DigitalFormTypeCode)digitalFormType.Value;
                switch (digitalFormTypeCode)
                {
                    case DigitalFormTypeCode.TradeJoining:
                        {
                            joiningFormBl = new TradeDigitalJoiningFormBL(globalContext, apiConfiguration);
                            break;
                        }
                    case DigitalFormTypeCode.OperationalIncident:
                        {
                            joiningFormBl = new OperationalIncidentDigitalFormBL(globalContext, apiConfiguration);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }
            return joiningFormBl;
        }

        internal virtual void HandleDefaultOwner(ApiDigitalForm apiDigitalForm) { }

        internal virtual ActionResult HandleJoiningFormCreate(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            HandleDefaultOwner(apiDigitalForm);
            DigitalFormDAL digitalFormDAL = new DigitalFormDAL(this.GlobalContext);
            var retrievedDigitalForm = digitalFormDAL.GetActiveByAttribute("alt_digitalformidentitynumber", apiDigitalForm.DigitalFormIdentityNumber, new[] { "activityid" })?.FirstOrDefault();
            if (retrievedDigitalForm == null)
            {
                if (apiDigitalForm.DigitalFormStatus != null
                    && apiDigitalForm.DigitalFormStatus.Code == this.DigitalFormComplitedStatus)
                {
                    this.SetJoiningForm(apiDigitalForm);
                    actionResult = ValidateJoiningForm();
                    if (actionResult.IsSuccess)
                    {
                        apiDigitalForm.DigitalFormStatus = null;
                       // apiDigitalForm.DigitalFormDetails = apiDigitalForm.ToString();
                        apiDigitalForm.Id = digitalFormDAL.Create(apiDigitalForm);
                        actionResult.ReturnObject = new ApiDigitalForm() { Id = apiDigitalForm.Id };
                        this.HandleJoiningDataReception(apiDigitalForm);
                    }
                }
                else
                {
                    actionResult.ReturnObject = new ApiDigitalForm() { Id = digitalFormDAL.Create(apiDigitalForm) };
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CrmErrorCodes.DuplicateRecordEntityKey);
                actionResult.ReturnObject = new ApiDigitalForm() { Id = retrievedDigitalForm.Id };
            }
            return actionResult;
        }

        internal virtual ActionResult HandleJoiningFormUpdate(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ApiDigitalForm digitalFormToProcess = !string.IsNullOrWhiteSpace(apiDigitalForm.DigitalFormDetails) ?
                this.GetDeserializedContent<ApiDigitalForm>(apiDigitalForm.DigitalFormDetails) : apiDigitalForm;
            if (digitalFormToProcess.Id == null)
            {
                digitalFormToProcess.Id = apiDigitalForm.Id;
            }
            if (digitalFormToProcess.DigitalFormStatus != null
                && digitalFormToProcess.DigitalFormStatus.Code == this.DigitalFormComplitedStatus)
            {
                this.SetJoiningForm(digitalFormToProcess);
                actionResult = ValidateJoiningForm();
                if (actionResult.IsSuccess)
                {
                    actionResult = this.HandleJoiningDataReception(digitalFormToProcess);
                }
            }
            else
            {
                DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
                digitalFormDal.Update(digitalFormToProcess);
            }
            return actionResult;
        }

        internal virtual ActionResult HandleJoiningDataReception(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();

            if (string.IsNullOrWhiteSpace(apiDigitalForm.DigitalFormIdentityNumber))
            {
                DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
                apiDigitalForm.DigitalFormIdentityNumber = digitalFormDal.Get(apiDigitalForm.Id.Value, new string[] { "alt_digitalformidentitynumber" }).DigitalFormIdentityNumber;
            }
            if (apiDigitalForm.RegardingObject == null)
            {
                this.HandleRegardingObject(apiDigitalForm);
            }

            return this.ConstractData(apiDigitalForm);
        }

        internal virtual ActionResult ValidateJoiningForm()
        {
            this.GlobalContext.LogEntry();
            return new ActionResult();
        }

        internal virtual void SetJoiningForm(ApiDigitalForm apiDigitalForm) { }

        internal virtual ActionResult ConstractData(ApiDigitalForm apiDigitalForm, string joiningProcessNumber = null) { throw new NotImplementedException(); }

        internal virtual void HandleRegardingObject(ApiDigitalForm apiDigitalForm) { }
    }
}
