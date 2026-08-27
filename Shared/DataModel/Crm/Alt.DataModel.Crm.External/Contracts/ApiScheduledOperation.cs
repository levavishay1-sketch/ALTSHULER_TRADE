using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiScheduledOperation : ApiEntity
    {
        public const string EntityLogicalName = "alt_scheduledoperation";
        public ApiScheduledOperation() : base(EntityLogicalName) { }

        private ApiSchedulerSetup schedulerSetup;
        [CrmEntityMapper("alt_schedulersetupid", CrmPropertyType.EntityReference, mappToCrm: false)]
        public ApiSchedulerSetup SchedulerSetup
        {
            get
            {
                return this.schedulerSetup;
            }
            set
            {
                this.SetProperty(value);
                this.schedulerSetup = value;
            }
        }

        private int? schedulerSetupCode;
        [CrmEntityMapper("alt_schedulersetupcodeint", CrmPropertyType.Int, mappToCrm: false)]
        public int? SchedulerSetupCode
        {
            get
            {
                return this.schedulerSetupCode;
            }
            set
            {
                this.SetProperty(value);
                this.schedulerSetupCode = value;
            }
        }

        private DateTime? operationStartDate;
        [CrmEntityMapper("alt_operationstartdate", CrmPropertyType.DateTime)]
        public DateTime? OperationStartDate
        {
            get
            {
                return this.operationStartDate;
            }
            set
            {
                this.SetProperty(value);
                this.operationStartDate = value;
            }
        }

        private string executionresult;
        [CrmEntityMapper("alt_executionresult", CrmPropertyType.String)]
        public string ExecutionResult
        {
            get
            {
                return this.executionresult;
            }
            set
            {
                this.SetProperty(value);
                this.executionresult = value;
            }
        }

        private string name;
        [CrmEntityMapper("alt_name", CrmPropertyType.String)]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.SetProperty(value);
                this.name = value;
            }
        }
    }
}
