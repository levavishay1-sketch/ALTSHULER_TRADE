using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;
using System.Collections.Generic;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class LeadBL : ExternalBLBase
    {
        private string defaultOwnerTeamCodeParameterName = "DefaultOwnerTeamCode";

        public LeadBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult HandleCreateLead(ApiLead apiLead)
        {
            this.GlobalContext.LogEntry();

            ActionResult apiActionResult = new ActionResult();
            this.HandleLeadFullName(apiLead);
            this.HandleDefaultOwner(apiLead);

            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            Guid id = leadDal.Create(apiLead, true);

            if (apiLead.LeadSourceCode.Value == (int)LeadSourceCode.DigitalForm)
            {
                ApiLead retrievedLead;
                var isDisqualifyDuplicatedLeads = this.GlobalContext.CacheManager.GetGlobalParameter<bool>("DisqualifyDuplicatedLeads");
                if (isDisqualifyDuplicatedLeads)
                {
                    retrievedLead = leadDal.GetRelevantLeadByMobilePhone(apiLead.MobilePhone);
                }
                else
                {
                    retrievedLead = leadDal.Get(id, new string[] { "alt_leadidentitynumber" });
                }
                apiActionResult.ReturnObject = new ApiLead() { LeadIdentityNumber = retrievedLead.LeadIdentityNumber };
            }
            else
            {
                apiActionResult.ReturnObject = new ApiLead() { Id = id };
            }

            return apiActionResult;
        }

        public ActionResult HandleUpdateLead(ApiLead leadApi)
        {
            this.GlobalContext.LogEntry();

            ActionResult apiActionResult = new ActionResult();
            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            leadDal.Update(leadApi);

            return apiActionResult;
        }

        public ActionResult HandleLeadsSynchronizationToIVR(ApiScheduledOperation scheduledOperation, ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            base.GetAndSetApiConfiguration((int)ApiConfigurationCode.LeadsImportToIVR);
            int defaultOwnerTeamCode = this.GlobalContext.CacheManager.GetGlobalParameter<int>(defaultOwnerTeamCodeParameterName);

            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            List<ApiLead> retrievedLeadsForIVR = leadDal.GetLeadsForIVR(defaultOwnerTeamCode);
            if (retrievedLeadsForIVR.Count == 0)
            {
                actionResult.IsSuccess = true;
                actionResult.ReturnObject = "לא נמצאו לידים מתאימים";
            }
            else
            {
                actionResult = new ESBLeadsForIVRDAL(this.GlobalContext, this.ApiConfiguration).SetLeads(retrievedLeadsForIVR);
                if (actionResult.IsSuccess)
                {
                    var response = base.GetDeserializedContent<ESBLeadsForIVRResponse>(actionResult.ReturnObject.ToString());
                    IVRBL iVRBL = new IVRBL(this.GlobalContext);
                    actionResult = iVRBL.HandleIVRResponse(response);
                }
            }

            return actionResult;
        }

        public ActionResult HandleClearTotalMissedPhoneCallsTodayFromLeads(ApiScheduledOperation scheduledOperation, ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            LeadDAL leadDal = new LeadDAL(this.GlobalContext);
            List<ApiLead> retrievedLeads = leadDal.GetLeadsWithTotalMissedPhoneCallsToday();

            List<ApiLead> leadsToUpdate = new List<ApiLead>();
            foreach (ApiLead lead in retrievedLeads)
            {
                leadsToUpdate.Add(new ApiLead()
                {
                    Id = lead.Id,
                    TotalMissedPhoneCallsTodayInt = null
                });
            }

            actionResult = leadDal.ExecuteMultipleRequestsInChunks(leadsToUpdate, RequestType.Update);

            return actionResult;
        }

        private void HandleDefaultOwner(ApiLead apiLead)
        {
            this.GlobalContext.LogEntry();

            // Need to Implement logic for default owner per product.
            base.HandleDefaultOwner<ApiTeam>(apiLead, "tradeDefaultOwner");
        }

        private void HandleLeadFullName(ApiLead leadApi)
        {
            this.GlobalContext.LogEntry();

            if (string.IsNullOrWhiteSpace(leadApi.FirstName)
                && string.IsNullOrWhiteSpace(leadApi.LastName)
                && !string.IsNullOrWhiteSpace(leadApi.FullName))
            {
                leadApi.FirstName = leadApi.FullName;
            }
        }
    }
}
