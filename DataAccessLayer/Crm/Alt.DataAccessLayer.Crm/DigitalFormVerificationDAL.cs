using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class DigitalFormVerificationDAL : CrmBaseDAL<alt_DigitalFormVerification>
    {
        public DigitalFormVerificationDAL(GlobalContext globalContext)
            : base(globalContext, alt_DigitalFormVerification.EntityLogicalName) { }

        public static string bankEntityAlias = "bank";
        public static string branchEntityAlias = "branch";
        public static string beneficiaryAccountHolderEntityAlias = "beneficiaryAccountHolder";
        public static string mainAccountHolderEntityAlias = "mainAccountHolder";
        public static string teamEntityAlias = "team";

        private string[] bankFields = new string[]
        {
            alt_Bank.Fields.alt_Name,
            alt_Bank.Fields.alt_Code
        };

        private string[] branchFields = new string[]
        {
            alt_Branch.Fields.alt_Name,
            alt_Branch.Fields.alt_BranchNumber,
            alt_Branch.Fields.alt_BranchName,
        };

        string[] columns = new[]
        {
            alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode,
            alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersForStageJoiningBit,
            alt_DigitalFormVerification.Fields.alt_BeneficiaryDeclarationControlExistsBit,
            alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersStageManagementBit,
            alt_DigitalFormVerification.Fields.Id,
            alt_DigitalFormVerification.Fields.alt_DigitalFormNumber,
            alt_DigitalFormVerification.Fields.alt_FormStatusCode,
            alt_DigitalFormVerification.Fields.alt_BankAccountNumber
        };

        public alt_DigitalFormVerification GetDigitalFormVerificationDetails(Guid id, string[] attributes = null)
        {
            this.GlobalContext.LogEntry();
            return base.Get(id, attributes ?? columns);
        }

        public alt_DigitalFormVerification GetByDigitalFormNumberWithNoOpportunity(string opportunityIdentityNumber)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = alt_DigitalFormVerification.EntityLogicalName,
                ColumnSet = new ColumnSet(alt_DigitalFormVerification.Fields.alt_DigitalFormVerificationId),
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression(alt_DigitalFormVerification.Fields.alt_OpportunityId, ConditionOperator.Null));
            filter.Conditions.Add(new ConditionExpression(alt_DigitalFormVerification.Fields.alt_DigitalFormNumber, ConditionOperator.Equal, opportunityIdentityNumber));
            query.Criteria.AddFilter(filter);

            return this.GetFirstOrDefault(query);
        }

        public List<alt_DigitalFormVerification> GetDigitalFormVerificationsByBankDetails(string bankNumber, string branchNumber)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = alt_DigitalFormVerification.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                NoLock = true,
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(alt_DigitalFormVerification.Fields.StateCode, ConditionOperator.Equal, (int)alt_DigitalFormVerificationState.Active),
                        new ConditionExpression(alt_DigitalFormVerification.Fields.alt_FormStatusCode, ConditionOperator.NotEqual, (int)FormStatusCode.Canceled),
                    }
                },
            };
            query.Orders.Add(new OrderExpression(alt_DigitalFormVerification.Fields.CreatedOn, OrderType.Descending));

            var bankLinkEntity = query.AddLink(alt_Bank.EntityLogicalName, alt_DigitalFormVerification.Fields.alt_BankId, alt_Bank.Fields.Id, JoinOperator.Inner);
            bankLinkEntity.EntityAlias = bankEntityAlias;
            bankLinkEntity.Columns.AddColumns(bankFields);
            bankLinkEntity.LinkCriteria.AddCondition(alt_Bank.Fields.alt_Code, ConditionOperator.Equal, bankNumber);

            var branchLinkEntity = query.AddLink(alt_Branch.EntityLogicalName, alt_DigitalFormVerification.Fields.alt_BranchId, alt_Branch.Fields.Id, JoinOperator.Inner);
            branchLinkEntity.EntityAlias = branchEntityAlias;
            branchLinkEntity.Columns.AddColumns(branchFields);
            branchLinkEntity.LinkCriteria.AddCondition(alt_Branch.Fields.alt_BranchNumber, ConditionOperator.Equal, branchNumber);

            var mainAccountHolderLinkEntity = query.AddLink(alt_AccountHolder.EntityLogicalName, alt_DigitalFormVerification.Fields.alt_DigitalFormVerificationId, alt_AccountHolder.Fields.alt_DigitalFormVerificationId, JoinOperator.Inner);
            mainAccountHolderLinkEntity.EntityAlias = mainAccountHolderEntityAlias;
            mainAccountHolderLinkEntity.Columns.AddColumns(alt_AccountHolder.Fields.alt_Name, alt_AccountHolder.Fields.alt_IdentificationNumber);
            mainAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal,
                (int)AccountHolderTypeCode.Owner);
            mainAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_MainAccountHolderBit, ConditionOperator.Equal,
                true);

            var beneficiaryAccountHolderLinkEntity = query.AddLink(alt_AccountHolder.EntityLogicalName, alt_DigitalFormVerification.Fields.Id, alt_AccountHolder.Fields.alt_DigitalFormVerificationId, JoinOperator.LeftOuter);
            beneficiaryAccountHolderLinkEntity.EntityAlias = beneficiaryAccountHolderEntityAlias;
            beneficiaryAccountHolderLinkEntity.Columns.AddColumns(alt_AccountHolder.Fields.alt_Name);
            beneficiaryAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal,
                (int)AccountHolderTypeCode.Beneficiary);

            var teamLinkEntity = query.AddLink(Team.EntityLogicalName, alt_DigitalFormVerification.Fields.alt_ControlStageTeamId, Team.Fields.Id, JoinOperator.LeftOuter);
            teamLinkEntity.EntityAlias = teamEntityAlias;
            teamLinkEntity.Columns.AddColumns(Team.Fields.alt_TeamCodeInt);

            return this.GetMultipleWithPaging(query);
        }
    }
}