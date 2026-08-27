using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.Framework;
using System;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class CityBL : ExternalBLBase
    {
        public CityBL(GlobalContext globalContext) : base(globalContext) { }

        internal ActionResult HandleCitiesSynchronization(ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            return new GovernmentDataBL(this.GlobalContext)
                .HandleGovernmentData<ApiCity>(GovernmentDataTypeCode.Cities, retrievedSchedulerSetup);
        }
    }
}
