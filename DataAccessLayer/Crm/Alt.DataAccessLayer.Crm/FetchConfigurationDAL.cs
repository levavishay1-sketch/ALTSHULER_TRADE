using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class FetchConfigurationDAL : CrmBaseDAL<alt_FetchConfiguration>
    {
        private string[] attributesForRetrieve =
        {
            alt_FetchConfiguration.Fields.alt_TargetEntityName,
            alt_FetchConfiguration.Fields.alt_TargetEntitySchemaName,
            alt_FetchConfiguration.Fields.alt_FetchXml
        };

        public FetchConfigurationDAL(GlobalContext globalContext) : base(globalContext, alt_FetchConfiguration.EntityLogicalName) { }

        public List<alt_FetchConfiguration> GetFetchConfigurationsByFilterFieldAndEntryPoint(Guid entryPointId, string searchField)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_FetchConfiguration.EntityLogicalName,
                ColumnSet = new ColumnSet(attributesForRetrieve)
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression(alt_FetchConfiguration.Fields.alt_SearchEntryPointConfigurationId, ConditionOperator.Equal, entryPointId));
            filter.Conditions.Add(new ConditionExpression(alt_FetchConfiguration.Fields.alt_FilterByField, ConditionOperator.Equal, searchField));
            filter.Conditions.Add(new ConditionExpression(alt_FetchConfiguration.Fields.StateCode, ConditionOperator.Equal, 0));

            query.Criteria.AddFilter(filter);

            return this.GetMultipleWithPaging(query);
        }
    }
}
