using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace OneTimeConsole.DataAccessLayer
{
    public class LeadDAL : CrmBaseDAL<Lead>
    {
        private string[] fieldsForInactiveLeads = new string[]
        {
            Lead.Fields.Id,
            Lead.Fields.ModifiedOn,
            Lead.Fields.StateCode,
            Lead.Fields.StatusCode
        };

        public LeadDAL(GlobalContext globalContext, string entityName = null) : base(globalContext, entityName) { }

        public List<Lead> RetrieveInactiveLeadsWithEmptyClosedOnDate()
        {
            GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression(entityLogicalName)
            {
                ColumnSet = new ColumnSet(fieldsForInactiveLeads),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(Lead.Fields.StateCode, ConditionOperator.NotEqual, (int)LeadState.Open),
                        new ConditionExpression(Lead.Fields.alt_ClosedOnDate, ConditionOperator.Null)
                    }
                }
            };

            List<Lead> retrievedLeads = GetMultipleWithPaging(query);
            GlobalContext.Log.Info($"RetrievedLeads: {retrievedLeads?.Count}");
            return retrievedLeads;
        }
    }
}
