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
    public class ScheduledOperationDAL : CrmExternalBaseDAL<ApiScheduledOperation>
    {
        string[] attributesToRetrieve = new string[]
          {
                "alt_schedulersetupid",
                "alt_schedulersetupcodeint",
                "statuscode",
                "alt_operationstartdate",
                "modifiedon",
                "alt_executionresult",
                "createdon"
          };
        public ScheduledOperationDAL(GlobalContext globalContext)
            : base(globalContext, ApiScheduledOperation.EntityLogicalName) { }

        public ApiScheduledOperation GetScheduledOperationDetails(Guid id, string[] attributes = null)
        {
            this.GlobalContext.LogEntry();
            return base.Get(id, attributes ?? attributesToRetrieve);
        }

        public List<ApiScheduledOperation> GetScheduledOperationsByDate(DateTime date)
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = "alt_scheduledoperation",
                ColumnSet = new ColumnSet(attributesToRetrieve)
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.On, date.ToString("yyyy-MM-dd")));
            query.Criteria.AddFilter(filter);

            return base.GetMultiple(query);
        }
    }
}
