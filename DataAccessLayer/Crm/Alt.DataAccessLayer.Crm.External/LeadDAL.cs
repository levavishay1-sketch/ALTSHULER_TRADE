using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class LeadDAL : CrmExternalBaseDAL<ApiLead>
    {
        public LeadDAL(GlobalContext globalContext) : base(globalContext, ApiLead.EntityLogicalName) { }

        public ApiLead GetRelevantLeadByMobilePhone(string mobilePhone)
        {
            this.GlobalContext.LogEntry();

            var openedLead = this.GetOpenedLeadsByMobilePhone(mobilePhone).FirstOrDefault();
            var qualifiedLead = this.GetQualifiedLeadsWithOpenedDigitalForm(mobilePhone).FirstOrDefault();
            return qualifiedLead?.CreatedOn.Value < openedLead?.CreatedOn.Value ?
                qualifiedLead : openedLead;
        }

        public List<ApiLead> GetOpenedLeadsByMobilePhone(string mobilePhone)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = ApiLead.EntityLogicalName,
                ColumnSet = new ColumnSet("alt_leadidentitynumber", "createdon"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = {
                        new ConditionExpression("mobilephone", ConditionOperator.Equal, mobilePhone),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    },

                },
            };
            query.NoLock = true;
            query.AddOrder("createdon", OrderType.Ascending);
            return this.GetMultiple(query);
        }

        public List<ApiLead> GetQualifiedLeadsWithOpenedDigitalForm(string mobilePhone)
        {
            this.GlobalContext.LogEntry();

            var query = new QueryExpression(ApiLead.EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet("alt_leadidentitynumber", "createdon"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("mobilephone", ConditionOperator.Equal, mobilePhone),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 1)
                    }
                },
                Orders =
                {
                    new OrderExpression("createdon", OrderType.Ascending)
                },
                LinkEntities =
                {
                    new LinkEntity("lead", "alt_digitalform", "leadid", "regardingobjectid", JoinOperator.Inner)
                    {
                        EntityAlias = "aa",
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                            }
                        }
                    }
                },
                NoLock = true
            };
            return this.GetMultiple(query);
        }

        public List<ApiLead> GetLeadsForIVR(int defaultOwnerTeamCode)
        {
            this.GlobalContext.LogEntry();

            var query = new QueryExpression(this.entityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("statuscode", ConditionOperator.In, new object[] { 1, 2 }),
                        new ConditionExpression("createdon", ConditionOperator.OlderThanXHours, 2),
                    },
                    Filters =
                    {
                        new FilterExpression(LogicalOperator.Or)
                        {
                            Conditions =
                            {
                                new ConditionExpression("alt_senttoivrbit", ConditionOperator.Equal, false),
                                new ConditionExpression("alt_senttoivrbit", ConditionOperator.Null)
                            }
                        }
                    }
                },
                Orders =
                {
                    new OrderExpression("createdon", OrderType.Ascending)
                }
            };

            var teamLink = query.AddLink("team", "ownerid", "teamid", JoinOperator.Inner);
            teamLink.LinkCriteria.AddCondition("alt_teamcodeint", ConditionOperator.Equal, defaultOwnerTeamCode);
            return this.GetMultiple(query);
        }

        public List<ApiLead> GetLeadsWithTotalMissedPhoneCallsToday()
        {
            this.GlobalContext.LogEntry();

            var query = new QueryExpression(this.entityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("alt_totalmissedphonecallstodayint", ConditionOperator.NotNull),
                    }
                }
            };
            return this.GetMultiple(query);
        }
    }
}
