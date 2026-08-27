using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class SmsBL : CrmBaseBL
    {
        public SmsBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleSMSSender(alt_SMS targetSmsEntity)
        {
            this.GlobalContext.LogEntry();
            if (targetSmsEntity.AttributeHasValue<EntityReference>(alt_SMS.Fields.alt_ContactId))
            {
                List<Entity> list = new List<Entity>();
                Entity toEntity = new Entity(ActivityParty.EntityLogicalName);
                toEntity["partyid"] = targetSmsEntity.alt_ContactId;
                list.Add(toEntity);
                EntityCollection entityCollection = new EntityCollection(list);
                targetSmsEntity["to"] = entityCollection;
            }
        }

        public void SetOwner(alt_SMS targetSms)
        {
            this.GlobalContext.LogEntry();

            if (targetSms.AttributeHasValue<EntityReference>(alt_SMS.Fields.RegardingObjectId))
            {
                ActivityBL activityBl = new ActivityBL(this.GlobalContext);
                activityBl.SetOwnerAccordingToCallingUser(targetSms);
            }
        }

        public void HandleSMSAutomaticCreationByTemplateCode(alt_SMS targetSms)
        {
            this.GlobalContext.LogEntry();
            this.SetSmsTemplate(targetSms);

            TemplateCommonBL templateCommonBL = new TemplateCommonBL(this.GlobalContext);
            templateCommonBL.HandleTemplateParsing(targetSms, TemplateType.Sms);
        }

        public void HandleSmsSendStatus(alt_SMS targetSms, alt_SMS preSms)
        {
            this.GlobalContext.LogEntry();
            if (targetSms.AttributeHasValue<OptionSetValue>(alt_SMS.Fields.StatusCode)
                && targetSms.StatusCode.Value == (int)SmsStatusCode.Send)
            {
                alt_SMS mergedSmsEntity = targetSms.Equals(preSms) ?
                    targetSms : targetSms.Merge<alt_SMS>(preSms);

                var actionResult = this.ValidateSmsBeforeSending(mergedSmsEntity);
                if (actionResult.IsSuccess)
                {
                    targetSms.StatusCode = new OptionSetValue((int)SmsStatusCode.SendingNow);
                }
                else
                {
                    this.SetSmsAsCanceled(targetSms, actionResult.Error.Message);
                }
            }
        }

        public void SetSmsTemplate(alt_SMS targetSms)
        {
            this.GlobalContext.LogEntry();
            if (!targetSms.AttributeHasValue<EntityReference>(alt_SMS.Fields.alt_SMSTemplateId)
                && targetSms.AttributeHasValue<int?>(alt_SMS.Fields.alt_TemplateCodeInt))
            {
                SMSTemplateDAL smsTemplateDAL = new SMSTemplateDAL(this.GlobalContext);
                EntityReference smsTemplate = smsTemplateDAL.GetFirstActivetOrDefaultByAttribute<int>(alt_SMSTemplate.Fields.alt_CodeInt, targetSms.alt_TemplateCodeInt.Value, new[] { alt_SMS.Fields.alt_SMSTemplateId })?.ToEntityReference()
                    ?? throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.TemplateNotExist, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.TemplateNotExist));

                targetSms.alt_SMSTemplateId = smsTemplate;
            }
        }

        public ActionResult ValidateSmsBeforeSending(alt_SMS mergedSms)
        {
            this.GlobalContext.LogEntry();
            ActionResult sendResult = new ActionResult();

            ContactBL contactBl = new ContactBL(this.GlobalContext);
            if (!mergedSms.AttributeHasValue<string>(alt_SMS.Fields.alt_MobilePhone))
            {
                this.GlobalContext.Log.Warning(string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "טלפון נייד"));
                sendResult.SetToFailedActionResult(CustomErrorCodes.CommonRequiredFieldMessage, new[] { "טלפון נייד" });
            }
            else if (mergedSms.AttributeHasValue<EntityReference>(alt_SMS.Fields.alt_ContactId)
                && contactBl.IsPassedAway(mergedSms.alt_ContactId.Id))
            {
                sendResult.SetToFailedActionResult(CustomErrorCodes.PassedawayContactSendSmsErrorMessage);
            }

            return sendResult;
        }

        public void SetSmsAsCanceled(alt_SMS targetSms, string errorMessage = null)
        {
            targetSms.StateCode = alt_SMSState.Canceled;
            targetSms.ActualEnd = DateTime.UtcNow;
            targetSms.StatusCode = new OptionSetValue((int)SmsStatusCode.Canceled);
            targetSms.alt_SendResult = errorMessage;
        }

        public void HandleSetSmsMobilePhoneByContact(alt_SMS targetSms)
        {
            this.GlobalContext.LogEntry();
            if (targetSms.AttributeHasValue<EntityReference>(alt_SMS.Fields.alt_ContactId)
                && (!targetSms.AttributeHasValue<string>(alt_SMS.Fields.alt_MobilePhone)))
            {
                ContactDAL contactDAL = new ContactDAL(this.GlobalContext);
                Contact retrievedContact = contactDAL.Get(targetSms.alt_ContactId.Id, new[] { Contact.Fields.MobilePhone });
                targetSms.alt_MobilePhone = retrievedContact.MobilePhone;
            }
        }

        public Guid CreateSms(EntityReference regarding, EntityReference contactId, EntityReference templateId)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateSms(regarding, contactId, templateId);
        }

        public Guid CreateSms(EntityReference regarding, EntityReference contactId, int? templateCode)
        {
            this.GlobalContext.LogEntry();

            return this.HandleMappingAndCreateSms(regarding, contactId, null, templateCode);
        }

        public Guid CreateSms(EntityReference regarding, EntityReference contactId, string mobilePhone, string subject, string description)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateSms(regarding, contactId, null, null, mobilePhone, subject, description);
        }

        public Guid CreateSms(EntityReference regarding, EntityReference contactId, string mobilePhone, int? templateCode)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateSms(regarding, contactId, null, templateCode, mobilePhone);
        }

        public Guid CreateSms(EntityReference regarding, EntityReference contactId, string subject, string description)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateSms(regarding, contactId, null, null, subject, description);
        }

        public void CreateSms(EntityReference regardingObjectId, Recipient recipient, int? smsTemplateCode = null, EntityReference smsTemplateId = null, string parserEntryPoint = null)
        {
            this.GlobalContext.LogEntry(recipient.MobilePhone);
            var contactId = recipient.CustomerId?.LogicalName == Contact.EntityLogicalName ?
                            recipient.CustomerId : null;
            this.HandleMappingAndCreateSms(regardingObjectId, contactId, smsTemplateId, smsTemplateCode, recipient.MobilePhone, null, null, parserEntryPoint);
        }

        private Guid HandleMappingAndCreateSms(EntityReference regarding, EntityReference contactId, EntityReference template = null, int? tmeplateCode = null, string mobilePhone = null, string subject = null, string description = null, string parserEntryPoint = null)
        {
            this.GlobalContext.LogEntry();
            alt_SMS smsToCreate = new alt_SMS
            {
                alt_CreationMethodCode = new OptionSetValue((int)CreationMethodCode.Proccess),
                alt_SMSTemplateId = template,
                alt_TemplateCodeInt = tmeplateCode,
                RegardingObjectId = regarding,
                alt_ContactId = contactId,
                alt_MobilePhone = mobilePhone,
                alt_ParserCustomEntryPoint = parserEntryPoint
            };

            smsToCreate.alt_IsAutomaticBit = (template == null && tmeplateCode == null) ? false : true;

            if (!smsToCreate.alt_IsAutomaticBit.Value)
            {
                smsToCreate.Subject = subject;
                smsToCreate.Description = description;
            }

            ActionResult validationResult = this.ValidateSmsBeforeSending(smsToCreate);
            smsToCreate.StatusCode = validationResult.IsSuccess ?
                new OptionSetValue((int)SmsStatusCode.Send) : new OptionSetValue((int)SmsStatusCode.Canceled);
            smsToCreate.StateCode = validationResult.IsSuccess ?
                alt_SMSState.Open : alt_SMSState.Canceled;
            smsToCreate.alt_SendResult = validationResult.IsSuccess ?
                validationResult.Error?.ToString() : null;
            SmsDAL smsDAL = new SmsDAL(this.GlobalContext);
            return smsDAL.Create(smsToCreate);
        }
    }
}
