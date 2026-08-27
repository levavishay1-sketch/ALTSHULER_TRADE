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
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class EmailBL : CrmBaseBL
    {
        public EmailBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleEmailAutomaticCreationByTemplateCode(Email targetEmail)
        {
            this.GlobalContext.LogEntry();

            this.SetEmailTemplate(targetEmail);
            this.SetSenderByEmailTemplate(targetEmail);

            TemplateCommonBL activityCommonBL = new TemplateCommonBL(this.GlobalContext);
            activityCommonBL.HandleTemplateParsing(targetEmail, TemplateType.Email);
        }

        public void SetOwner(Email targetEmail)
        {
            this.GlobalContext.LogEntry();
            if (targetEmail.AttributeHasValue<EntityReference>(Email.Fields.RegardingObjectId))
            {
                ActivityBL activityBl = new ActivityBL(this.GlobalContext);
                activityBl.SetOwnerAccordingToCallingUser(targetEmail);
            }
        }

        public Guid CreateAndSendEmail(Email email)
        {
            return CreateAndSendEmailHandler(email, true);
        }

        public Guid CreateEmail(EntityReference regarding, List<EntityReference> to, EntityReference templateId)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateEmail(regarding, null, to, templateId);
        }

        public Guid CreateEmail(EntityReference regarding, List<EntityReference> to, int templateCode)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateEmail(regarding, null, to, null, templateCode);
        }

        public Guid CreateEmail(EntityReference regarding, EntityReference contactId, EntityReference templateId)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateEmail(regarding, new List<EntityReference> { regarding }, new List<EntityReference> { contactId }, templateId);
        }

        public Guid CreateEmail(EntityReference regarding, EntityReference contactId, int templateCode)
        {
            this.GlobalContext.LogEntry();
            return this.CreateEmail(regarding, new List<EntityReference> { contactId }, templateCode);
        }

        public Guid CreateEmail(EntityReference regarding, EntityCollection to, int templateCode)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateEmail(regarding, null, to, null, templateCode);
        }

        public Guid CreateEmail(EntityReference regarding, EntityCollection to, string subject, string description)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateEmail(regarding, null, to, null, null, false, subject, description);
        }

        public Guid CreateEmail(EntityReference regarding, EntityReference contactId, string subject, string description)
        {
            this.GlobalContext.LogEntry();
            return this.HandleMappingAndCreateEmail(regarding, new List<EntityReference> { regarding }, new List<EntityReference> { contactId }, null, null, false, subject, description);
        }

        public Guid CreateEmail(EntityReference regarding, Recipient recipient, int templateCode, string parserEntryPoint)
        {
            this.GlobalContext.LogEntry();

            return this.CreateEmail(regarding, new List<Recipient>() { recipient }, null, parserEntryPoint, templateCode);
        }

        public Guid CreateEmail(EntityReference regarding, List<Recipient> recipients, EntityReference emailTemplate, string parserEntryPoint, int? templateCode = null)
        {
            this.GlobalContext.LogEntry();
            Email emailToCreate = this.GetInitializedEmail(regarding, null, emailTemplate, templateCode, true);
            emailToCreate.alt_ParserCustomEntryPoint = parserEntryPoint;
            this.SetEmailAddressused(emailToCreate, recipients);
            this.SetEmailRelated(emailToCreate, recipients);

            return this.CreateAndSendEmailHandler(emailToCreate, true);
        }


        public Guid CreateEmail(EntityReference regarding, string emailAddress, int templateCode, EntityCollection related = null, string parserEntryPoint = null)
        {
            this.GlobalContext.LogEntry();

            EntityCollection to = new EntityCollection();
            Entity activityParty = new Entity(ActivityParty.EntityLogicalName);
            activityParty[ActivityParty.Fields.AddressUsed] = emailAddress;
            to.Entities.Add(activityParty);

            return this.HandleMappingAndCreateEmail(regarding, null, to, null, templateCode, true, null, null, true, related, parserEntryPoint);
        }

        public Guid CreateEmail(EntityReference regarding, EntityCollection from, string emailAddress, string subject, string description, bool isvalidToSend)
        {
            this.GlobalContext.LogEntry();

            EntityCollection to = new EntityCollection();
            Entity activityParty = new Entity(ActivityParty.EntityLogicalName);
            activityParty[ActivityParty.Fields.AddressUsed] = emailAddress;
            to.Entities.Add(activityParty);

            return this.HandleMappingAndCreateEmail(regarding, from, to, null, null, false, subject, description, isvalidToSend);
        }

        private void SetEmailTemplate(Email targetEmailEntity)
        {
            this.GlobalContext.LogEntry();

            if (!targetEmailEntity.AttributeHasValue<EntityReference>(Email.Fields.alt_EmailTemplateId)
                && targetEmailEntity.AttributeHasValue<int?>(Email.Fields.alt_TemplateCodeInt))
            {
                EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL(this.GlobalContext);
                targetEmailEntity.alt_EmailTemplateId = emailTemplateDAL.GetFirstActivetOrDefaultByAttribute<int>(alt_EmailTemplate.Fields.alt_CodeInt, targetEmailEntity.alt_TemplateCodeInt.Value, new[] { alt_EmailTemplate.Fields.alt_EmailTemplateId })?
                    .ToEntityReference() ?? throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.TemplateNotExist, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.TemplateNotExist));
            }
        }

        private void SetSenderByEmailTemplate(Email targetEmailEntity)
        {
            this.GlobalContext.LogEntry();

            if (targetEmailEntity.AttributeHasValue<EntityReference>(Email.Fields.alt_EmailTemplateId)
                && targetEmailEntity.AttributeHasValue<bool?>(Email.Fields.alt_IsAutomaticBit)
                && targetEmailEntity.alt_IsAutomaticBit.Value)
            {
                EntityReference from = new EntityReference();
                EmailTemplateDAL emailTemplateDal = new EmailTemplateDAL(this.GlobalContext);
                alt_EmailTemplate retrievedEmailTemplate = emailTemplateDal.Get(targetEmailEntity.alt_EmailTemplateId.Id, new[] { alt_EmailTemplate.Fields.alt_SendFromCode, alt_EmailTemplate.Fields.alt_FromQueueId, alt_EmailTemplate.Fields.alt_FromTeamId });

                SendFromCode sendFromCode = (SendFromCode)retrievedEmailTemplate.alt_SendFromCode.Value;

                switch (sendFromCode)
                {
                    case SendFromCode.Queue:
                        {
                            from = retrievedEmailTemplate.alt_FromQueueId;
                            break;
                        }

                    case SendFromCode.DefaultTeam:
                        {
                            TeamDAL teamDal = new TeamDAL(this.GlobalContext);
                            Team retrievedTeam = teamDal.Get(retrievedEmailTemplate.alt_FromTeamId.Id, new[] { "queueid" });
                            from = retrievedTeam.QueueId;
                            break;
                        }

                    case SendFromCode.User:
                        {
                            Guid currentUserId = this.GlobalContext.InitiatingUserId;
                            from = new EntityReference(SystemUser.EntityLogicalName, currentUserId);
                            break;
                        }

                    default:
                        {
                            throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.TemplateNotExist, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.TemplateNotExist));
                        }
                }
                targetEmailEntity[Email.Fields.From] = from.ConvertEntityReferenceToActivityPartyEntityCollection();
            }
        }

        private Guid HandleMappingAndCreateEmail(EntityReference regarding, EntityCollection from, EntityCollection to, EntityReference template, int? templateCode = null, bool isAutomaticBit = true, string subject = null, string description = null, bool isValidToSend = true, EntityCollection related = null, string parserEntryPoint = null)
        {
            this.GlobalContext.LogEntry();

            Email emailToCreate = GetInitializedEmail(regarding, from, template, templateCode, isAutomaticBit, subject, description);
            emailToCreate[Email.Fields.To] = to.FilterOnlyActivityPartyRecords();
            emailToCreate[Email.Fields.related] = related;
            emailToCreate[Email.Fields.alt_ParserCustomEntryPoint] = parserEntryPoint;

            return CreateAndSendEmailHandler(emailToCreate, isValidToSend);
        }

        private Guid HandleMappingAndCreateEmail(EntityReference regarding, IEnumerable<EntityReference> from, IEnumerable<EntityReference> to, EntityReference template, int? templateCode = null, bool isAutomaticBit = true, string subject = null, string description = null, bool isValidToSend = true)
        {
            this.GlobalContext.LogEntry();

            var fromEntityCollection = from?.ConvertEntityReferenceToActivityPartyEntityCollection();
            Email emailToCreate = GetInitializedEmail(regarding, fromEntityCollection, template, templateCode, isAutomaticBit, subject, description);
            var toAttribute = to.ConvertEntityReferenceToActivityPartyEntityCollection();
            emailToCreate[Email.Fields.To] = toAttribute;

            return CreateAndSendEmailHandler(emailToCreate, isValidToSend);
        }

        private Guid CreateAndSendEmailHandler(Email emailToCreate, bool isValidToSend)
        {
            this.GlobalContext.LogEntry();

            EmailDAL emailDAL = new EmailDAL(this.GlobalContext);
            Guid emailId = emailDAL.Create(emailToCreate);
            if (isValidToSend)
            {
                this.SendEmailHandler(emailDAL, emailId);
            }
            return emailId;
        }

        private ActionResult SendEmailHandler(EmailDAL emailDAL, Guid emailId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                emailDAL.SendEmail(emailId);
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Error(ex.ToString());
                actionResult.IsSuccess = false;
                emailDAL.Update(new Email()
                {
                    Id = emailId,
                    StateCode = EmailState.Canceled,
                    StatusCode = new OptionSetValue((int)EmailStatusCode.Canceled)
                });
            }
            return actionResult;
        }

        private Email GetInitializedEmail(EntityReference regarding, EntityCollection from, EntityReference template, int? templateCode = null, bool isAutomaticBit = true, string subject = null, string description = null)
        {
            this.GlobalContext.LogEntry();
            Email email = new Email
            {
                alt_IsAutomaticBit = isAutomaticBit,
                alt_EmailTemplateId = template,
                alt_TemplateCodeInt = templateCode,
                RegardingObjectId = regarding,
                alt_CreationMethodCode = new OptionSetValue((int)CreationMethodCode.Proccess)
            };

            if (!string.IsNullOrWhiteSpace(subject))
            {
                email.Subject = subject;
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                email.Description = description;
            }
            if (!isAutomaticBit)
            {
                email[Email.Fields.From] = from;
            }
            return email;
        }

        private void SetEmailRelated(Email emailToCreate, List<Recipient> recipients)
        {
            this.GlobalContext.LogEntry();
            EntityCollection related = new EntityCollection();
            recipients.ForEach(recipient =>
            {
                if (recipient.CustomerId != null)
                {
                    Entity activityParty = new Entity(ActivityParty.EntityLogicalName);
                    activityParty.Attributes.Add(ActivityParty.Fields.PartyId, new EntityReference(recipient.CustomerId.LogicalName, recipient.CustomerId.Id));
                    related.Entities.Add(activityParty);
                }
            });
            if (related.Entities.Count > 0)
            {
                emailToCreate[Email.Fields.related] = related;
            }
        }

        private void SetEmailAddressused(Email emailToCreate, List<Recipient> recipients)
        {
            this.GlobalContext.LogEntry();
            List<ActivityParty> to = new List<ActivityParty>();
            recipients.ForEach(recipient =>
            {
                if (!string.IsNullOrWhiteSpace(recipient.Email))
                {
                    var addedRecipients = to.Where(r => r.Attributes.ContainsKey(ActivityParty.Fields.AddressUsed)
                                         && (string)r.Attributes[ActivityParty.Fields.AddressUsed] == recipient.Email);
                    if (addedRecipients?.Count() == 0)
                    {
                        ActivityParty activityParty = new ActivityParty();
                        activityParty[ActivityParty.Fields.AddressUsed] = recipient.Email;
                        to.Add(activityParty);
                    }
                }
                else
                {
                    this.GlobalContext.Log.Warning($"Attempt to send email for {recipient.CustomerId?.LogicalName} id ({recipient.CustomerId?.Id}) without email address.");
                }
            });
            if (to.Count > 0)
            {
                emailToCreate.To = to;
            }
        }
    }
}
