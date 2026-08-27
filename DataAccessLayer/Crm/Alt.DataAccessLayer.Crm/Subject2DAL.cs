using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class Subject2DAL : CrmBaseDAL<alt_Subject2>
    {
        public Subject2DAL(GlobalContext globalContext) : base(globalContext, alt_Subject2.EntityLogicalName) { }

        public List<alt_Subject2> GetSubject2InCodeRange(List<int> codes)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = alt_Subject2.EntityLogicalName,
                ColumnSet = new ColumnSet(alt_Subject2.Fields.Id),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = {
                        new ConditionExpression(alt_Subject2.Fields.alt_CodeInt, ConditionOperator.In, codes.Cast<object>().ToArray())
                    },
                },
                NoLock = true
            };

            return this.GetMultiple(query);
        }
    }
}
