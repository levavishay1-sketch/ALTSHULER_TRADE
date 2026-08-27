using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class SystemUserDAL : CrmBaseDAL<SystemUser>
    {
        public SystemUserDAL(GlobalContext globalContext) : base(globalContext, SystemUser.EntityLogicalName) { }

        public List<SystemUser> GetApplicationUsers()
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression()
            {
                EntityName = SystemUser.EntityLogicalName,
                ColumnSet = new ColumnSet(SystemUser.Fields.SystemUserId)         
            };

            var filterExpression = new FilterExpression();
            query.Criteria.AddFilter(filterExpression);

            filterExpression.FilterOperator = LogicalOperator.Or;
            filterExpression.AddCondition(SystemUser.Fields.LastName, ConditionOperator.Equal, "SYSTEM");
            var applicationUserFilter = new FilterExpression();
            filterExpression.AddFilter(applicationUserFilter);

            applicationUserFilter.AddCondition(SystemUser.Fields.ApplicationId, ConditionOperator.NotNull);
            applicationUserFilter.AddCondition(SystemUser.Fields.AzureActiveDirectoryObjectId, ConditionOperator.NotNull);

            return this.GetMultiple(query);
        }
    }
}
