using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.BusinessLogicLayer.Crm
{
    public class TaskBL : CrmBaseBL
    {
        public TaskBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleLeadScheduledTreatmentDate(Task targetTask, Task preTask = null)
        {
            this.GlobalContext.LogEntry();
            Task mergedTask = preTask != null ? targetTask.Merge(preTask) : targetTask;

            if (targetTask.AttributeHasValue<DateTime?>(Task.Fields.ScheduledEnd)
                && mergedTask.AttributeHasValue<EntityReference>(Task.Fields.RegardingObjectId)
                && mergedTask.RegardingObjectId.LogicalName == Lead.EntityLogicalName)
            {
                ActivityBL activityBL = new ActivityBL(this.GlobalContext);
                Entity regardingObject = new Entity(mergedTask.RegardingObjectId.LogicalName, mergedTask.RegardingObjectId.Id);

                activityBL.SetLeadScheduledTreatmentDate(regardingObject, targetTask);
                activityBL.UpdateRegardingObject(regardingObject);
            }
        }
    }
}
