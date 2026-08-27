using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class MailingBL : ExternalBLBase
    {
        const string abandonmentDuringJoiningProcessName = "AbandonmentDuringJoiningProcess";
        const string tradeMailingSettingsGlobalParameterName = "TradeAutomaticMailingProcessesSettings";

        public MailingBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult HandleAutomaticMailingOnAbandonedJoiningProcess(ApiScheduledOperation apiScheduledOperation)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            bool isSuccess = true;

            AutomaticMailingProcessSettings processSettings = this.GetAutomaticMailingProcess(tradeMailingSettingsGlobalParameterName);

            if (processSettings != null
                && (processSettings.EmailTemplateCode != null || processSettings.SmsTemplateCode != null))
            {
                List<ApiDigitalForm> digitalForms = this.GetDigitalFormsToMailing();
                List<MailingResult> mailingResults = new List<MailingResult>();

                foreach (var digitalForm in digitalForms)
                {
                    MailingResult smsMailingResult = new MailingResult() { SourceId = digitalForm.Id.Value, TemplateType = TemplateType.Sms };
                    MailingResult emailMailingResult = new MailingResult() { SourceId = digitalForm.Id.Value, TemplateType = TemplateType.Email };

                    LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                    OpportunityDAL opportunityDal = new OpportunityDAL(this.GlobalContext);
                    ApiLead retrievedLead = leadDal.Get(digitalForm.RegardingObject.Id.Value, new string[] { "mobilephone", "emailaddress1", "parentaccountid", "parentcontactid", "qualifyingopportunityid", "ownerid" });
                    ApiOpportunity retrievedOpportunity = null;
                    ApiEntity customer = retrievedLead.ParentContactId ?? retrievedLead.ParentAccountId;

                    if (retrievedLead.QualifyingOpportunityId != null)
                    {
                        retrievedOpportunity = opportunityDal.Get(retrievedLead.QualifyingOpportunityId.Id.Value, new string[] { "alt_mobilephone", "emailaddress" });                     
                    }
                    string mobilePhone = retrievedOpportunity?.MobilePhone ?? retrievedLead.MobilePhone;
                    string emailAddress = retrievedOpportunity?.EmailAddress ?? retrievedLead.EmailAddress1;
                    ActionResult sendSmsActionResult = this.SendSms(retrievedLead, customer, mobilePhone, processSettings.SmsTemplateCode, smsMailingResult);
                    ActionResult sendEmailActionResult = this.SendEmail(retrievedLead, customer, emailAddress, processSettings.EmailTemplateCode, emailMailingResult);

                    if (!sendSmsActionResult.IsSuccess || !sendEmailActionResult.IsSuccess && isSuccess)
                    {
                        isSuccess = false;
                    }

                    if ((sendSmsActionResult.IsSuccess && sendSmsActionResult.ReturnObject != null)
                        || (sendEmailActionResult.IsSuccess && sendEmailActionResult.ReturnObject != null))
                    {
                        this.UpdateDigitalFormSentSecondAbandonmentNoticeBit(digitalForm);
                    }
                    mailingResults.Add(smsMailingResult);
                    mailingResults.Add(emailMailingResult);
                }
                HtmlBuilder htmlBuilder = new HtmlBuilder();
                string mailingResultHtml = mailingResults.Count > 0 ? htmlBuilder.CreateTable<MailingResult>(mailingResults) : string.Empty;
                actionResult.IsSuccess = isSuccess;
                actionResult.ReturnObject = $"Abandoned digital forms to notice count: {digitalForms.Count}. {mailingResultHtml} {processSettings?.ToString()}";
            }
            return actionResult;
        }

        public ActionResult SendSms(ApiEntityBase regardingObjectId, ApiEntity customer, string mobilePhone, int? smsTemplateCode, MailingResult mailingResult = null)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (smsTemplateCode != null && mobilePhone != null)
            {
                try
                {
                    SmsDAL smsBl = new SmsDAL(this.GlobalContext);
                    ApiSms smsToCreate = new ApiSms
                    {
                        IsAutomatic = true,
                        Owner = regardingObjectId.Owner,
                        StatusCode = (int)SmsStatusCode.Send,
                        RegardingObject = regardingObjectId,
                        MobilePhone = mobilePhone,
                        TemplateCode = smsTemplateCode,
                        ContactId = customer != null && customer.LogicalName == ApiContact.EntityLogicalName ? customer as ApiContact : null
                    };
                    actionResult.ReturnObject = smsBl.Create(smsToCreate);
                }
                catch (Exception ex)
                {
                    actionResult.SetToFailedActionResult(ex.Message);
                    this.GlobalContext.Log.Error(ex.ToString());
                }
                finally
                {
                    if (mailingResult != null)
                    {
                        mailingResult.Target = mobilePhone;
                        mailingResult.SuccessResult = actionResult.ReturnObject?.ToString();
                        mailingResult.FailedResult = actionResult.Error?.ToString();
                    }
                }
            }
            return actionResult;
        }

        private ActionResult SendEmail(ApiEntity regardingObject, ApiEntity customer, string emailAddress, int? emailTemplateCode, MailingResult mailingResult = null)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            if (emailTemplateCode != null)
            {
                if (!string.IsNullOrWhiteSpace(emailAddress))
                {
                    var relatedCustomer = customer != null ?
                        new List<ApiActivityParty>() { new ApiActivityParty(customer.LogicalName) { Id = customer.Id } } : null;
                    try
                    {
                        EmailSettings emailSettings = new EmailSettings()
                        {
                            Regarding = regardingObject,
                            Recipients = new List<ApiActivityParty>() { new ApiActivityParty() { AddressUsed = emailAddress } },
                            Related = relatedCustomer,
                            TemplateCode = emailTemplateCode
                        };
                        EmailBL emailBl = new EmailBL(this.GlobalContext);
                        actionResult = emailBl.CreateEmailByEmailSettings(emailSettings);
                    }
                    catch (Exception ex)
                    {
                        actionResult.SetToFailedActionResult(ex.Message);
                        this.GlobalContext.Log.Error(ex.ToString());
                    }
                    finally
                    {
                        if (mailingResult != null)
                        {
                            mailingResult.Target = emailAddress;
                            mailingResult.SuccessResult = actionResult.ReturnObject?.ToString();
                            mailingResult.FailedResult = actionResult.Error?.ToString();
                        }
                    }
                }
            }
            return actionResult;
        }

        private void UpdateDigitalFormSentSecondAbandonmentNoticeBit(ApiDigitalForm digitalForm)
        {
            this.GlobalContext.LogEntry();
            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            digitalFormDal.Update(new ApiDigitalForm()
            {
                Id = digitalForm.Id,
                SentSecondAbandonmentNoticeBit = true
            });
        }

        private AutomaticMailingProcessSettings GetAutomaticMailingProcess(string gobalParameterName)
        {
            this.GlobalContext.LogEntry();

            string globalParameterValue = this.GlobalContext.CacheManager.GetGlobalParameter<string>(gobalParameterName);
            var settings = base.GetDeserializedContent<AutomaticMailingSettings>(globalParameterValue);
            return settings?.MailingProcessesSettings?.Where(s => s.ProcessName == abandonmentDuringJoiningProcessName).FirstOrDefault();
        }

        private List<ApiDigitalForm> GetDigitalFormsToMailing()
        {
            this.GlobalContext.LogEntry();

            List<ApiDigitalForm> digitalFormsToAlert = new List<ApiDigitalForm>();
            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            List<ApiDigitalForm> retrievedDigitalForms = digitalFormDal.GetActiveByAttribute("alt_digitalformtypecode", (int)DigitalFormTypeCode.TradeJoining, new string[] { "regardingobjectid", "createdon", "alt_sentsecondabandonmentnoticebit" });
            if (retrievedDigitalForms != null && retrievedDigitalForms.Count > 0)
            {
                DateTime dateNow = DateTime.UtcNow;
                digitalFormsToAlert = retrievedDigitalForms?.Where(d => d.RegardingObject != null
                && d.RegardingObject.LogicalName == ApiLead.EntityLogicalName
                && d.SentSecondAbandonmentNoticeBit != true
                && d.CreatedOn.Value.AddHours(24) <= dateNow).ToList();
            }
            return digitalFormsToAlert;
        }
    }
}
