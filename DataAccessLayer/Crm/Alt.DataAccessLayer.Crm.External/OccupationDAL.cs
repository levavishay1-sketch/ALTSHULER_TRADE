using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm.External
{
    public class OccupationDAL : CrmExternalBaseDAL<ApiOccupation>
    {
        public OccupationDAL(GlobalContext globalContext) : base(globalContext, ApiOccupation.EntityLogicalName) { }

        public List<ApiOccupation> GetAll()
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiOccupation.EntityLogicalName,
                ColumnSet = new ColumnSet(new string[] { "alt_name", "alt_codeint" }),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("statuscode", ConditionOperator.Equal, 1)
                    }
                },
                NoLock = true
            };

            return base.GetMultipleWithPaging(query);
        }
    }
}
