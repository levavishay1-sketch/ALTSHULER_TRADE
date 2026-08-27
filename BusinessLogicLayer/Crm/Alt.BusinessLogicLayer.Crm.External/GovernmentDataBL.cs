using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class GovernmentDataBL : ExternalBLBase
    {
        public GovernmentDataBL(GlobalContext globalContext)
            : base(globalContext) { }

        internal ActionResult HandleGovernmentData<T>(GovernmentDataTypeCode governmentDataTypeCode, ApiSchedulerSetup retrievedSchedulerSetup) where T : ApiEntity
        {
            this.GlobalContext.LogEntry();
            base.GetAndSetApiConfiguration((int)ApiConfigurationCode.RetrieveGovernmentData);

            DateTime fromDate = this.GetFromDate(retrievedSchedulerSetup);
            ActionResult actionResult = new ESBGovernmentDataDAL(this.GlobalContext, this.ApiConfiguration)
                .GetGovernmentData(governmentDataTypeCode, fromDate);

            if (actionResult.IsSuccess)
            {
                var response = base.GetDeserializedContent<ESBResponse<ESBGovernmentDataResponse<T>>>(actionResult.ReturnObject.ToString());
                if (response.ResultStatusCode == ESBResultStatusCode.Success
                    && response.ResponseData?.Data != null)
                {
                    List<ApiEntity> recordsToUpsert = response.ResponseData.Data
                        .OfType<ApiEntity>()
                        .ToList();
                    if (recordsToUpsert != null && recordsToUpsert.Count > 0)
                    {
                        this.SetDefaultValues(recordsToUpsert);
                        actionResult = new CommonDAL(this.GlobalContext, null)
                              .ExecuteMultipleRequestsInChunks(recordsToUpsert, RequestType.Upsert, 20, false);
                    }
                }
                else
                {
                    actionResult.SetToFailedActionResult(response.ErrorMessage);
                }
            }
            return actionResult;
        }

        private void SetDefaultValues(List<ApiEntity> recordsToUpsert)
        {
            this.GlobalContext.LogEntry();

            var firstRecord = recordsToUpsert[0];
            if (firstRecord is ApiBranch)
            {
                this.SetBranchDefaultValues(recordsToUpsert);
            }
        }

        private void SetBranchDefaultValues(List<ApiEntity> recordsToUpsert)
        {
            this.GlobalContext.LogEntry();

            foreach (ApiBranch branch in recordsToUpsert)
            {
                branch.Name = $"{branch.BranchNumber}-{branch.BranchName}";
                if (string.IsNullOrWhiteSpace(branch.Code))
                {
                    branch.Code = $"{branch.Bank?.Code}-{branch.BranchNumber}";
                }
            }
        }

        internal DateTime GetFromDate(ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            DateTime fromDate = DateTime.Now;
            int? lastXDays;
            if (retrievedSchedulerSetup != null
                && retrievedSchedulerSetup.TryGetSettingsItemValue<int?>(nameof(lastXDays), out lastXDays))
            {
                fromDate = DateTime.Now.AddDays(-lastXDays.Value);
            }
            return fromDate;
        }

    }
}
