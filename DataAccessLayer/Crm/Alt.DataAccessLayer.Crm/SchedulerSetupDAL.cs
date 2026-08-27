using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class SchedulerSetupDAL : CrmBaseDAL<alt_SchedulerSetup>
    {
        string[] schedulerSetupFieldsToRetrieve = new[]
               {
                    alt_SchedulerSetup.Fields.alt_CodeInt,
                    alt_SchedulerSetup.Fields.StatusCode,
                    alt_SchedulerSetup.Fields.alt_CurrentScheduledOperationStatusCode,
                    alt_SchedulerSetup.Fields.alt_MaxAttemptsBetweenOperationsInt,
                    alt_SchedulerSetup.Fields.alt_OperationStartDate,
                    alt_SchedulerSetup.Fields.alt_TimeBetweenOperationsInt
                };

        public SchedulerSetupDAL(GlobalContext globalContext)
            : base(globalContext, alt_SchedulerSetup.EntityLogicalName)
        {
        }

        public alt_SchedulerSetup GetSchedulerSetupDetailsByCode(int code, string[] attributes = null)
        {
            this.GlobalContext.LogEntry();
            return base.GetFirstActivetOrDefaultByAttribute(alt_SchedulerSetup.Fields.alt_CodeInt, code,
                attributes ?? schedulerSetupFieldsToRetrieve);
        }

        public alt_SchedulerSetup GetSchedulerSetupDetails(Guid id, string[] attributes = null)
        {
            this.GlobalContext.LogEntry();
            return base.Get(id, attributes ?? schedulerSetupFieldsToRetrieve);
        }
    }
}
