using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class CustomerOperationRequestBL : CrmBaseBL
    {
        public CustomerOperationRequestBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetAttributesValueByCustomerOperationTemplate(alt_CustomerOperationRequest targetCustomerOperationRequest)
        {
            this.GlobalContext.LogEntry();

            if (targetCustomerOperationRequest.AttributeHasValue<int?>(alt_CustomerOperationRequest.Fields.alt_CustomerOperationTemplateCodeInt)
                || targetCustomerOperationRequest.AttributeHasValue<EntityReference>(alt_CustomerOperationRequest.Fields.alt_CustomerOperationTemplateId))
            {
                CommonDAL commonDal = new CommonDAL(this.GlobalContext, alt_CustomerOperationTemplate.EntityLogicalName);
                string[] attributesToRetrieve =
                {
                    alt_CustomerOperationTemplate.Fields.alt_Name,
                    alt_CustomerOperationTemplate.Fields.alt_CustomerOperationTemplateId,
                    alt_CustomerOperationTemplate.Fields.alt_ApiConfigurationId,
                    alt_CustomerOperationTemplate.Fields.alt_CodeInt
                };
                alt_CustomerOperationTemplate customerOperationTemplate = targetCustomerOperationRequest.alt_CustomerOperationTemplateCodeInt != null ?
                    commonDal.GetActiveByAttribute(alt_CustomerOperationTemplate.Fields.alt_CodeInt, targetCustomerOperationRequest.alt_CustomerOperationTemplateCodeInt.Value, attributesToRetrieve)
                    .FirstOrDefault()?.ToEntity<alt_CustomerOperationTemplate>()
                    : commonDal.Get(targetCustomerOperationRequest.alt_CustomerOperationTemplateId.Id, attributesToRetrieve)
                    .ToEntity<alt_CustomerOperationTemplate>();
                if (customerOperationTemplate != null)
                {
                    targetCustomerOperationRequest.alt_Name = customerOperationTemplate.alt_Name;
                    if (targetCustomerOperationRequest.alt_CustomerOperationTemplateId == null)
                    {
                        targetCustomerOperationRequest.alt_CustomerOperationTemplateId = customerOperationTemplate.ToEntityReference();
                    }
                    if (targetCustomerOperationRequest.alt_CustomerOperationTemplateCodeInt == null)
                    {
                        targetCustomerOperationRequest.alt_CustomerOperationTemplateCodeInt = customerOperationTemplate.alt_CodeInt;
                    }
                }
                else
                {
                    this.GlobalContext.Log.Error(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CustomerOperationTemplateNotFound));
                }
            }
            else
            {
                throw new InvalidPluginExecutionException(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.NotAllRequiredFieldsHaveBeenFilled));
            }
        }

        public void HandleSendRequest(alt_CustomerOperationRequest targetCustomerOperationRequest)
        {
            this.GlobalContext.LogEntry();
            if (targetCustomerOperationRequest.AttributeHasValue<OptionSetValue>(alt_CustomerOperationRequest.Fields.StatusCode)
                && targetCustomerOperationRequest.StatusCode.Value == (int)CustomerOperationRequestStatusCode.Send)
            {
                targetCustomerOperationRequest.StatusCode = new OptionSetValue((int)CustomerOperationRequestStatusCode.Sending);
                targetCustomerOperationRequest.alt_SendResult = null;
            }
        }
    }
}
