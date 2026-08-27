using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System.Collections.Generic;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class PhoneCallBL : ExternalBLBase
    {
        private const string SubjectForUnasnweredCallsFromIVR = "שיחה יוצאת – חייגן חדשים";

        public PhoneCallBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult CreatePhoneCallsAndUpdateLeads(ApiPhoneCallList phoneCalls)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            List<ApiPhoneCall> phoneCallsToCreate = new List<ApiPhoneCall>();
            foreach (ApiPhoneCall phoneCall in phoneCalls.CallAttempts)
            {
                phoneCallsToCreate.Add(this.MapPhoneCallToCreate(phoneCall));
            }
            PhoneCallDAL phoneCallDAL = new PhoneCallDAL(this.GlobalContext);
            actionResult = phoneCallDAL.ExecuteMultipleRequestsInChunks(phoneCallsToCreate, RequestType.Create);

            return actionResult;
        }

        private ApiPhoneCall MapPhoneCallToCreate(ApiPhoneCall phoneCall)
        {
            ApiPhoneCall mappedApiPhoneCall = new ApiPhoneCall("phonecall")
            {
                RegardingObject = phoneCall.RegardingObject,
                ToActivityPartyList = new List<ApiActivityParty>
                {
                    new ApiActivityParty(ApiLead.EntityLogicalName)
                    {
                        Id = phoneCall.To.Id
                    }
                },
                FromActivityPartyList = new List<ApiActivityParty>
                {
                    new ApiActivityParty(ApiSystemUser.EntityLogicalName)
                    {
                        Id = this.GlobalContext.InitiatingUserId
                    }
                },
                ScheduledEnd = phoneCall.ScheduledEnd,
                DirectionCode = true,
                PriorityCode = (int)PriorityCode.Normal,
                Subject = SubjectForUnasnweredCallsFromIVR,
                PhoneNumber = phoneCall.PhoneNumber,
                SourceSystemCode = (int)SourceSystemCode.IVR,
                CreationMethodCode = (int)CreationMethodCode.Interface,
                PhoneStatusCode = (int)CallStatusCode.NoAnswer,
                CompleteActivity = true
            };

            return mappedApiPhoneCall;
        }
    }
}
