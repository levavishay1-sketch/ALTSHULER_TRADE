using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm
{
    public class AppointmentBL : CrmBaseBL
    {
        Entity regardingObject { get; set; }
        List<alt_AccountHolder> accountHolders { get; set; }

        Dictionary<string, string[]> regardingObjectAttributes = new Dictionary<string, string[]>
        {
            {Lead.EntityLogicalName, new []{ Lead.Fields.MobilePhone, Lead.Fields.EMailAddress1 }},
            {Opportunity.EntityLogicalName, new []
            {
                Opportunity.Fields.alt_MobilePhone,
                Opportunity.Fields.CustomerId,
                Opportunity.Fields.EmailAddress,
                Opportunity.Fields.alt_FirstName,
                Opportunity.Fields.alt_LastName,
                Opportunity.Fields.alt_CompanyName
            }}
        };
        public AppointmentBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetSubject(Appointment targetAppointment)
        {
            this.GlobalContext.LogEntry();
            if (targetAppointment.AttributeHasValue<EntityReference>(Appointment.Fields.alt_ActivitySubjectId))
            {
                List<string> nameParts = new List<string>();
                ActivitySubjectDAL activitySubjectDal = new ActivitySubjectDAL(this.GlobalContext);
                nameParts.Add(activitySubjectDal.GetPrimeryAttributeValue(targetAppointment.alt_ActivitySubjectId, alt_ActivitySubject.Fields.alt_Name));
                string customerName = this.GetCustomerNameByRegardingObject(targetAppointment);
                if (!string.IsNullOrWhiteSpace(customerName))
                {
                    nameParts.Add(customerName);
                }
                targetAppointment.Subject = string.Join(" - ", nameParts);
            }
        }

        public void SendSmsAndEmailByActivitySubject(Appointment targetAppointment)
        {
            this.GlobalContext.LogEntry();
            if (targetAppointment.AttributeHasValue<EntityReference>(Appointment.Fields.alt_ActivitySubjectId)
                && targetAppointment.AttributeHasValue<EntityReference>(Appointment.Fields.RegardingObjectId)
                && (targetAppointment.alt_SendEmailBit.Value
                    || targetAppointment.alt_SendSmsBit.Value))
            {
                ActivitySubjectDAL activitySubjectDal = new ActivitySubjectDAL(this.GlobalContext);
                alt_ActivitySubject retrievedActivitySubject = activitySubjectDal.Get(targetAppointment.alt_ActivitySubjectId.Id,
                    new[] { alt_ActivitySubject.Fields.alt_SmsTemplateId, alt_ActivitySubject.Fields.alt_EmailTemplateId });
                List<Recipient> recipients = this.GetRecipientsDetails(targetAppointment);
                if (retrievedActivitySubject.alt_EmailTemplateId != null && targetAppointment.alt_SendEmailBit.Value)
                {
                    this.SendEmail(targetAppointment, retrievedActivitySubject.alt_EmailTemplateId, recipients);
                }
                if (retrievedActivitySubject.alt_SmsTemplateId != null && targetAppointment.alt_SendSmsBit.Value)
                {
                    this.SendSms(targetAppointment, retrievedActivitySubject.alt_SmsTemplateId, recipients);
                }
            }
        }

        private void SendSms(Appointment targetAppointment, EntityReference smsTemplateId, List<Recipient> recipients)
        {
            this.GlobalContext.LogEntry();
            foreach (var recipient in recipients)
            {
                this.SendSms(targetAppointment, smsTemplateId, recipient);
            }
        }

        private List<Recipient> GetRecipientsDetails(Appointment targetAppointment)
        {
            this.GlobalContext.LogEntry();
            List<Recipient> recipients = new List<Recipient>();
            EntityReference regardingObjectId = targetAppointment.RegardingObjectId;
            switch (regardingObjectId.LogicalName)
            {
                case Lead.EntityLogicalName:
                    {
                        recipients = this.GetLeadRecipients(regardingObjectId);
                        break;
                    }
                case Opportunity.EntityLogicalName:
                    {
                        recipients = this.GetOpportunityRecipients(regardingObjectId);
                        break;
                    }
                case alt_DigitalFormVerification.EntityLogicalName:
                case alt_Portfolio.EntityLogicalName:
                    {
                        recipients = this.GetAccountHoldersRecipients(targetAppointment);
                        break;
                    }
                default:
                    break;
            }
            return recipients;

        }

        private List<Recipient> GetAccountHoldersRecipients(Appointment targetAppointment)
        {
            this.GlobalContext.LogEntry();
            List<Recipient> recipients = new List<Recipient>();
            List<EntityReference> requiredAttendees = targetAppointment.GetActivityPartiesAsEntityReferences(Appointment.Fields.RequiredAttendees);
            List<alt_AccountHolder> accountHolders = this.GetAccountHolderDetails(targetAppointment.RegardingObjectId, requiredAttendees);
            accountHolders.ForEach(accountHolder =>
            {
                recipients.Add(new Recipient
                {
                    CustomerId = accountHolder.alt_CustomerId,
                    MobilePhone = accountHolder.alt_MobilePhone,
                    Email = accountHolder.alt_Email
                });
            });
            return recipients;
        }

        private List<Recipient> GetOpportunityRecipients(EntityReference regardingObjectId)
        {
            this.GlobalContext.LogEntry();
            var regardingObject = this.GetRegardingObjectDetails(regardingObjectId).ToEntity<Opportunity>();
            return new List<Recipient>()
            {
                new Recipient
                {
                    MobilePhone = regardingObject.alt_MobilePhone,
                    Email = regardingObject.EmailAddress,
                    CustomerId = regardingObject.CustomerId
                }
            };
        }

        private List<Recipient> GetLeadRecipients(EntityReference regardingObjectId)
        {
            this.GlobalContext.LogEntry();

            var regardingObject = this.GetRegardingObjectDetails(regardingObjectId).ToEntity<Lead>();
            return new List<Recipient>()
            {
                new Recipient()
                {
                    MobilePhone = regardingObject.MobilePhone,
                    Email = regardingObject.EMailAddress1
                }
            };
        }

        private void SendSms(Appointment targetAppointment, EntityReference smsTemplate, Recipient recipient)
        {
            this.GlobalContext.LogEntry(recipient.MobilePhone);
            try
            {
                SmsBL smsBL = new SmsBL(this.GlobalContext);
                smsBL.CreateSms(targetAppointment.RegardingObjectId, recipient, null, smsTemplate, JsonSerializer.Serialize(targetAppointment.ToEntityReference()));
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Error(ex);
            }
        }

        private void SendEmail(Appointment targetAppointment, EntityReference emailTemplate, List<Recipient> recipients)
        {
            this.GlobalContext.LogEntry();
            var emailAddresses = recipients?.Where(r => !string.IsNullOrWhiteSpace(r.Email))?.ToList();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                try
                {
                    EmailBL emailBl = new EmailBL(this.GlobalContext);
                    emailBl.CreateEmail(targetAppointment.RegardingObjectId, recipients, emailTemplate, JsonSerializer.Serialize(targetAppointment.ToEntityReference()));
                }
                catch (Exception ex)
                {
                    this.GlobalContext.Log.Error(ex);
                }
            }
        }

        private string GetCustomerNameByRegardingObject(Appointment targetAppointment)
        {
            this.GlobalContext.LogEntry();
            string customerName = null;
            if (targetAppointment.RegardingObjectId != null)
            {
                EntityReference regardingObjectId = targetAppointment.RegardingObjectId;
                switch (regardingObjectId.LogicalName)
                {
                    case Opportunity.EntityLogicalName:
                        {
                            Opportunity retrievedOpportunity = this.GetRegardingObjectDetails(regardingObjectId).ToEntity<Opportunity>();
                            if (retrievedOpportunity.CustomerId != null)
                            {
                                customerName = retrievedOpportunity.CustomerId.LogicalName == Contact.EntityLogicalName ?
                                                             $"{retrievedOpportunity.alt_FirstName} {retrievedOpportunity.alt_LastName}" : retrievedOpportunity.alt_CompanyName;
                            }
                            break;
                        }
                    case Lead.EntityLogicalName:
                        {
                            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                            customerName = leadDal.GetPrimeryAttributeValue(regardingObjectId, Lead.Fields.FullName);
                            break;
                        }
                    case alt_DigitalFormVerification.EntityLogicalName:
                    case alt_Portfolio.EntityLogicalName:
                        {
                            List<EntityReference> requiredAttendees = targetAppointment.GetActivityPartiesAsEntityReferences(Appointment.Fields.RequiredAttendees);
                            customerName = string.Join(" - ", this.GetAccountHolderDetails(regardingObjectId, requiredAttendees)?.Select(a => a.alt_Name).ToList());
                            break;
                        }
                    default:
                        break;
                }
            }
            return customerName;
        }

        private Entity GetRegardingObjectDetails(EntityReference regardingObjectId)
        {
            this.GlobalContext.LogEntry();
            if (this.regardingObject == null)
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, regardingObjectId.LogicalName);
                this.regardingObject = commonDAL.Get(regardingObjectId.Id, regardingObjectAttributes[regardingObjectId.LogicalName]);
            }
            return this.regardingObject;
        }

        private List<alt_AccountHolder> GetAccountHolderDetails(EntityReference regardingObjectId, List<EntityReference> customers)
        {
            this.GlobalContext.LogEntry();
            if (this.accountHolders == null)
            {
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                var customerIds = customers.Select(c => c.Id).ToList();
                this.accountHolders = accountHolderDal.GetRelatedAccountHolders(regardingObjectId, customerIds);
            }
            return this.accountHolders;
        }
    }
}
