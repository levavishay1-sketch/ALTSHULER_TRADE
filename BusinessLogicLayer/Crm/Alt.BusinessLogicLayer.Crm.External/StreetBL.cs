using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.Framework;
using System;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class StreetBL : ExternalBLBase
    {
        public StreetBL(GlobalContext globalContext) : base(globalContext) { }

        internal ActionResult HandleStreetsSynchronization(ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            return new GovernmentDataBL(this.GlobalContext)
                .HandleGovernmentData<ApiStreet>(GovernmentDataTypeCode.Streets, retrievedSchedulerSetup);
        }
    }
}
