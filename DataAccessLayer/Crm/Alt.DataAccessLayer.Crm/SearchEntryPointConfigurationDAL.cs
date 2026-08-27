using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Alt.DataAccessLayer.Crm
{
    public class SearchEntryPointConfigurationDAL : CrmBaseDAL<alt_SearchEntryPointConfiguration>
    {
        public SearchEntryPointConfigurationDAL(GlobalContext globalContext) : base(globalContext, alt_SearchEntryPointConfiguration.EntityLogicalName) { }

        public alt_SearchEntryPointConfiguration GetSearchEntryPointConfiguration(string entityLogicalName, int sourceType)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_SearchEntryPointConfiguration.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_SearchEntryPointConfiguration.Fields.alt_SourceEntitySchemaName, ConditionOperator.Equal, entityLogicalName),
                            new ConditionExpression(alt_SearchEntryPointConfiguration.Fields.alt_SourceType, ConditionOperator.Equal, sourceType)
                        }
                    }
            };
            return base.GetFirstOrDefault(query);
        }
    }
}
