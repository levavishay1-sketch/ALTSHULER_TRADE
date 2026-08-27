using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class PortfolioDAL : CrmBaseDAL<alt_Portfolio>
    {
        public static string bankAccountDetailsAlias = "bankAccountDetails";
        public static string bankEntityAlias = "bank";
        public static string branchEntityAlias = "branch";
        public static string beneficiaryAccountHolderEntityAlias = "beneficiaryAccountHolder";
        public static string mainAccountHolderEntityAlias = "mainAccountHolder";

        private string[] portfolioFields = new string[]
        {
            alt_Portfolio.Fields.Id,
            alt_Portfolio.Fields.alt_JoiningProcessNumber,
            alt_Portfolio.Fields.alt_ShenhavAccountNumber,
            alt_Portfolio.Fields.alt_ShenhavStatusCode
        };

        private string[] bankAccountDetailsFields = new string[]
        {
            alt_BankAccountDetails.Fields.alt_BankAccountDetailsId,
            alt_BankAccountDetails.Fields.alt_Name
        };

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

        public PortfolioDAL(GlobalContext globalContext) : base(globalContext, alt_Portfolio.EntityLogicalName) { }

        public List<alt_Portfolio> GetPortfoliosByBankDetails(string bankNumber, string branchNumber)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = alt_Portfolio.EntityLogicalName,
                ColumnSet = new ColumnSet(portfolioFields),
                NoLock = true,
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(alt_Portfolio.Fields.StateCode, ConditionOperator.Equal, (int)alt_PortfolioState.Active),
                    }
                },
            };
            query.Orders.Add(new OrderExpression(alt_Portfolio.Fields.CreatedOn, OrderType.Descending));

            var bankAccountDetailsLinkEntity = query.AddLink(alt_BankAccountDetails.EntityLogicalName, alt_Portfolio.Fields.Id, alt_BankAccountDetails.Fields.alt_PortfolioId, JoinOperator.Inner);
            bankAccountDetailsLinkEntity.EntityAlias = bankAccountDetailsAlias;
            bankAccountDetailsLinkEntity.Columns.AddColumns(bankAccountDetailsFields);

            var bankLinkEntity = bankAccountDetailsLinkEntity.AddLink(alt_Bank.EntityLogicalName, alt_BankAccountDetails.Fields.alt_BankId, alt_Bank.Fields.Id, JoinOperator.Inner);
            bankLinkEntity.EntityAlias = bankEntityAlias;
            bankLinkEntity.Columns.AddColumns(bankFields);
            bankLinkEntity.LinkCriteria.AddCondition(alt_Bank.Fields.alt_Code, ConditionOperator.Equal, bankNumber);

            var branchLinkEntity = bankAccountDetailsLinkEntity.AddLink(alt_Branch.EntityLogicalName, alt_BankAccountDetails.Fields.alt_BranchId, alt_Branch.Fields.Id, JoinOperator.Inner);
            branchLinkEntity.EntityAlias = branchEntityAlias;
            branchLinkEntity.Columns.AddColumns(branchFields);
            branchLinkEntity.LinkCriteria.AddCondition(alt_Branch.Fields.alt_BranchNumber, ConditionOperator.Equal, branchNumber);

            var mainAccountHolderLinkEntity = query.AddLink(alt_AccountHolder.EntityLogicalName, alt_Portfolio.Fields.alt_PortfolioId, alt_AccountHolder.Fields.alt_PortfolioId, JoinOperator.Inner);
            mainAccountHolderLinkEntity.EntityAlias = mainAccountHolderEntityAlias;
            mainAccountHolderLinkEntity.Columns.AddColumns(alt_AccountHolder.Fields.alt_Name, alt_AccountHolder.Fields.alt_IdentificationNumber);
            mainAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal,
                (int)AccountHolderTypeCode.Owner);
            mainAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_MainAccountHolderBit, ConditionOperator.Equal,
                true);

            var beneficiaryAccountHolderLinkEntity = query.AddLink(alt_AccountHolder.EntityLogicalName, alt_Portfolio.Fields.alt_PortfolioId, alt_AccountHolder.Fields.alt_PortfolioId, JoinOperator.LeftOuter);
            beneficiaryAccountHolderLinkEntity.EntityAlias = beneficiaryAccountHolderEntityAlias;
            beneficiaryAccountHolderLinkEntity.Columns.AddColumns(alt_AccountHolder.Fields.alt_Name);
            beneficiaryAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal,
                (int)AccountHolderTypeCode.Beneficiary);

            return this.GetMultipleWithPaging(query);
        }
    }
}
