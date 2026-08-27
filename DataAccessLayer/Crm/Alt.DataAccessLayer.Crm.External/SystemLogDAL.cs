using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class SystemLogDAL : CrmExternalBaseDAL<ApiSystemLog>
    {
        public SystemLogDAL(GlobalContext globalContext)
            : base(globalContext, ApiSystemLog.EntityLogicalName) { }
        public List<ApiSystemLog> GetErrorLogsByDate(DateTime date)
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = "alt_systemlog",
                ColumnSet = new ColumnSet(true),
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.On, date));
            filter.Conditions.Add(new ConditionExpression("alt_messagelevelcode", ConditionOperator.NotEqual, (int)MessageLevel.Information));
            query.AddOrder("alt_name", OrderType.Ascending);

            query.Criteria.AddFilter(filter);

            return this.GetMultipleWithPaging(query);
        }
    }
}
