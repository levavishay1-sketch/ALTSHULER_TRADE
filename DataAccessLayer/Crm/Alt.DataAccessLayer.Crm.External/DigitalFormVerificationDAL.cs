using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class DigitalFormVerificationDAL : CrmExternalBaseDAL<ApiDigitalFormVerification>
    {
        string bankEntityAlias = "bank";
        string branchEntityAlias = "branch";
        string commissionClientTypeEntityAlias = "commissionClientType";
        string loyaltyProgramEntityAlias = "loyaltyProgram";
        string primaryAttributeName = "alt_name";

        public DigitalFormVerificationDAL(GlobalContext globalContext) : base(globalContext, ApiDigitalFormVerification.EntityLogicalName) { }

        public ApiDigitalFormVerification GetDigitalFormVerificationDetails(Guid id)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = ApiDigitalFormVerification.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression($"{ApiDigitalFormVerification.EntityLogicalName.ToLower()}id", ConditionOperator.Equal, id) }
                },
            };
            query.NoLock = true;
            var bankLinkEntity = query.AddLink(ApiBank.EntityLogicalName, "alt_bankid", "alt_bankid", JoinOperator.LeftOuter);
            bankLinkEntity.EntityAlias = bankEntityAlias;
            bankLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_code");

            var branchLinkEntity = query.AddLink(ApiBranch.EntityLogicalName, "alt_branchid", "alt_branchid", JoinOperator.LeftOuter);
            branchLinkEntity.EntityAlias = branchEntityAlias;
            branchLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_branchnumber", "alt_branchname");

            var commissionClientTypeLinkEntity = query.AddLink(ApiCommissionClientType.EntityLogicalName, "alt_commissionclienttypeid", "alt_commissionclienttypeid", JoinOperator.LeftOuter);
            commissionClientTypeLinkEntity.EntityAlias = commissionClientTypeEntityAlias;
            commissionClientTypeLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_code");

            var loyaltyProgramLinkEntity = query.AddLink(ApiLoyaltyProgram.EntityLogicalName, "alt_loyaltyprogramid", "alt_loyaltyprogramid", JoinOperator.LeftOuter);
            loyaltyProgramLinkEntity.EntityAlias = loyaltyProgramEntityAlias;
            loyaltyProgramLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_codeint");

            var digitalFormVerification = this.GetMultipleAsEntity(query).Entities.FirstOrDefault();
            return digitalFormVerification != null ? this.MappToApiAgentContract(digitalFormVerification) : null;
        }

        public List<ApiDigitalFormVerification> GetDigitalFormVerificationsByFetchXML(string fetchXML)
        {
            this.GlobalContext.LogEntry();
            return this.GetMultiple(new FetchExpression(fetchXML));
        }

        private ApiDigitalFormVerification MappToApiAgentContract(Entity digitalFormVerification)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            ApiDigitalFormVerification apiDigitalFormVerification = base.MappCrmEntityToApiEntity(digitalFormVerification);

            if (apiDigitalFormVerification.Bank != null)
            {
                apiDigitalFormVerification.Bank.Name = digitalFormVerification.GetAliasedAttributeValue<string>(bankEntityAlias, primaryAttributeName);
                apiDigitalFormVerification.Bank.Code = digitalFormVerification.GetAliasedAttributeValue<string>(bankEntityAlias, "alt_code");
            }
            if (apiDigitalFormVerification.Branch != null)
            {
                apiDigitalFormVerification.Branch.Name = digitalFormVerification.GetAliasedAttributeValue<string>(branchEntityAlias, primaryAttributeName);
                apiDigitalFormVerification.Branch.BranchNumber = digitalFormVerification.GetAliasedAttributeValue<string>(branchEntityAlias, "alt_branchnumber");
                apiDigitalFormVerification.Branch.BranchName = digitalFormVerification.GetAliasedAttributeValue<string>(branchEntityAlias, "alt_branchname");
            }
            if (apiDigitalFormVerification.CommissionClientType != null)
            {
                apiDigitalFormVerification.CommissionClientType.Code = digitalFormVerification.GetAliasedAttributeValue<string>(commissionClientTypeEntityAlias, "alt_code");
            }
            if (apiDigitalFormVerification.LoyaltyProgramId != null)
            {
                apiDigitalFormVerification.LoyaltyProgramId.Code = digitalFormVerification.GetAliasedAttributeValue<int?>(loyaltyProgramEntityAlias, "alt_codeint");
                apiDigitalFormVerification.LoyaltyProgramId.Name = digitalFormVerification.GetAliasedAttributeValue<string>(loyaltyProgramEntityAlias, primaryAttributeName);
            }
            return apiDigitalFormVerification;
        }
    }
}
