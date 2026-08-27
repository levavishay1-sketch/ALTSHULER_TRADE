using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class OTPManagerBL : CrmBaseBL
    {
        private const string otpCodePlaceholder = "@OTPCode@";
        public OTPManagerBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult SendOTP(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();

            OtpDTO dto = new OtpDTO();
            dto.ActivityTempateType = (int)inputParameters["ActivityTemplateType"];
            dto.TemplateCode = (int)inputParameters["TemplateCode"];
            dto.RegardingObjectId = inputParameters["RegardingObjectId"]?.ToString();
            dto.ParserCustomEntryPoint = inputParameters["ParserCustomEntryPoint"]?.ToString();
            dto.ContactId = inputParameters["ContactId"]?.ToString();
            dto.To = inputParameters["To"]?.ToString();
            this.GlobalContext.Log.Info(dto.ToString());

            ActivityTemplateTypeCode templateType = (ActivityTemplateTypeCode)dto.ActivityTempateType;
            switch (templateType)
            {
                case ActivityTemplateTypeCode.Sms:
                    {
                        return this.SendOTPViaSms(dto);
                    }

                case ActivityTemplateTypeCode.Email:
                    {
                        return this.SendOtpViaEmail(dto);
                    }
                default:
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.InvalidTemplateType, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidTemplateType));
            }
        }

        private ActionResult SendOtpViaEmail(OtpDTO dto)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL(this.GlobalContext);
            var emailTemplate = emailTemplateDAL.GetActiveByAttribute(alt_EmailTemplate.Fields.alt_CodeInt, dto.TemplateCode, null).FirstOrDefault();
            if (emailTemplate != null)
            {
                Email emailToCreate = new Email
                {
                    alt_EmailTemplateId = emailTemplate.ToEntityReference(),
                    alt_ParserCustomEntryPoint = dto.ParserCustomEntryPoint,
                    RegardingObjectId = new EntityReference(emailTemplate.alt_SchemaName, new Guid(dto.RegardingObjectId))
                };
                this.SetSenderByEmailTemplate(emailToCreate, emailTemplate);
                this.SetEmailRelated(emailToCreate, dto.ContactId);
                this.SetAddressUsed(emailToCreate, dto.To);

                var entryPoint = this.GetTemplateParserEntryPoint(emailToCreate.RegardingObjectId, dto.ParserCustomEntryPoint);
                int otpCode = this.GenerateOTPCode();

                string templateBody = this.GetParsedMessage(emailTemplate.alt_TemplateBody, entryPoint);
                emailToCreate.Description = templateBody.Replace(otpCodePlaceholder, otpCode.ToString());

                string subject = this.GetParsedMessage(emailTemplate.alt_SubjectTemplate, entryPoint);
                emailToCreate.Subject = subject.Replace(otpCodePlaceholder, otpCode.ToString());

                EmailBL emailBl = new EmailBL(this.GlobalContext);
                emailBl.CreateAndSendEmail(emailToCreate);
                actionResult.ReturnObject = otpCode;
            }
            else
            {
                actionResult.SetToFailedActionResult($"Invalid template code for email ({dto.TemplateCode})");
            }
            return actionResult;
        }

        private ActionResult SendOTPViaSms(OtpDTO dto)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            SMSTemplateDAL smsTemplateDAL = new SMSTemplateDAL(this.GlobalContext);
            var smsTemplate = smsTemplateDAL.GetActiveByAttribute(alt_SMSTemplate.Fields.alt_CodeInt, dto.TemplateCode, null).FirstOrDefault(); ;
            if (smsTemplate != null)
            {
                SmsDAL smsDal = new SmsDAL(this.GlobalContext);
                var smsToCreate = new alt_SMS
                {
                    alt_SMSTemplateId = smsTemplate.ToEntityReference(),
                    RegardingObjectId = new EntityReference(smsTemplate.alt_SchemaName, new Guid(dto.RegardingObjectId)),
                    alt_MobilePhone = dto.To,
                    alt_ParserCustomEntryPoint = dto.ParserCustomEntryPoint,
                    alt_ContactId = new EntityReference(Contact.EntityLogicalName, new Guid(dto.ContactId)),
                    StatusCode = new OptionSetValue((int)SmsStatusCode.Send)
                };
                var entryPoint = this.GetTemplateParserEntryPoint(smsToCreate.RegardingObjectId, dto.ParserCustomEntryPoint);
                int otpCode = this.GenerateOTPCode();

                string templateBody = this.GetParsedMessage(smsTemplate.alt_TemplateBody, entryPoint);
                smsToCreate.Description = templateBody.Replace(otpCodePlaceholder, otpCode.ToString());

                string subject = this.GetParsedMessage(smsTemplate.alt_SubjectTemplate, entryPoint);
                smsToCreate.Subject = subject.Replace(otpCodePlaceholder, otpCode.ToString());
                smsDal.Create(smsToCreate);
                actionResult.ReturnObject = otpCode;
            }
            else
            {
                actionResult.SetToFailedActionResult($"Invalid template code for sms ({dto.TemplateCode})");
            }
            return actionResult;
        }

        private string GetParsedMessage(string message, EntityReference entryPoint)
        {
            this.GlobalContext.LogEntry();

            ParseActivityMessageDAL parseActivityMessageDAL = new ParseActivityMessageDAL(this.GlobalContext);
            if (!string.IsNullOrWhiteSpace(message))
            {
                Parser parser = new Parser(new ParserSettings()
                {
                    RegardingObjectId = entryPoint.Id.ToString(),
                    RegardingObjectEntityLogicalName = entryPoint.LogicalName,
                    MessageToParse = message,
                    EntityValueResolver = null
                });

                return parser.GetParsedMessage(parseActivityMessageDAL.ExecuteQuery<Entity>);
            }
            else
            {
                return string.Empty;
            }
        }

        private EntityReference GetTemplateParserEntryPoint(EntityReference regarding, string parserCustomEntryPoint)
        {
            this.GlobalContext.LogEntry();
            EntityReference parserCustomEntryPointReference = null;
            if (!string.IsNullOrEmpty(parserCustomEntryPoint))
            {
                CustomEntityReference customEntityReference = JsonSerializer.Deserialize<CustomEntityReference>(parserCustomEntryPoint);
                parserCustomEntryPointReference = new EntityReference(customEntityReference.LogicalName, customEntityReference.Id);
            }
            return parserCustomEntryPointReference ?? regarding;
        }

        private int GenerateOTPCode()
        {
            this.GlobalContext.LogEntry();
            int otpCodeLength = this.GlobalContext.CacheManager.GetGlobalParameter<int>("OTPCodeLength");
            int minNumber = int.Parse(string.Empty.PadRight(otpCodeLength, '1'));
            int maxNumber = int.Parse(string.Empty.PadRight(otpCodeLength, '9'));
            Random rnd = new Random();
            int otpCode = rnd.Next(minNumber, maxNumber);
            this.GlobalContext.Log.Info($"OTP Code: {otpCode}");
            return otpCode;
        }

        private void SetSenderByEmailTemplate(Email emailToCreate, alt_EmailTemplate emailTemplate)
        {
            this.GlobalContext.LogEntry();
            List<EntityReference> from = new List<EntityReference>();

            SendFromCode sendFromCode = (SendFromCode)emailTemplate.alt_SendFromCode.Value;

            switch (sendFromCode)
            {
                case SendFromCode.Queue:
                    {
                        from.Add(emailTemplate.alt_FromQueueId);
                        break;
                    }
                case SendFromCode.DefaultTeam:
                    {
                        TeamDAL teamDal = new TeamDAL(this.GlobalContext);
                        Team retrievedTeam = teamDal.Get(emailTemplate.alt_FromTeamId.Id, new[] { Team.Fields.QueueId });
                        from.Add(retrievedTeam.QueueId);
                        break;
                    }
                case SendFromCode.User:
                    {
                        Guid currentUserId = this.GlobalContext.InitiatingUserId;
                        from.Add(new EntityReference(SystemUser.EntityLogicalName, currentUserId));
                        break;
                    }
                default:
                    {
                        from = null;
                        break;
                    }
            }
            if (from != null)
            {
                emailToCreate[Email.Fields.From] = from?.ConvertEntityReferenceToActivityPartyEntityCollection();
            }
        }

        private void SetEmailRelated(Email emailToCreate, string contactId)
        {
            this.GlobalContext.LogEntry();

            EntityCollection related = new EntityCollection();
            Entity activityParty = new Entity(ActivityParty.EntityLogicalName);
            activityParty.Attributes.Add(ActivityParty.Fields.PartyId, new EntityReference(Contact.EntityLogicalName, new Guid(contactId)));
            related.Entities.Add(activityParty);
            emailToCreate[Email.Fields.related] = related;
        }

        private void SetAddressUsed(Email emailToCreate, string emailAddress)
        {
            this.GlobalContext.LogEntry();
            List<ActivityParty> to = new List<ActivityParty>();
            ActivityParty activityParty = new ActivityParty();
            activityParty["addressused"] = emailAddress;
            to.Add(activityParty);
            emailToCreate.To = to;
        }

    }
}
