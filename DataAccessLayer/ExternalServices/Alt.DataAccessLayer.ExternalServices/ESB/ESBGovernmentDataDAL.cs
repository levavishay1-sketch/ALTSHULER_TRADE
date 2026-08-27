using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBGovernmentDataDAL : ExternalServicesBaseDAL<ESBGovernmentDataRequest, ApiEntity>
    {
        GovernmentDataTypeCode? governmentDataType;
        DateTime fromDate;

        public ESBGovernmentDataDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration)
            : base(globalContext, apiConfiguration) { }


        public ActionResult GetGovernmentData(GovernmentDataTypeCode governmentDataTypeCode, DateTime fromDate)
        {
            this.GlobalContext.LogEntry();

            this.governmentDataType = governmentDataTypeCode;
            this.fromDate = fromDate;

            return base.Post(new ApiEntity());
        }

        protected override ESBGovernmentDataRequest MapApiEntityToTargetModel(ApiEntity apiEntity)
        {
            this.GlobalContext.LogEntry();

            ESBGovernmentDataRequest eSBGovernmentDataRequest = new ESBGovernmentDataRequest
            {
                GovernmentDataType = this.governmentDataType?.GetDescriptionAttribute(),
                FromDate = this.fromDate.ToString("yyyy-MM-dd")
            };

            return eSBGovernmentDataRequest;
        }
    }
}
