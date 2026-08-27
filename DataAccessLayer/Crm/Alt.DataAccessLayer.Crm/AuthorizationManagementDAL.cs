using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Alt.DataAccessLayer.Crm
{
    public class AuthorizationManagementDAL : CrmBaseDAL<alt_AuthorizationManagement>
    {
        string[] columns = new[]
            {
                alt_AuthorizationManagement.Fields.alt_AuthorizationManagementId
            };
        public AuthorizationManagementDAL(GlobalContext globalContext) : base(globalContext, alt_AuthorizationManagement.EntityLogicalName)
        {
        }
        public alt_AuthorizationManagement GetLastCreatedOnAuthorizationManagementByDigitalFormVerificationId(Guid alt_DigitalFormVerificationId, string[] attributes = null)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                NoLock = true,
                EntityName = alt_AuthorizationManagement.EntityLogicalName,
                ColumnSet = new ColumnSet(attributes ?? this.columns),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_AuthorizationManagement.Fields.alt_DigitalFormVerificationId, ConditionOperator.Equal, alt_DigitalFormVerificationId),
                            new ConditionExpression(alt_AuthorizationManagement.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    },
                Orders =
                {
                    new OrderExpression(alt_AuthorizationManagement.Fields.CreatedOn, OrderType.Descending)
                }
            };
            return this.GetFirstOrDefault(query);
        }
    }
}