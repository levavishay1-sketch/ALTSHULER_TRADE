using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class PhoneCallDAL : CrmBaseDAL<PhoneCall>
    {
        public PhoneCallDAL(GlobalContext globalContext) : base(globalContext, PhoneCall.EntityLogicalName)
        {
        }

        public List<PhoneCall> GetPhoneCallsByRegardingIdOrderByScheduledEndAscending(Guid regardingId)
        {
            GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression(PhoneCall.EntityLogicalName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(PhoneCall.Fields.StateCode ,PhoneCall.Fields.ScheduledEnd),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(PhoneCall.Fields.RegardingObjectId, ConditionOperator.Equal, regardingId)
                    }
                },
                Orders =
                {
                    new OrderExpression(PhoneCall.Fields.ScheduledEnd, OrderType.Ascending)
                }
            };

            List<PhoneCall> retrievedPhoneCalls = GetMultiple(query);
            return retrievedPhoneCalls;
        }
    }
}