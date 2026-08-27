using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm
{
    public class AccountHolderDAL : CrmBaseDAL<alt_AccountHolder>
    {
        string[] attributes ={
                alt_AccountHolder.Fields.alt_AccountHolderTypeCode,
                alt_AccountHolder.Fields.alt_CheckTerrorOrganizationCode,
                alt_AccountHolder.Fields.alt_DigitalVisualRecognitionCode,
                alt_AccountHolder.Fields.alt_DigitalFormVerificationId,
                alt_AccountHolder.Fields.alt_BeneficiaryDeclarationControlCode,
                alt_AccountHolder.Fields.alt_PerformAdditionalVerificationCode,
                alt_AccountHolder.Fields.alt_PostalCode,
                alt_AccountHolder.Fields.alt_ManualControlVerificationIDDescription,
                alt_AccountHolder.Fields.alt_ManualControlVerificationIDAppliedCode,
                alt_AccountHolder.Fields.alt_Email,
                alt_AccountHolder.Fields.alt_MobilePhone,
                alt_AccountHolder.Fields.alt_Name,
                alt_AccountHolder.Fields.alt_CustomerId
            };
        public AccountHolderDAL(GlobalContext globalContext) : base(globalContext, alt_AccountHolder.EntityLogicalName) { }

        public List<alt_AccountHolder> GetAllAccountHolderByDigitalFormVerificationId(Guid digitalFormVerificationId, string[] attributes = null)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return base.GetActiveByAttribute(alt_AccountHolder.Fields.alt_DigitalFormVerificationId, digitalFormVerificationId, attributes ?? this.attributes);
        }

        public List<alt_AccountHolder> GetAllAccountHoldersByDigitalFormVerificationIds(Guid[] digitalFormVerificationIds)
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = alt_AccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    alt_AccountHolder.Fields.alt_IdentificationNumber,
                    alt_AccountHolder.Fields.alt_AccountHolderTypeCode,
                    alt_AccountHolder.Fields.alt_DigitalFormVerificationId
                ),
                NoLock = true,
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            alt_AccountHolder.Fields.alt_DigitalFormVerificationId,
                            ConditionOperator.In,
                            digitalFormVerificationIds
                        )
                    }
                }
            };

            return this.GetMultipleWithPaging(query);
        }

        public List<alt_AccountHolder> GetAccountHolderByTypeAccountHolderAndDigitalFormVerificationId(Guid digitalFormVerificationId, int[] accountHolderTypeCode, string[] attributes)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_AccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(attributes),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_AccountHolder.Fields.alt_DigitalFormVerificationId, ConditionOperator.Equal, digitalFormVerificationId),
                            new ConditionExpression(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.In, accountHolderTypeCode),
                            new ConditionExpression(alt_AccountHolder.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    }
            };
            return this.GetMultiple(query);
        }

        public alt_AccountHolder GetMainAccountHolderByDigitalFormVerificationId(Guid digitalFormVerificationId, string[] attributes = null)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_AccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(attributes ?? this.attributes),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_AccountHolder.Fields.alt_DigitalFormVerificationId, ConditionOperator.Equal, digitalFormVerificationId),
                            new ConditionExpression(alt_AccountHolder.Fields.alt_MainAccountHolderBit, ConditionOperator.Equal, true),
                            new ConditionExpression(alt_AccountHolder.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    }
            };
            return this.GetMultiple(query).FirstOrDefault();
        }

        public List<alt_AccountHolder> GetRelatedAccountHolders(EntityReference relatedEntity, List<Guid> customerIds)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_AccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(attributes)
            };
            query.Criteria = new FilterExpression();
            query.Criteria.FilterOperator = LogicalOperator.And;

            FilterExpression relatedEntityFilter = query.Criteria.AddFilter(LogicalOperator.And);
            relatedEntityFilter.Conditions.Add(new ConditionExpression($"{relatedEntity.LogicalName}id", ConditionOperator.Equal, relatedEntity.Id));
            relatedEntityFilter.Conditions.Add(new ConditionExpression(alt_AccountHolder.Fields.alt_CustomerId, ConditionOperator.In, customerIds));
            relatedEntityFilter.Conditions.Add(new ConditionExpression(alt_AccountHolder.Fields.StateCode, ConditionOperator.Equal, 0));

            return base.GetMultiple(query);
        }
    }
}