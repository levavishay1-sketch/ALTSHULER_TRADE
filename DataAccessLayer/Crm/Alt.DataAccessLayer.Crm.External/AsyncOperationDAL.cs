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
    public class AsyncOperationDAL : CrmExternalBaseDAL<ApiAsyncOperation>
    {
        public AsyncOperationDAL(GlobalContext globalContext)
            : base(globalContext, ApiAsyncOperation.EntityLogicalName) { }

        public List<ApiAsyncOperation> GetFailedJobsByDate(DateTime date)
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = "asyncoperation",
                ColumnSet = new ColumnSet("asyncoperationid", "name", "startedon", "ownerid", "requestid", "regardingobjectid", "createdon", "friendlymessage", "message")
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.On, date.ToString("yyyy-MM-dd")));
            filter.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 31));
            query.AddOrder("name", OrderType.Ascending);
            query.Criteria.AddFilter(filter);

            return base.GetMultipleWithPaging(query);
        }
    }
}
