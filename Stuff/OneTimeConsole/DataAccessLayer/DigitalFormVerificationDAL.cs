using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace OneTimeConsole.DataAccessLayer
{
    public class DigitalFormVerificationDAL : CrmBaseDAL<alt_DigitalFormVerification>
    {
        public DigitalFormVerificationDAL(GlobalContext globalContext, string entityName = null) : base(globalContext, entityName) { }

        internal List<alt_DigitalFormVerification> GetActiveWithMainAccountHolder()
        {
            GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression(entityLogicalName)
            {
                NoLock = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(alt_DigitalFormVerification.Fields.StateCode, ConditionOperator.Equal, (int)alt_DigitalFormVerificationState.Active),
                        new ConditionExpression(alt_DigitalFormVerification.Fields.alt_PrimaryAccountHolderId, ConditionOperator.Null)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity()
                    {
                        JoinOperator = JoinOperator.Inner,
                        LinkFromEntityName = alt_DigitalFormVerification.EntityLogicalName,
                        LinkToEntityName = alt_AccountHolder.EntityLogicalName,
                        LinkFromAttributeName = alt_DigitalFormVerification.PrimaryIdAttribute,
                        LinkToAttributeName = alt_AccountHolder.Fields.alt_DigitalFormVerificationId,
                        EntityAlias = alt_AccountHolder.EntityLogicalName,
                        Columns = new ColumnSet(alt_AccountHolder.PrimaryIdAttribute),
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression(alt_AccountHolder.Fields.StateCode, ConditionOperator.Equal, (int)alt_AccountHolderState.Active),
                                new ConditionExpression(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal, (int)AccountHolderTypeCode.Owner),
                                new ConditionExpression(alt_AccountHolder.Fields.alt_MainAccountHolderBit, ConditionOperator.Equal, true)
                            }
                        }
                    }
                }
            };

            List<alt_DigitalFormVerification> retrievedDigitalFormVerifications = GetMultipleWithPaging(query);
            return retrievedDigitalFormVerifications;
        }

        internal List<alt_DigitalFormVerification> GetWithEmptyEncouragingDepositSystemUser()
        {
            GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression(entityLogicalName)
            {
                NoLock = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(alt_DigitalFormVerification.Fields.alt_EncouragingDepositSystemUserId, ConditionOperator.Null),
                        new ConditionExpression(alt_DigitalFormVerification.Fields.alt_OpportunityId, ConditionOperator.NotNull)
                    },
                    FilterOperator = LogicalOperator.And
                },
                LinkEntities =
                {
                    new LinkEntity()
                    {
                        JoinOperator = JoinOperator.Inner,
                        LinkFromEntityName = alt_DigitalFormVerification.EntityLogicalName,
                        LinkToEntityName = Opportunity.EntityLogicalName,
                        LinkFromAttributeName = alt_DigitalFormVerification.Fields.alt_OpportunityId,
                        LinkToAttributeName = Opportunity.PrimaryIdAttribute,
                        EntityAlias = Opportunity.EntityLogicalName,
                        Columns = new ColumnSet(Opportunity.Fields.OwnerId),
                    }
                }
            };

            List<alt_DigitalFormVerification> retrievedDigitalFormVerifications = GetMultipleWithPaging(query);
            return retrievedDigitalFormVerifications;
        }
    }
}