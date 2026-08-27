using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class KYCDAL : CrmBaseDAL<alt_KYC>
    {
        public KYCDAL(GlobalContext globalContext) : base(globalContext, alt_KYC.EntityLogicalName) { }

        public List<alt_KYC> GetActiveAccountHoldersKYCsByDigitalFormVerificationId(Guid alt_DigitalFormVerificationId, string[] attributes)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_KYC.EntityLogicalName,
                ColumnSet = new ColumnSet(attributes),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_KYC.Fields.alt_DigitalFormVerificationId, ConditionOperator.Equal, alt_DigitalFormVerificationId),
                            new ConditionExpression(alt_KYC.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    },
                LinkEntities =
                    {
                        new LinkEntity
                        {
                            JoinOperator = JoinOperator.Inner,
                            LinkFromEntityName = alt_KYC.EntityLogicalName,
                            LinkFromAttributeName =alt_KYC.Fields.alt_AccountHolderId,
                            LinkToEntityName = alt_AccountHolder.EntityLogicalName,
                            LinkToAttributeName = alt_AccountHolder.Fields.alt_AccountHolderId
                        }
                    }
            };
            return this.GetMultiple(query);
        }

        public List<alt_KYC> GetActiveAccountHolderKYCsByAccountHolderTypeAndDigitalFormVerificationId(Guid alt_DigitalFormVerificationId, int[] accountHolderTypeCode, string[] attributes = null)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_KYC.EntityLogicalName,
                ColumnSet = attributes != null ? new ColumnSet(attributes) : new ColumnSet(true),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_KYC.Fields.alt_DigitalFormVerificationId, ConditionOperator.Equal, alt_DigitalFormVerificationId),
                            new ConditionExpression(alt_KYC.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    },
                LinkEntities =
                    {
                        new LinkEntity
                        {
                            JoinOperator = JoinOperator.Inner,
                            LinkFromEntityName = alt_KYC.EntityLogicalName,
                            LinkFromAttributeName =alt_KYC.Fields.alt_AccountHolderId,
                            LinkToEntityName = alt_AccountHolder.EntityLogicalName,
                            LinkToAttributeName = alt_AccountHolder.Fields.alt_AccountHolderId,
                            LinkCriteria =
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                    new ConditionExpression(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.In, accountHolderTypeCode)
                                }
                            }
                        }
                    }
            };
            return this.GetMultiple(query);
        }

        public List<alt_KYC> GetAllActiveKYCByAccountHolderId(Guid alt_AccountHolderId, string[] attributes)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_KYC.EntityLogicalName,
                ColumnSet = new ColumnSet(attributes),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_KYC.Fields.alt_AccountHolderId, ConditionOperator.Equal, alt_AccountHolderId),
                            new ConditionExpression(alt_KYC.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    }
            };
            return this.GetMultiple(query);
        }
    }
}