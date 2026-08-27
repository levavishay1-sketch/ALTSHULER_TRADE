using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class PhoneCallBL : CrmBaseBL
    {
        string assignedToRepresentitiveTreatmentStatusCodeGlobalParameterName = "AssignedToRepresentitiveTreatmentStatusCode";
        string noAnswerTreatmentStatusCodeGlobalParameterName = "NoAnswerTreatmentStatusCode";

        public PhoneCallBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetStatusCodeByCallStatusCode(PhoneCall targetPhoneCall)
        {
            this.GlobalContext.LogEntry();

            if (targetPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.alt_StatusCode)
                && targetPhoneCall.AttributeHasValue<bool>(PhoneCall.Fields.alt_CompleteActivityBit)
                && targetPhoneCall.alt_CompleteActivityBit.Value)
            {
                targetPhoneCall.StatusCode = new OptionSetValue((int)PhoneCallStatusCode.Made);
            }
        }

        public void SetStateCodeByStatusCode(PhoneCall targetPhoneCall)
        {
            this.GlobalContext.LogEntry();

            if (targetPhoneCall.Contains(PhoneCall.Fields.StatusCode))
            {
                PhoneCallState state = PhoneCallState.Open;
                PhoneCallStatusCode phoneCallStatusCode = (PhoneCallStatusCode)targetPhoneCall.StatusCode.Value;

                switch (phoneCallStatusCode)
                {
                    case PhoneCallStatusCode.Made:
                    case PhoneCallStatusCode.Canceled:
                        {
                            state = PhoneCallState.Completed;
                            break;
                        }
                    default:
                        break;
                }

                targetPhoneCall.StateCode = state;
            }
        }


        public void ValidateFieldsBeforeCompletion(PhoneCall targetPhoneCall, PhoneCall prePhoneCall)
        {
            GlobalContext.LogEntry();

            PhoneCall mergedPhoneCall = targetPhoneCall.Merge(prePhoneCall);
            if (targetPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.StateCode) &&
                targetPhoneCall.StateCode.Value == PhoneCallState.Completed)
            {
                bool statusCodeContainsValue = mergedPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.alt_StatusCode);
                bool callBackRequiredCodeContainsValue = mergedPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.alt_CallBackRequiredCode);

                if (!statusCodeContainsValue)
                {
                    throw new InvalidPluginExecutionException("על מנת להשלים את השיחה יש למלא סטטוס שיחה והאם נדרשת שיחה חוזרת ללקוח");
                }
                else if (mergedPhoneCall.alt_StatusCode.Value != (int)CallStatusCode.CustomerNotInterested && !callBackRequiredCodeContainsValue)
                {
                    throw new InvalidPluginExecutionException("על מנת להשלים את השיחה יש למלא סטטוס שיחה והאם נדרשת שיחה חוזרת ללקוח");
                }
            }
        }


        public void HandleSubjectByCreationMethod(PhoneCall targetPhoneCall)
        {
            this.GlobalContext.LogEntry();

            if (targetPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.alt_CreationMethodCode)
                && targetPhoneCall.alt_CreationMethodCode.Value == (int)CreationMethodCode.Manual)
            {
                CommonDAL commonDal = new CommonDAL(this.GlobalContext, targetPhoneCall.RegardingObjectId.LogicalName);
                string customerName = !string.IsNullOrWhiteSpace(targetPhoneCall.RegardingObjectId.Name)
                    ? targetPhoneCall.RegardingObjectId.Name
                    : commonDal.GetPrimeryAttributeValue(targetPhoneCall.RegardingObjectId);

                targetPhoneCall.Subject = $"שיחת טלפון - {customerName}";
            }
        }


        public void HandleCallbackPhoneCallForLead(PhoneCall targetPhoneCall, PhoneCall prePhoneCall = null)
        {
            GlobalContext.LogEntry();

            PhoneCall mergedPhoneCall = targetPhoneCall.Merge(prePhoneCall);

            if (targetPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.StateCode)
                && targetPhoneCall.StateCode == PhoneCallState.Completed
                && mergedPhoneCall.alt_CallBackRequiredCode != null
                && mergedPhoneCall.alt_CallBackRequiredCode.Value == (int)YesNoCode.Yes)

            {
                CreateCallbackPhoneCall(mergedPhoneCall);
            }
        }

        private void CreateCallbackPhoneCall(PhoneCall phoneCall)
        {
            GlobalContext.LogEntry();

            PhoneCall newPhoneCall = MapCallbackPhoneCall(phoneCall);
            Guid createdPhoneCallId = new CommonDAL(GlobalContext, PhoneCall.EntityLogicalName).Create(newPhoneCall);

            GlobalContext.Log.Info($"\nCreatedPhoneCallId: {createdPhoneCallId}");
        }

        private PhoneCall MapCallbackPhoneCall(PhoneCall mergedPhoneCall)
        {
            GlobalContext.LogEntry();

            IEnumerable<ActivityParty> to = mergedPhoneCall.To?.Select(x => new ActivityParty { PartyId = x.PartyId });
            IEnumerable<ActivityParty> from = mergedPhoneCall.From?.Select(x => new ActivityParty { PartyId = x.PartyId });

            CommonDAL commonDal = new CommonDAL(this.GlobalContext, mergedPhoneCall.RegardingObjectId.LogicalName);
            string customerName = !string.IsNullOrWhiteSpace(mergedPhoneCall.RegardingObjectId.Name)
                ? mergedPhoneCall.RegardingObjectId.Name
                : commonDal.GetPrimeryAttributeValue(mergedPhoneCall.RegardingObjectId);

            PhoneCall callbackPhoneCall = new PhoneCall()
            {
                Subject = $"שיחת טלפון חוזרת ל{customerName}",
                ScheduledEnd = mergedPhoneCall.alt_CallBackDate,
                To = to,
                From = from,
                OwnerId = mergedPhoneCall.OwnerId,
                DirectionCode = true,
                PhoneNumber = mergedPhoneCall.PhoneNumber,
                PriorityCode = new OptionSetValue((int)PriorityCode.Normal),
                RegardingObjectId = mergedPhoneCall.RegardingObjectId,
                alt_CreationMethodCode = new OptionSetValue((int)CreationMethodCode.Proccess)
            };

            return callbackPhoneCall;
        }


        public void HandleRegardingObjectUpdate(PhoneCall targetPhoneCall, PhoneCall prePhoneCall = null)
        {
            this.GlobalContext.LogEntry();

            PhoneCall mergedPhoneCall = targetPhoneCall.Merge(prePhoneCall);
            switch (mergedPhoneCall.RegardingObjectId.LogicalName)
            {
                case Lead.EntityLogicalName:
                    {
                        this.HandleRegardingLeadUpdate(targetPhoneCall, mergedPhoneCall);
                        break;
                    }
                default:
                    break;
            }
        }

        private void HandleRegardingLeadUpdate(PhoneCall targetPhoneCall, PhoneCall mergedPhoneCall)
        {
            this.GlobalContext.LogEntry();

            if (targetPhoneCall.Contains(PhoneCall.Fields.StateCode) && targetPhoneCall.StateCode == PhoneCallState.Completed)
            {
                LeadDAL leadDAL = new LeadDAL(GlobalContext);
                Lead retrievedLead = leadDAL.Get(mergedPhoneCall.RegardingObjectId.Id,
                    new string[]
                    {
                        Lead.Fields.alt_TotalPhoneCallsInt,
                        Lead.Fields.alt_TotalMissedPhoneCallsInt,
                        Lead.Fields.alt_TotalPhoneCallsAnsweredInt,
                        Lead.Fields.alt_TotalIVRMissedPhoneCallsInt,
                        Lead.Fields.alt_TotalMissedPhoneCallsTodayInt,
                        Lead.Fields.alt_TreatmentStatusId
                    }
                );

                Lead leadToUpdate = new Lead()
                {
                    Id = mergedPhoneCall.RegardingObjectId.Id,
                };

                int? dialingAttemptsCount = retrievedLead.alt_TotalPhoneCallsInt != null ?
                    retrievedLead.alt_TotalPhoneCallsInt + 1 : 1;
                leadToUpdate.Attributes.Add(Lead.Fields.alt_TotalPhoneCallsInt, dialingAttemptsCount);

                if (mergedPhoneCall.alt_StatusCode != null)
                {
                    if (retrievedLead.alt_LastPhoneCallStatusCode == null || retrievedLead.alt_LastPhoneCallStatusCode.Value != mergedPhoneCall.alt_StatusCode.Value)
                    {
                        leadToUpdate.Attributes.Add(Lead.Fields.alt_LastPhoneCallStatusCode, mergedPhoneCall.alt_StatusCode);
                    }

                    if (mergedPhoneCall.alt_StatusCode.Value == (int)CallStatusCode.NoAnswer)
                    {
                        int? missedCalls = retrievedLead.alt_TotalMissedPhoneCallsInt != null ?
                            retrievedLead.alt_TotalMissedPhoneCallsInt + 1 : 1;
                        leadToUpdate.Attributes.Add(Lead.Fields.alt_TotalMissedPhoneCallsInt, missedCalls);

                        if (targetPhoneCall.AttributeHasValue<OptionSetValue>(PhoneCall.Fields.alt_SourceSystemCode)
                            && targetPhoneCall.alt_SourceSystemCode.Value == (int)SourceSystemCode.IVR)
                        {
                            int? missedCallsFromIVR = retrievedLead.alt_TotalIVRMissedPhoneCallsInt != null ?
                                retrievedLead.alt_TotalIVRMissedPhoneCallsInt + 1 : 1;
                            leadToUpdate.Attributes.Add(Lead.Fields.alt_TotalIVRMissedPhoneCallsInt, missedCallsFromIVR);

                            int? missedCallsFromIVRToday = retrievedLead.alt_TotalMissedPhoneCallsTodayInt != null ?
                                retrievedLead.alt_TotalMissedPhoneCallsTodayInt + 1 : 1;
                            leadToUpdate.Attributes.Add(Lead.Fields.alt_TotalMissedPhoneCallsTodayInt, missedCallsFromIVRToday);

                            if (retrievedLead.AttributeHasValue<EntityReference>(Lead.Fields.alt_TreatmentStatusId))
                            {
                                var assignedToRepresentitiveTreatmentStatusCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>(assignedToRepresentitiveTreatmentStatusCodeGlobalParameterName);
                                TreatmentStatusDAL treatmentStatusDAL = new TreatmentStatusDAL(this.GlobalContext);
                                alt_TreatmentStatus retrievedTreatmentStatus =
                                    treatmentStatusDAL.GetActiveByAttribute(
                                        alt_TreatmentStatus.Fields.Id,
                                        retrievedLead.alt_TreatmentStatusId.Id,
                                        new string[] { alt_TreatmentStatus.Fields.alt_CodeInt }).FirstOrDefault();

                                this.GlobalContext.Log.Info("retrieved treatment status code: " + retrievedTreatmentStatus.alt_CodeInt.Value);

                                if (retrievedTreatmentStatus.alt_CodeInt.Value != assignedToRepresentitiveTreatmentStatusCode)
                                {
                                    var noAnswerTreatmentStatusCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>(noAnswerTreatmentStatusCodeGlobalParameterName);
                                    alt_TreatmentStatus noAnswerTreatmentStatus = treatmentStatusDAL.GetByCode(noAnswerTreatmentStatusCode);
                                    leadToUpdate.Attributes.Add(Lead.Fields.alt_TreatmentStatusId, noAnswerTreatmentStatus.ToEntityReference());
                                }
                            }
                            else
                            {
                                TreatmentStatusDAL treatmentStatusDAL = new TreatmentStatusDAL(this.GlobalContext);
                                var noAnswerTreatmentStatusCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>(noAnswerTreatmentStatusCodeGlobalParameterName);
                                alt_TreatmentStatus noAnswerTreatmentStatus = treatmentStatusDAL.GetByCode(noAnswerTreatmentStatusCode);
                                leadToUpdate.Attributes.Add(Lead.Fields.alt_TreatmentStatusId, noAnswerTreatmentStatus.ToEntityReference());
                            }
                        }
                    }
                    else
                    {
                        int? answeredCalls = retrievedLead.alt_TotalPhoneCallsAnsweredInt != null ?
                            retrievedLead.alt_TotalPhoneCallsAnsweredInt + 1 : 1;
                        leadToUpdate.Attributes.Add(Lead.Fields.alt_TotalPhoneCallsAnsweredInt, answeredCalls);

                        if (mergedPhoneCall.alt_StatusCode.Value == (int)CallStatusCode.CustomerNotInterested)
                        {
                            leadToUpdate.Attributes.Add(Lead.Fields.StatusCode, mergedPhoneCall.alt_DisqualificationReasonCode);
                        }
                    }
                }

                if (leadToUpdate.Attributes.Any())
                {
                    leadDAL.Update(leadToUpdate);
                }
            }
        }

        public void HandlePhoneCallsCountForAndScheduledTreatmentDateRelatedLeadOrOpportunity(PhoneCall targetPhoneCall)
        {
            this.GlobalContext.LogEntry();

            if (targetPhoneCall.AttributeHasValue<EntityReference>(PhoneCall.Fields.RegardingObjectId)
                && (targetPhoneCall.RegardingObjectId.LogicalName == Lead.EntityLogicalName
                    || targetPhoneCall.RegardingObjectId.LogicalName == Opportunity.EntityLogicalName))
            {
                Entity regardingObject = new Entity(targetPhoneCall.RegardingObjectId.LogicalName, targetPhoneCall.RegardingObjectId.Id);
                ActivityBL activityBL = new ActivityBL(this.GlobalContext);

                this.SetPhoneCallsCount(regardingObject, targetPhoneCall);

                if (targetPhoneCall.AttributeHasValue<DateTime?>(PhoneCall.Fields.ScheduledEnd)
                    && targetPhoneCall.RegardingObjectId.LogicalName == Lead.EntityLogicalName)
                {
                    activityBL.SetLeadScheduledTreatmentDate(regardingObject, targetPhoneCall);
                }
                activityBL.UpdateRegardingObject(regardingObject);
            }
        }

        public void HandleLeadScheduledTreatmentDate(PhoneCall targetPhoneCall, PhoneCall prePhoneCall)
        {
            this.GlobalContext.LogEntry();

            PhoneCall mergedPhoneCall = prePhoneCall != null ?
                targetPhoneCall.Merge(prePhoneCall) : targetPhoneCall;

            if (targetPhoneCall.AttributeHasValue<DateTime?>(PhoneCall.Fields.ScheduledEnd)
                && mergedPhoneCall.AttributeHasValue<EntityReference>(PhoneCall.Fields.RegardingObjectId)
                && mergedPhoneCall.RegardingObjectId.LogicalName == Lead.EntityLogicalName)
            {
                Entity regardingObject = new Entity(mergedPhoneCall.RegardingObjectId.LogicalName, mergedPhoneCall.RegardingObjectId.Id);
                ActivityBL activityBL = new ActivityBL(this.GlobalContext);

                activityBL.SetLeadScheduledTreatmentDate(regardingObject, targetPhoneCall);
                activityBL.UpdateRegardingObject(regardingObject);
            }
        }

        private void SetPhoneCallsCount(Entity regardingObject, PhoneCall targetPhoneCall)
        {
            this.GlobalContext.LogEntry();

            PhoneCallDAL phoneCallDAL = new PhoneCallDAL(GlobalContext);
            List<PhoneCall> retrievedPhoneCalls = phoneCallDAL.GetPhoneCallsByRegardingIdOrderByScheduledEndAscending(targetPhoneCall.RegardingObjectId.Id);

            int? phoneCallsCount = retrievedPhoneCalls?.Count > 0 ? retrievedPhoneCalls?.Count : null;
            this.GlobalContext.Log.Info($"retrievedPhoneCallsCount: {phoneCallsCount}");

            regardingObject[Lead.Fields.alt_CallCountInt] = phoneCallsCount; ;
        }
    }
}