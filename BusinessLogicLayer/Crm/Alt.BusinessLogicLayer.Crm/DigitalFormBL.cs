using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class DigitalFormBL : CrmBaseBL
    {
        const string abandonmentDuringJoiningProcessName = "AbandonmentDuringJoiningProcess";
        alt_DigitalFormStatus digitalFormStatus { get; set; }

        public DigitalFormBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void HandleTransferToOutSystemStatusCode(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if ((targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_TransferToOutSystemStatusCode)
                    && targetDigitalForm.alt_TransferToOutSystemStatusCode.Value == (int)TransferStatusCode.Send)
                || (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_CreationMethodCode)
                    && targetDigitalForm.alt_CreationMethodCode.Value == (int)CreationMethodCode.Manual
                    && targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_DigitalFormTypeCode)
                    && targetDigitalForm.alt_DigitalFormTypeCode.Value == (int)DigitalFormTypeCode.TradeJoining))
            {
                targetDigitalForm.alt_TransferToOutSystemStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
                if (!string.IsNullOrWhiteSpace(preDigitalForm.alt_TransferToOutSystemErrorDescription))
                {
                    targetDigitalForm.alt_TransferToOutSystemErrorDescription = null;
                }
            }
        }

        public void SetDigitalFormStatusByDataDuplicateDigitalFormLink(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.AttributeHasValue<string>(alt_DigitalForm.Fields.alt_DigitalFormLink)
                && preDigitalForm.alt_CreationMethodCode != null
                && preDigitalForm.alt_CreationMethodCode.Value == (int)DigitalFormCreationMethodCode.Manual)
            {
                DigitalFormDAL digitalFormDAL = new DigitalFormDAL(this.GlobalContext);
                var retrievedDigitalForm = digitalFormDAL.GetAnotherActiveDigitalFormByLink(targetDigitalForm.Id, targetDigitalForm.alt_DigitalFormLink);

                if (retrievedDigitalForm != null)
                {
                    this.GlobalContext.Log.Info("other active digital form with the same link: " + retrievedDigitalForm.Id);

                    string code = this.GlobalContext.CacheManager.GetGlobalParameter<string>("DigitalFormDuplicateStatusCode");
                    DigitalFormStatusDAL digitalFormStatusDal = new DigitalFormStatusDAL(this.GlobalContext);
                    alt_DigitalFormStatus retrievedDigitalFormStatus = digitalFormStatusDal.GetByAttribute(alt_DigitalFormStatus.Fields.alt_Code, code, new[] { alt_DigitalFormStatus.Fields.alt_DigitalFormStatusId }).FirstOrDefault();
                    targetDigitalForm.alt_DigitalFormStatusId = retrievedDigitalFormStatus.ToEntityReference();
                }
            }
        }

        public void ActivateDigitalFormVerificationDocumentSearch(alt_DigitalForm targetDigitalForm)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.StatusCode)
                && targetDigitalForm.StatusCode.Value == (int)DigitalFormStatusCode.SentToVerification)
            {
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                var retrievedDigitalFormVerification = digitalFormVerificationDal.GetFirstActivetOrDefaultByAttribute(alt_DigitalFormVerification.Fields.alt_DigitalFormId, targetDigitalForm.Id, new string[]
                {
                    alt_DigitalFormVerification.Fields.Id,
                    alt_DigitalFormVerification.Fields.alt_Name
                });
                if (retrievedDigitalFormVerification != null)
                {
                    CommonDAL commonDal = new CommonDAL(this.GlobalContext, alt_ArchiveDocumentSearch.EntityLogicalName);
                    Guid id = commonDal.Create(new alt_ArchiveDocumentSearch()
                    {
                        RegardingObjectId = retrievedDigitalFormVerification.ToEntityReference(),
                        Subject = retrievedDigitalFormVerification.alt_Name,
                        alt_SearchFromArchiveStatusCode = new OptionSetValue((int)TransferStatusCode.Send)
                    });
                }
            }
        }

        public void HandleAutomaicMailing(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();

            var mergedDigitalForm = targetDigitalForm.Merge<alt_DigitalForm>(preDigitalForm);
            if (targetDigitalForm.AttributeHasValue<bool?>(alt_DigitalForm.Fields.alt_SendAbandonmentNoticeBit)
                && targetDigitalForm.alt_SendAbandonmentNoticeBit.Value
                && mergedDigitalForm.RegardingObjectId != null)
            {
                switch (mergedDigitalForm.RegardingObjectId.LogicalName)
                {
                    case Lead.EntityLogicalName:
                        {
                            if (SendSmsAndEmailByLeadRegardingObject(mergedDigitalForm.RegardingObjectId, abandonmentDuringJoiningProcessName))
                            {
                                this.UpdateSentFirstAbandonmentNoticeBit(mergedDigitalForm);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        public void HandleDigitalFormLink(alt_DigitalForm targetDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.AttributeHasValue<string>(alt_DigitalForm.Fields.alt_DigitalFormLink)
                && targetDigitalForm.AttributeHasValue<EntityReference>(alt_DigitalForm.Fields.RegardingObjectId)
                && targetDigitalForm.RegardingObjectId.LogicalName == Lead.EntityLogicalName)
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                Lead retrievedLead = leadDal.GetFirstActivetOrDefaultByAttribute(Lead.Fields.alt_LeadIdentityNumber, targetDigitalForm.alt_DigitalFormIdentityNumber, new string[] { Lead.Fields.alt_DigitalFormLink });
                if (!retrievedLead.AttributeHasValue<string>(Lead.Fields.alt_DigitalFormLink))
                {
                    leadDal.Update(new Lead()
                    {
                        Id = targetDigitalForm.RegardingObjectId.Id,
                        alt_DigitalFormLink = targetDigitalForm.alt_DigitalFormLink
                    });
                }
            }
        }

        public void SetDigitalFormStatusOnManualCreateInOS(alt_DigitalForm targetDigitalForm)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_TransferToOutSystemStatusCode)
                && targetDigitalForm.alt_TransferToOutSystemStatusCode.Value == (int)TransferStatusCode.Sent
                && targetDigitalForm.AttributeHasValue<string>(alt_DigitalForm.Fields.alt_DigitalFormLink))
            {
                string code = this.GlobalContext.CacheManager.GetGlobalParameter<string>("DigitalFormManualCreatedStatusCode");
                DigitalFormStatusDAL digitalFormStatusDal = new DigitalFormStatusDAL(this.GlobalContext);
                alt_DigitalFormStatus retrievedDigitalFormStatus = digitalFormStatusDal.GetByAttribute(alt_DigitalFormStatus.Fields.alt_Code, code, new[] { alt_DigitalFormStatus.Fields.alt_DigitalFormStatusId }).FirstOrDefault();
                targetDigitalForm.alt_DigitalFormStatusId = retrievedDigitalFormStatus.ToEntityReference();
            }
        }

        public void HandleAbandonedJoiningProcess(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();

            this.SetSendAbandonmentNoticeBit(targetDigitalForm, preDigitalForm);
            this.ClearAbandonedProcessAttributes(targetDigitalForm, preDigitalForm);
        }

        public void SetSendAbandonmentNoticeBit(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalForm.AttributeHasValue<bool?>(alt_DigitalForm.Fields.alt_AbandonedProcessBit)
                && targetDigitalForm.alt_AbandonedProcessBit.Value)
            {
                var mergedDigitalForm = targetDigitalForm.Merge<alt_DigitalForm>(preDigitalForm);
                if (mergedDigitalForm.RegardingObjectId != null && mergedDigitalForm.alt_DigitalFormTypeCode != null)
                {
                    DigitalFormTypeCode digitalFormTypeCode = (DigitalFormTypeCode)mergedDigitalForm.alt_DigitalFormTypeCode.Value;
                    switch (digitalFormTypeCode)
                    {
                        case DigitalFormTypeCode.TradeJoining:
                            {
                                if (mergedDigitalForm.alt_SendAbandonmentNoticeBit != true)
                                {
                                    targetDigitalForm.alt_SendAbandonmentNoticeBit = true;
                                }
                                break;
                            }

                        default:
                            break;
                    }
                }
            }
        }

        public void ClearAbandonedProcessAttributes(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.AttributeHasValue<EntityReference>(alt_DigitalForm.Fields.alt_DigitalFormStatusId))
            {
                if (preDigitalForm.alt_AbandonedProcessBit == null
                    || preDigitalForm.alt_AbandonedProcessBit.Value)
                {
                    targetDigitalForm.alt_AbandonedProcessBit = false;
                }
                if (!string.IsNullOrWhiteSpace(preDigitalForm.alt_AbandonmentPage))
                {
                    targetDigitalForm.alt_AbandonmentPage = null;
                }
            }
        }

        public void SetDigitalFormStatusByDataReceptionStatus(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_DataReceptionStatusCode))
            {
                string digitalFormStatusCodeParameterName = null;
                DataReceptionStatusCode dataReceptionStatusCode = (DataReceptionStatusCode)targetDigitalForm.alt_DataReceptionStatusCode.Value;
                switch (dataReceptionStatusCode)
                {
                    case DataReceptionStatusCode.UnderConstruction:
                        {
                            digitalFormStatusCodeParameterName = "DigitalFormUnderDataConstructionStatusCode";
                            break;
                        }
                    case DataReceptionStatusCode.Success:
                        {
                            this.HandleSuccessDataReceptionStatusCode(targetDigitalForm, preDigitalForm);
                            break;
                        }
                    case DataReceptionStatusCode.Failed:
                        {
                            digitalFormStatusCodeParameterName = "DigitalFormDataConstructionFaildStatusCode";
                            break;
                        }
                    case DataReceptionStatusCode.Retry:
                    default:
                        break;
                }
                if (digitalFormStatusCodeParameterName != null)
                {
                    this.SetDigitalFormStatusByGlobalParameter(targetDigitalForm, digitalFormStatusCodeParameterName);
                }
            }
        }

        private void HandleSuccessDataReceptionStatusCode(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();

            var digitalFormTemplateId = targetDigitalForm.alt_DigitalFormTemplateId ?? preDigitalForm.alt_DigitalFormTemplateId;
            if (digitalFormTemplateId != null)
            {
                this.SetDigitalFormStatusByTemplate(targetDigitalForm, digitalFormTemplateId);
            }
            else
            {
                this.SetDigitalFormStatusByGlobalParameter(targetDigitalForm, "TradeDigitalFormComplitedStatusCode");
            }
        }

        private void SetDigitalFormStatusByTemplate(alt_DigitalForm targetDigitalForm, EntityReference digitalFormTemplateId)
        {
            this.GlobalContext.LogEntry();

            var digitalFormTemplate = new CommonDAL(this.GlobalContext, alt_DigitalFormTemplate.EntityLogicalName)
                .GetFirstOrDefaultByAttribute(alt_DigitalFormTemplate.Fields.alt_DigitalFormTemplateId, digitalFormTemplateId.Id, new string[] { alt_DigitalFormTemplate.Fields.alt_SuccessSubmissionDigitalFormStatusId })?
                .ToEntity<alt_DigitalFormTemplate>();
            if (digitalFormTemplate?.alt_SuccessSubmissionDigitalFormStatusId != null)
            {
                targetDigitalForm.alt_DigitalFormStatusId = digitalFormTemplate.alt_SuccessSubmissionDigitalFormStatusId;
            }
        }

        private void SetDigitalFormStatusByGlobalParameter(alt_DigitalForm targetDigitalForm, string digitalFormStatusCodeParameterName)
        {
            this.GlobalContext.LogEntry();

            string code = this.GlobalContext.CacheManager.GetGlobalParameter<string>(digitalFormStatusCodeParameterName);
            var digitalFormStatus = this.GetDigitalFormStatusByCode(code);
            targetDigitalForm.alt_DigitalFormStatusId = digitalFormStatus.ToEntityReference();
        }

        public void HandleDataRecipientRetry(alt_DigitalForm targetDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.AttributeHasValue<OptionSetValue>(alt_DigitalForm.Fields.alt_DataReceptionStatusCode)
                  && targetDigitalForm.alt_DataReceptionStatusCode.Value == (int)DataReceptionStatusCode.Retry)
            {
                targetDigitalForm.alt_DataReceptionStatusCode = new OptionSetValue((int)DataReceptionStatusCode.UnderConstruction);
            }
        }

        public void HandleRegardingObject(alt_DigitalForm targetDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.AttributeHasValue<EntityReference>(alt_DigitalForm.Fields.RegardingObjectId))
            {
                this.MappDigitalFormByRegardingObject(targetDigitalForm);
            }
            else if (targetDigitalForm.alt_DigitalFormTypeCode != null
                && targetDigitalForm.alt_DigitalFormTypeCode.Value == (int)DigitalFormTypeCode.TradeJoining
                && !string.IsNullOrWhiteSpace(targetDigitalForm.alt_DigitalFormIdentityNumber))
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                Lead retrievedLead = leadDal.GetFirstActivetOrDefaultByAttribute(Lead.Fields.alt_LeadIdentityNumber, targetDigitalForm.alt_DigitalFormIdentityNumber, new string[] { Lead.Fields.LeadId, Lead.Fields.alt_IdentityNumber, Lead.Fields.MobilePhone, Lead.Fields.FullName });
                if (retrievedLead != null)
                {
                    targetDigitalForm.RegardingObjectId = retrievedLead.ToEntityReference();
                    targetDigitalForm.RegardingObjectId.Name = retrievedLead.FullName;
                    targetDigitalForm.alt_CustomerIdentityNumber = retrievedLead.alt_IdentityNumber;
                    targetDigitalForm.Subject = this.GetDigitalFormSubject(targetDigitalForm.alt_DigitalFormIdentityNumber, retrievedLead.MobilePhone);
                }
            }
        }

        public void MappDigitalFormByRegardingObject(alt_DigitalForm targetDigitalForm)
        {
            this.GlobalContext.LogEntry();
            EntityReference regardingObjectId = targetDigitalForm.GetAttributeValue<EntityReference>(alt_DigitalForm.Fields.RegardingObjectId);
            if (regardingObjectId.LogicalName == Lead.EntityLogicalName)
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                Lead retrievedLead = leadDal.Get(regardingObjectId.Id, new string[] { Lead.Fields.alt_LeadIdentityNumber, Lead.Fields.alt_IdentityNumber, Lead.Fields.MobilePhone });
                targetDigitalForm.alt_DigitalFormIdentityNumber = retrievedLead.alt_LeadIdentityNumber;
                targetDigitalForm.Subject = this.GetDigitalFormSubject(targetDigitalForm.alt_DigitalFormIdentityNumber, retrievedLead.MobilePhone);
                if (!string.IsNullOrWhiteSpace(retrievedLead.alt_IdentityNumber))
                {
                    targetDigitalForm.alt_CustomerIdentityNumber = retrievedLead.alt_IdentityNumber;
                }
            }
        }

        public void SetStateCodeAndStatusCodeByDigitalFormStatusId(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.Contains(alt_DigitalForm.Fields.alt_DigitalFormStatusId)
                && targetDigitalForm.alt_DigitalFormStatusId != null)
            {
                alt_DigitalForm mergedDigitalForm = targetDigitalForm.Merge(preDigitalForm);
                DigitalFormStatusDAL digitalFormdStatusDal = new DigitalFormStatusDAL(GlobalContext);
                alt_DigitalFormStatus digitalFormStatus = digitalFormdStatusDal.GetDigitalFormStatusDetails(mergedDigitalForm.alt_DigitalFormStatusId.Id);

                if (mergedDigitalForm.StatusCode == null
                    || mergedDigitalForm.StatusCode.Value != digitalFormStatus.alt_DigitalFromStatusCode.Value)
                {
                    targetDigitalForm.StatusCode = digitalFormStatus.alt_DigitalFromStatusCode;
                    alt_DigitalFormState digitalFormState = this.GetDigitalFormStateByStatusCode(targetDigitalForm.StatusCode);
                    if (mergedDigitalForm.StateCode != digitalFormState)
                    {
                        targetDigitalForm.StateCode = digitalFormState;
                    }
                }
            }
        }

        public void HandleDigitalFormLinkUpdate(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();
            if (targetDigitalForm.Contains(alt_DigitalForm.Fields.alt_DigitalFormLink)
                && targetDigitalForm.alt_DigitalFormLink != null)
            {
                alt_DigitalForm mergedDigitalForm = targetDigitalForm.Equals(preDigitalForm) ?
                    targetDigitalForm : targetDigitalForm.Merge<alt_DigitalForm>(preDigitalForm);
                if (mergedDigitalForm.AttributeHasValue<EntityReference>(alt_DigitalForm.Fields.RegardingObjectId))
                {
                    LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                    leadDal.Update(new Lead()
                    {
                        Id = mergedDigitalForm.RegardingObjectId.Id,
                        alt_DigitalFormLink = targetDigitalForm.alt_DigitalFormLink
                    });
                }
            }
        }

        public void HandleDigitalFormStatusIdLogic(alt_DigitalForm targetDigitalForm, alt_DigitalForm preDigitalForm)
        {
            this.GlobalContext.LogEntry();

            if (targetDigitalForm.Contains(alt_DigitalForm.Fields.alt_DigitalFormStatusId)
                && targetDigitalForm.alt_DigitalFormStatusId != null)
            {
                DigitalFormStatusDAL digitalFormdStatusDal = new DigitalFormStatusDAL(GlobalContext);
                alt_DigitalFormStatus digitalFormStatus = digitalFormdStatusDal.GetDigitalFormStatusDetails(targetDigitalForm.alt_DigitalFormStatusId.Id);
                this.ExecuteDigitalFormStatusIdLogicByRegardingObject(targetDigitalForm, preDigitalForm.RegardingObjectId, digitalFormStatus);
            }
        }

        private alt_DigitalFormState GetDigitalFormStateByStatusCode(OptionSetValue statusCode)
        {
            this.GlobalContext.LogEntry();

            alt_DigitalFormState state = alt_DigitalFormState.Open;
            DigitalFormStatusCode digitalFormStatusCode = (DigitalFormStatusCode)statusCode.Value;
            switch (digitalFormStatusCode)
            {
                case DigitalFormStatusCode.SentToVerification:
                case DigitalFormStatusCode.Completed:
                    {
                        state = alt_DigitalFormState.Completed;
                        break;
                    }
                case DigitalFormStatusCode.Canceld:
                    {
                        state = alt_DigitalFormState.Canceled;
                        break;
                    }
                case DigitalFormStatusCode.Scheduled:
                    {
                        state = alt_DigitalFormState.Scheduled;
                        break;
                    }
                default:
                    break;
            }
            return state;
        }

        private void ExecuteDigitalFormStatusIdLogicByRegardingObject(alt_DigitalForm targetDigitalForm, EntityReference regardingObjectId, alt_DigitalFormStatus digitalFormStatus)
        {
            this.GlobalContext.LogEntry();

            if (regardingObjectId != null)
            {
                switch (regardingObjectId.LogicalName)
                {
                    case Lead.EntityLogicalName:
                        {
                            this.HandleLeadBusinessLogic(targetDigitalForm, regardingObjectId, digitalFormStatus);
                            break;
                        }
                    case Opportunity.EntityLogicalName:
                        {
                            this.HandleOpportunityBusinessLogic(regardingObjectId, digitalFormStatus);
                            break;
                        }
                    default:
                        break;
                }
            }
            else
            {
                this.GlobalContext.Log.Warning($"Digital form without regarding object");
            }
        }

        private void HandleLeadBusinessLogic(alt_DigitalForm targetDigitalForm, EntityReference regardingObjectId, alt_DigitalFormStatus digitalFormStatus)
        {
            this.GlobalContext.LogEntry();

            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            Lead retrievedLead = leadDal.GetLeadDetails(regardingObjectId.Id);

            if (retrievedLead.StateCode == LeadState.Open
                && digitalFormStatus.alt_LeadStatusCode != null
                && retrievedLead.StatusCode.Value != digitalFormStatus.alt_LeadStatusCode.Value
                || targetDigitalForm.AttributeHasValue<string>(alt_DigitalForm.Fields.alt_DigitalFormLink))
            {
                this.UpdateLeadByDigitalFormStatus(targetDigitalForm, retrievedLead, digitalFormStatus);
            }
            if (retrievedLead.QualifyingOpportunityId != null
                && digitalFormStatus.alt_OpportunityStatusCode != null)
            {
                this.HandleOpportunityBusinessLogic(retrievedLead.QualifyingOpportunityId, digitalFormStatus);
            }
        }

        private void UpdateLeadByDigitalFormStatus(alt_DigitalForm targetDigitalForm, Lead retrievedLead, alt_DigitalFormStatus digitalFormStatus)
        {
            this.GlobalContext.LogEntry();

            Lead leadToUpdate = new Lead() { Id = retrievedLead.Id };
            if (targetDigitalForm.AttributeHasValue<string>(alt_DigitalForm.Fields.alt_DigitalFormLink))
            {
                leadToUpdate.alt_DigitalFormLink = targetDigitalForm.alt_DigitalFormLink;
            }
            if (digitalFormStatus.alt_LeadStatusCode.Value != (int)LeadStatusCode.Qualified
                && retrievedLead.StatusCode.Value != digitalFormStatus.alt_LeadStatusCode.Value)
            {
                leadToUpdate.StatusCode = digitalFormStatus.alt_LeadStatusCode;
            }

            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            if (digitalFormStatus.alt_LeadStatusCode.Value == (int)LeadStatusCode.Qualified)
            {
                if (leadToUpdate.alt_DigitalFormLink != null)
                {
                    leadDal.Update(leadToUpdate);
                }
                QualifyLeadResponse qualifyLeadResponse = leadDal.QualifyLead(retrievedLead.ToEntityReference(), digitalFormStatus.alt_LeadStatusCode, true);
                retrievedLead.QualifyingOpportunityId = qualifyLeadResponse.CreatedEntities
                    .Where(e => e.LogicalName == Opportunity.EntityLogicalName)?.ToArray()?.FirstOrDefault();
            }
            else
            {
                leadDal.Update(leadToUpdate);
            }
        }

        private void HandleOpportunityBusinessLogic(EntityReference opprtunityEntityReference, alt_DigitalFormStatus digitalFormStatus)
        {
            this.GlobalContext.LogEntry();
            if (opprtunityEntityReference != null && digitalFormStatus.alt_OpportunityStatusCode != null)
            {
                OpportunityDAL opportunityDal = new OpportunityDAL(this.GlobalContext);
                Opportunity retrievedOpportunity = opportunityDal.Get(opprtunityEntityReference.Id, new string[] { Opportunity.Fields.StateCode, Opportunity.Fields.StatusCode });

                if (retrievedOpportunity.StateCode.Value == (int)OpportunityState.Open)
                {
                    this.UpdateOpportunityByDigitalFormStatus(retrievedOpportunity, digitalFormStatus);
                }
            }
        }

        private void UpdateOpportunityByDigitalFormStatus(Opportunity retrievedOpportunity, alt_DigitalFormStatus digitalFormStatus)
        {
            this.GlobalContext.LogEntry();

            OpportunityDAL opportunityDal = new OpportunityDAL(this.GlobalContext);

            Opportunity opportunityToUpdate = new Opportunity();
            opportunityToUpdate.Id = retrievedOpportunity.Id;
            opportunityToUpdate.CurrentSituation = digitalFormStatus.alt_Name;

            Func<EntityReference, OptionSetValue, OrganizationResponse> closeOpportunity = null;
            OptionSetValue opportunityStatusCode = digitalFormStatus.alt_OpportunityStatusCode;
            OpportunityStatusCode opportunityStatus = (OpportunityStatusCode)opportunityStatusCode.Value;

            if (retrievedOpportunity.StatusCode.Value != digitalFormStatus.alt_OpportunityStatusCode.Value)
            {
                switch (opportunityStatus)
                {
                    case OpportunityStatusCode.InProgress:
                    case OpportunityStatusCode.SendToInspection:
                        {
                            opportunityToUpdate.StatusCode = opportunityStatusCode;
                            break;
                        }
                    case OpportunityStatusCode.Winning:
                        {
                            closeOpportunity = opportunityDal.CloseOpportunityAsWon;
                            break;
                        }
                    case OpportunityStatusCode.Canceld:
                    case OpportunityStatusCode.AnotherFactorWon:
                        {
                            closeOpportunity = opportunityDal.CloseOpportunityAsLost;
                            break;
                        }
                    default:
                        break;
                }
            }
            opportunityDal.Update(opportunityToUpdate);
            if (closeOpportunity != null)
            {
                closeOpportunity(retrievedOpportunity.ToEntityReference(), opportunityStatusCode);
            }
        }

        private string GetDigitalFormSubject(string digitalFormIdentityNumber, string mobilePhone)
        {
            this.GlobalContext.LogEntry();
            List<string> nameParts = new List<string>()
            {
                digitalFormIdentityNumber,
                mobilePhone
            };
            return string.Join(" - ", nameParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private alt_DigitalFormStatus GetDigitalFormStatusByCode(string code)
        {
            if (this.digitalFormStatus == null)
            {
                DigitalFormStatusDAL digitalFormdStatusDal = new DigitalFormStatusDAL(GlobalContext);
                this.digitalFormStatus = digitalFormdStatusDal.GetByAttribute(alt_DigitalFormStatus.Fields.alt_Code, code, null).FirstOrDefault();
            }
            return this.digitalFormStatus;
        }


        private bool SendSmsAndEmailByLeadRegardingObject(EntityReference regardingObjectId, string processName)
        {
            this.GlobalContext.LogEntry();

            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            Lead retrievedLead = leadDal.GetLeadDetails(regardingObjectId.Id);
            Recipient recipient = new Recipient()
            {
                CustomerId = retrievedLead.ParentContactId ?? retrievedLead.ParentAccountId,
                MobilePhone = retrievedLead.MobilePhone,
                Email = retrievedLead.EMailAddress1
            };
            CommonBL commonBL = new CommonBL(this.GlobalContext);
            bool isSent = commonBL.ExecuteTradeAutomaticMailing(regardingObjectId, recipient, null, processName);

            return isSent;
        }

        private void UpdateSentFirstAbandonmentNoticeBit(alt_DigitalForm mergedDigitalForm)
        {
            this.GlobalContext.LogEntry();

            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            digitalFormDal.Update(new alt_DigitalForm()
            {
                Id = mergedDigitalForm.Id,
                alt_SentFirstAbandonmentNoticeBit = true
            });
        }
    }
}
