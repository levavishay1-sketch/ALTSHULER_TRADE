using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class RepresentativeRewardDAL : CrmBaseDAL<alt_RepresentativeReward>
    {
        public RepresentativeRewardDAL(GlobalContext globalContext) : base(globalContext, alt_RepresentativeReward.EntityLogicalName)
        {
        }

        public alt_RepresentativeReward GetActiveByRepresentativeAndRelatedRecord(EntityReference representative, EntityReference relatedRecord)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_RepresentativeReward.EntityLogicalName,
                ColumnSet = new ColumnSet(alt_RepresentativeReward.Fields.alt_RepresentativeRewardId),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_RepresentativeReward.Fields.alt_RepresentativeRewardSystemUserId, ConditionOperator.Equal, representative.Id),
                            new ConditionExpression(alt_RepresentativeReward.Fields.alt_RelatedRecordId, ConditionOperator.Equal, relatedRecord.Id),
                            new ConditionExpression(alt_RepresentativeReward.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    }
            };
            return this.GetMultiple(query).FirstOrDefault();
        }
    }
}
