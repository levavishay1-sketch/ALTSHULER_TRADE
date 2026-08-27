using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class ActivityBL : CrmBaseBL
    {
        public ActivityBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetOwnerAccordingToCallingUser(Entity entity)
        {
            this.GlobalContext.LogEntry();

            SystemUserBL systemUserBl = new SystemUserBL(this.GlobalContext);
            if (systemUserBl.IsCallingUserApplicationUser())
            {
                this.SetActivityOwnerByRegardingObject(entity);
            }
        }

        public void SetActivityOwnerByRegardingObject(Entity activity)
        {
            this.GlobalContext.LogEntry();

            EntityReference regardingObjectId = activity.GetAttributeValue<EntityReference>(ActivityPointer.Fields.RegardingObjectId);
            if (regardingObjectId != null)
            {
                CommonDAL commonDal = new CommonDAL(this.GlobalContext, regardingObjectId.LogicalName);
                Entity regardingObject = commonDal.Get(regardingObjectId.Id, new string[] { ActivityPointer.Fields.OwnerId });
                EntityReference ownerId = regardingObject.GetAttributeValue<EntityReference>(ActivityPointer.Fields.OwnerId);
                activity[ActivityPointer.Fields.OwnerId] = ownerId;
            }
        }

        public void SetLeadScheduledTreatmentDate(Entity regardingObject, Entity targetActivity)
        {
            this.GlobalContext.LogEntry();

            LeadDAL leadDAL = new LeadDAL(this.GlobalContext);
            Lead relatedLead = leadDAL.Get(regardingObject.Id, new string[] { Lead.Fields.alt_TargetScheduledTreatmentDate });

            var scheduledEnd = targetActivity.GetAttributeValue<DateTime?>(ActivityPointer.Fields.ScheduledEnd);
            if (scheduledEnd.HasValue
                && (relatedLead.alt_TargetScheduledTreatmentDate == null
                    || scheduledEnd.Value > relatedLead.alt_TargetScheduledTreatmentDate.Value))
            {
                regardingObject[Lead.Fields.alt_TargetScheduledTreatmentDate] = scheduledEnd.Value;
            }
        }

        public void UpdateRegardingObject(Entity regardingObject)
        {
            this.GlobalContext.LogEntry();

            if (regardingObject.Attributes.Any())
            {
                CommonDAL commonDAL = new CommonDAL(GlobalContext, regardingObject.LogicalName);
                commonDAL.Update(regardingObject);
            }
        }

        public void CloseActivitiesOnRegardingObjectStateCode(Entity regardingObject, int stateCodeValue)
        {
            this.GlobalContext.LogEntry();

            var activityTypeCodes = new string[] { PhoneCall.EntityLogicalName, Task.EntityLogicalName, Appointment.EntityLogicalName };
            ActivityCommonDAL activityCommonDAL = new ActivityCommonDAL(this.GlobalContext, null);
            List<ActivityPointer> activities = activityCommonDAL.GetActivitiesByRegardingObject(regardingObject, activityTypeCodes);
            if (activities != null && activities.Count > 0)
            {
                switch (regardingObject.LogicalName)
                {
                    case Lead.EntityLogicalName:
                        {
                            this.CloseLeadActivities(stateCodeValue, activities, activityCommonDAL);
                            break;
                        }
                    case Opportunity.EntityLogicalName:
                        {
                            this.CloseOpportunityActivities(stateCodeValue, activities, activityCommonDAL);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void CloseLeadActivities(int leadStateCodeValue, List<ActivityPointer> activities, ActivityCommonDAL activityCommonDAL)
        {
            this.GlobalContext.LogEntry();
            foreach (var activity in activities)
            {
                ActivityPointerState activityPointerState = leadStateCodeValue == (int)LeadState.Disqualified ?
                    ActivityPointerState.Canceled : ActivityPointerState.Completed;
                SetActivityStatusCodeByStateCode(activity, (int)activityPointerState, activityCommonDAL);
            }
        }

        private void CloseOpportunityActivities(int opportunityStateCodeValue, List<ActivityPointer> activities, ActivityCommonDAL activityCommonDAL)
        {
            this.GlobalContext.LogEntry();
            foreach (var activity in activities)
            {
                ActivityPointerState activityPointerState = opportunityStateCodeValue == (int)OpportunityState.Lost ?
                    ActivityPointerState.Canceled : ActivityPointerState.Completed;
                SetActivityStatusCodeByStateCode(activity, (int)activityPointerState, activityCommonDAL);
            }
        }

        private void SetActivityStatusCodeByStateCode(ActivityPointer activity, int activityPointerStateCode, ActivityCommonDAL activityCommonDAL)
        {
            this.GlobalContext.LogEntry();

            string activityTypeCode = activity["activitytypecode"].ToString();
            switch (activityTypeCode)
            {
                case Task.EntityLogicalName:
                    {
                        if (activityPointerStateCode == (int)ActivityPointerState.Completed)
                            activityCommonDAL.SetActivityState(activity, activityPointerStateCode, (int)TaskStatusCode.Completed);
                        if (activityPointerStateCode == (int)ActivityPointerState.Canceled)
                            activityCommonDAL.SetActivityState(activity, activityPointerStateCode, (int)TaskStatusCode.Canceled);
                        break;
                    }
                case PhoneCall.EntityLogicalName:
                    {
                        if (activityPointerStateCode == (int)ActivityPointerState.Completed)
                            activityCommonDAL.SetActivityState(activity, activityPointerStateCode, (int)PhoneCallStatusCode.Made);
                        if (activityPointerStateCode == (int)ActivityPointerState.Canceled)
                            activityCommonDAL.SetActivityState(activity, activityPointerStateCode, (int)PhoneCallStatusCode.Canceled);
                        break;
                    }
                case Appointment.EntityLogicalName:
                    {
                        if (activityPointerStateCode == (int)ActivityPointerState.Completed)
                            activityCommonDAL.SetActivityState(activity, activityPointerStateCode, (int)AppointmentStatusCode.Completed);
                        if (activityPointerStateCode == (int)ActivityPointerState.Canceled)
                            activityCommonDAL.SetActivityState(activity, activityPointerStateCode, (int)AppointmentStatusCode.Canceled);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
