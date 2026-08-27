using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.Framework;
using System;
using System.Collections.Generic;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class CountryBL : ExternalBLBase
    {
        public CountryBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult Get()
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            CountryDAL countryDal = new CountryDAL(this.GlobalContext);
            List<ApiCountry> countries = countryDal.GetAll();
            actionResult.ReturnObject = countries;

            return actionResult;
        }

        internal ActionResult HandleCountriesSynchronization(ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            return new GovernmentDataBL(this.GlobalContext)
                .HandleGovernmentData<ApiCountry>(GovernmentDataTypeCode.Countries, retrievedSchedulerSetup);
        }
    }
}
