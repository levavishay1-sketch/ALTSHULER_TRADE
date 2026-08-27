using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.Framework;
using System;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class BranchBL : ExternalBLBase
    {
        public BranchBL(GlobalContext globalContext) : base(globalContext) { }

        internal ActionResult HandleBranchesSynchronization(ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            return new GovernmentDataBL(this.GlobalContext)
                .HandleGovernmentData<ApiBranch>(GovernmentDataTypeCode.Branches, retrievedSchedulerSetup);
        }
    }
}
