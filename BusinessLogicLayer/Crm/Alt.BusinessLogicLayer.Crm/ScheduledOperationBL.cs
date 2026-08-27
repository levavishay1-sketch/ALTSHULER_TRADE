using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm
{
    public class ScheduledOperationBL : CrmBaseBL
    {
        public ScheduledOperationBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void Validate(alt_ScheduledOperation targetScheduledOperation)
        {
            this.GlobalContext.LogEntry();
            if (!targetScheduledOperation.AttributeHasValue<EntityReference>(alt_ScheduledOperation.Fields.alt_SchedulerSetupId)
              && !targetScheduledOperation.AttributeHasValue<int?>(alt_ScheduledOperation.Fields.alt_SchedulerSetupCodeInt))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.SchedulerSetupNotFound, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.SchedulerSetupNotFound));
            }
        }

        public void HandleScheduledOperationStatusCode(alt_ScheduledOperation targetScheduledOperation)
        {
            this.GlobalContext.LogEntry();
            if (targetScheduledOperation.Contains(alt_ScheduledOperation.Fields.StatusCode)
                && targetScheduledOperation.StatusCode.Value == (int)ScheduledOperationStatusCode.Run)
            {
                targetScheduledOperation.StatusCode.Value = (int)ScheduledOperationStatusCode.Running;
            }
        }

        public void SetScheduledOperationName(alt_ScheduledOperation targetScheduledOperation)
        {
            this.GlobalContext.LogEntry();
            if (!targetScheduledOperation.AttributeHasValue<string>(alt_ScheduledOperation.Fields.alt_Name))
            {
                string isAutomatic = (bool)targetScheduledOperation.alt_IsAutomaticBit ? "אוט'" : "ידנית";
                string setupName = targetScheduledOperation.alt_SchedulerSetupId.Name;

                if (string.IsNullOrWhiteSpace(setupName))
                {
                    SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
                    setupName = schedulerSetupDal.Get(targetScheduledOperation.alt_SchedulerSetupId.Id, new[] { "alt_name" }).alt_Name;
                }

                targetScheduledOperation.alt_Name = $"פעולה {setupName} - {isAutomatic}";
            }
        }

        public void HandleScheduledOperationData(alt_ScheduledOperation targetScheduledOperation)
        {
            this.GlobalContext.LogEntry();

            if (targetScheduledOperation.AttributeHasValue<EntityReference>(alt_ScheduledOperation.Fields.alt_SchedulerSetupId)
                || targetScheduledOperation.AttributeHasValue<int?>(alt_ScheduledOperation.Fields.alt_SchedulerSetupCodeInt))
            {
                SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
                alt_SchedulerSetup retrievedSchedulerSetup = targetScheduledOperation.AttributeHasValue<EntityReference>(alt_ScheduledOperation.Fields.alt_SchedulerSetupId) ?
                    schedulerSetupDal.GetSchedulerSetupDetails(targetScheduledOperation.alt_SchedulerSetupId.Id) :
                    schedulerSetupDal.GetSchedulerSetupDetailsByCode(targetScheduledOperation.alt_SchedulerSetupCodeInt.Value);

                if (!targetScheduledOperation.AttributeHasValue<int?>(alt_ScheduledOperation.Fields.alt_SchedulerSetupCodeInt))
                {
                    targetScheduledOperation.alt_SchedulerSetupCodeInt = retrievedSchedulerSetup.alt_CodeInt;
                }
                else if (!targetScheduledOperation.AttributeHasValue<EntityReference>(alt_ScheduledOperation.Fields.alt_SchedulerSetupId))
                {
                    targetScheduledOperation.alt_SchedulerSetupId = retrievedSchedulerSetup.ToEntityReference();
                }
                HandleOperationStatusBasedOnSetupCurrentStatus(targetScheduledOperation, targetScheduledOperation, retrievedSchedulerSetup);
            }
        }

        public void SetScheduledOperationOperationStartTime(alt_ScheduledOperation targetScheduledOperation)
        {
            this.GlobalContext.LogEntry();

            if (targetScheduledOperation.AttributeHasValue<OptionSetValue>(alt_ScheduledOperation.Fields.StatusCode)
                && targetScheduledOperation.StatusCode.Value == (int)ScheduledOperationStatusCode.Running
                && !targetScheduledOperation.AttributeHasValue<DateTime>(alt_ScheduledOperation.Fields.alt_OperationStartDate))
            {
                targetScheduledOperation.alt_OperationStartDate = targetScheduledOperation.ModifiedOn;
            }
        }

        public void SetScheduledOperationOperationRunTime(alt_ScheduledOperation targetScheduledOperation, alt_ScheduledOperation preScheduledOperation)
        {
            this.GlobalContext.LogEntry();

            if (targetScheduledOperation.AttributeHasValue<OptionSetValue>(alt_ScheduledOperation.Fields.StatusCode)
                && (targetScheduledOperation.StatusCode.Value == (int)ScheduledOperationStatusCode.Failed
                    || targetScheduledOperation.StatusCode.Value == (int)ScheduledOperationStatusCode.FinishedSuccessfully)
                && !targetScheduledOperation.AttributeHasValue<DateTime>(alt_ScheduledOperation.Fields.alt_OperationRunTime)
                && preScheduledOperation.alt_SchedulerSetupId != null 
                && preScheduledOperation.alt_OperationStartDate != null)
            {
                SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
                var retrievedSchedulerSetup = schedulerSetupDal.Get(preScheduledOperation.alt_SchedulerSetupId.Id, new string[]
                        {
                            alt_SchedulerSetup.Fields.alt_SetupTypeCode
                        });
                if (retrievedSchedulerSetup.alt_SetupTypeCode.Value == (int)SchedulerSetupTypeCode.OperationTrigger)
                {
                    targetScheduledOperation.alt_OperationRunTime = (targetScheduledOperation.ModifiedOn - preScheduledOperation.alt_OperationStartDate).Value.ToString(@"dd\.hh\:mm\:ss\.f");
                }
            }
        }

        public void HanldeSetupStatusChangeBasedOnOperationStatus(alt_ScheduledOperation targetScheduledOperation, alt_ScheduledOperation preScheduledOperation)
        {
            this.GlobalContext.LogEntry();

            if (targetScheduledOperation.AttributeHasValue<OptionSetValue>(alt_ScheduledOperation.Fields.StatusCode))
            {
                alt_ScheduledOperation mergedScheduledOperation = targetScheduledOperation.Equals(preScheduledOperation) ?
                    targetScheduledOperation : targetScheduledOperation.Merge(preScheduledOperation);

                alt_SchedulerSetup schedulerSetupToUpdate = new alt_SchedulerSetup() { Id = mergedScheduledOperation.alt_SchedulerSetupId.Id };
                ScheduledOperationStatusCode scheduledOperationStatusCode = (ScheduledOperationStatusCode)targetScheduledOperation.StatusCode.Value;
                switch (scheduledOperationStatusCode)
                {
                    case ScheduledOperationStatusCode.Running:
                        {
                            schedulerSetupToUpdate.StatusCode = new OptionSetValue((int)SchedulerSetupStatusCode.WaitingForOperationToFinish);
                            schedulerSetupToUpdate.alt_CurrentScheduledOperationStatusCode = new OptionSetValue((int)CurrentScheduledOperationStatusCode.Running);
                            schedulerSetupToUpdate.alt_OperationStartDate = mergedScheduledOperation.alt_OperationStartDate;

                            break;
                        }
                    case ScheduledOperationStatusCode.Failed:
                    case ScheduledOperationStatusCode.Canceled:
                        {
                            schedulerSetupToUpdate.StatusCode = new OptionSetValue((int)SchedulerSetupStatusCode.Active);
                            schedulerSetupToUpdate.alt_CurrentScheduledOperationStatusCode = new OptionSetValue((int)CurrentScheduledOperationStatusCode.Failed);
                            break;
                        }
                    case ScheduledOperationStatusCode.FinishedSuccessfully:
                        {
                            schedulerSetupToUpdate.StatusCode = new OptionSetValue((int)SchedulerSetupStatusCode.Active);
                            schedulerSetupToUpdate.alt_CurrentScheduledOperationStatusCode = new OptionSetValue((int)CurrentScheduledOperationStatusCode.Finished);
                            break;
                        }
                    default:
                        {
                            schedulerSetupToUpdate = null;
                            break;
                        }
                }

                if (schedulerSetupToUpdate != null)
                {
                    SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
                    schedulerSetupDal.Update(schedulerSetupToUpdate);
                }
            }
        }

        public void HandleOperationStatusBasedOnSetupCurrentStatus(alt_ScheduledOperation targetScheduledOperation, alt_ScheduledOperation preScheduledOperation, alt_SchedulerSetup retrievedSchedulerSetup = null)
        {
            this.GlobalContext.LogEntry();
            if (targetScheduledOperation.StatusCode?.Value == (int)ScheduledOperationStatusCode.Run)
            {
                alt_ScheduledOperation mergedScheduledOperation = targetScheduledOperation.Merge(preScheduledOperation);
                if (retrievedSchedulerSetup == null)
                {
                    SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
                    retrievedSchedulerSetup = schedulerSetupDal.GetSchedulerSetupDetails(mergedScheduledOperation.alt_SchedulerSetupId.Id);
                }
                if (retrievedSchedulerSetup?.alt_CurrentScheduledOperationStatusCode?.Value == (int)CurrentScheduledOperationStatusCode.Running)
                {
                    targetScheduledOperation.StatusCode = new OptionSetValue((int)ScheduledOperationStatusCode.Canceled);
                }
                //else if (mergedScheduledOperation.alt_IsAutomaticBit.Value
                //        && retrievedSchedulerSetup.AttributeHasValue<int?>(alt_SchedulerSetup.Fields.alt_TimeBetweenOperationsInt)
                //        && retrievedSchedulerSetup.AttributeHasValue<int?>(alt_SchedulerSetup.Fields.alt_MaxAttemptsBetweenOperationsInt)
                //        && retrievedSchedulerSetup.AttributeHasValue<DateTime>(alt_SchedulerSetup.Fields.alt_OperationStartDate))
                //{
                //    DateTime scheduledOperationRunTime = targetScheduledOperation.ModifiedOn ?? DateTime.UtcNow;
                //    double maxMinutesSinceLastStart = (double)(retrievedSchedulerSetup.alt_TimeBetweenOperationsInt * retrievedSchedulerSetup.alt_MaxAttemptsBetweenOperationsInt);
                //    DateTime lastRunTime = retrievedSchedulerSetup.alt_OperationStartDate.Value.ConvertIsraelTimeToUTC().AddMinutes(maxMinutesSinceLastStart);

                //    if (DateTime.Parse(lastRunTime.ToString("MM/dd/yyyy HH:mm")) > DateTime.Parse(scheduledOperationRunTime.ToString("MM/dd/yyyy HH:mm")))
                //    {
                //        targetScheduledOperation.StatusCode = new OptionSetValue((int)ScheduledOperationStatusCode.Canceled);
                //    }
                //}
            }
        }

        public void HandleScheduledOperationStateCode(alt_ScheduledOperation targetScheduledOperation)
        {
            this.GlobalContext.LogEntry();
            if (targetScheduledOperation.AttributeHasValue<OptionSetValue>(alt_ScheduledOperation.Fields.StatusCode))
            {
                ScheduledOperationStatusCode scheduledOperationStatusCode = (ScheduledOperationStatusCode)targetScheduledOperation.StatusCode.Value;
                switch (scheduledOperationStatusCode)
                {
                    case ScheduledOperationStatusCode.Draft:
                    case ScheduledOperationStatusCode.Run:
                    case ScheduledOperationStatusCode.Running:
                        {
                            targetScheduledOperation.StateCode = alt_ScheduledOperationState.Active;
                            break;
                        }
                    case ScheduledOperationStatusCode.Canceled:
                    case ScheduledOperationStatusCode.Failed:
                    case ScheduledOperationStatusCode.FinishedSuccessfully:
                        {
                            targetScheduledOperation.StateCode = alt_ScheduledOperationState.Inactive;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}
