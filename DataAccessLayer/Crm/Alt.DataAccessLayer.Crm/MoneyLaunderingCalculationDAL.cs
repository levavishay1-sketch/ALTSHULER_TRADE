using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class MoneyLaunderingCalculationDAL : CrmBaseDAL<alt_MoneyLaunderingCalculation>
    {
        public MoneyLaunderingCalculationDAL(GlobalContext globalContext) : base(globalContext, alt_MoneyLaunderingCalculation.EntityLogicalName)
        {
        }
        public List<alt_MoneyLaunderingCalculation> GetAllKYCLinkedAccountHolderByDigitalFormVerificationId(List<Guid> recordsKYC)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_MoneyLaunderingCalculation.EntityLogicalName,
                ColumnSet = new ColumnSet(alt_MoneyLaunderingCalculation.Fields.CreatedOn, alt_MoneyLaunderingCalculation.Fields.alt_KYCId, alt_MoneyLaunderingCalculation.Fields.alt_CalculetedMoneyLaunderingLevelCode),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(alt_MoneyLaunderingCalculation.Fields.alt_KYCId, ConditionOperator.In, recordsKYC)
                        }
                    },
                LinkEntities =
                    {
                        new LinkEntity
                        {
                            JoinOperator = JoinOperator.Inner,
                            LinkFromEntityName = alt_MoneyLaunderingCalculation.EntityLogicalName,
                            LinkFromAttributeName =alt_MoneyLaunderingCalculation.Fields.alt_KYCId,
                            LinkToEntityName = alt_KYC.EntityLogicalName,
                            LinkToAttributeName = alt_KYC.Fields.alt_KYCId
                        }
                    }
            };
            return this.GetMultiple(query);
        }
    }
}