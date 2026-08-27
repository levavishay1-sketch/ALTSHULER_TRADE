using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.Framework;
using System;

namespace Alt.BusinessLogicLayer.Crm.External
{
    class BankBL : ExternalBLBase
    {
        public BankBL(GlobalContext globalContext) : base(globalContext) { }

        internal ActionResult HandleBankSynchronization(ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            return new GovernmentDataBL(this.GlobalContext)
                .HandleGovernmentData<ApiBank>(GovernmentDataTypeCode.Banks, retrievedSchedulerSetup);
        }
    }
}
