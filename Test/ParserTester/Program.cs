using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Interfaces;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserTester
{
    class Program
    {
        static string connectionString = @"
                    AuthType=OAuth;
                    Username=lahav@altshul.co.il;
                    Password=Lt2026@@;
                    Url=https://altshulercrm.crm4.dynamics.com/;
                    AppId=58145B91-0C36-4500-8554-080854F2AC97;
                    RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;
                    TokenCacheStorePath=c:\MyTokenCache;
                    ";

        static void Main(string[] args)
        {
            var a = GetDigitalFormVerificationsByBankDetails("4177355", "10", "752");

            var b = 2;
        }


        private static string bankEntityAlias = "bank";
        private static string branchEntityAlias = "branch";
        private static string beneficiaryAccountHolderEntityAlias = "beneficiaryAccountHolder";
        private static string mainAccountHolderEntityAlias = "mainAccountHolder";
        private static string teamEntityAlias = "team";

        public static string[] bankFields = new string[]
        {
            alt_Bank.Fields.alt_Name,
            alt_Bank.Fields.alt_Code
        };

        public static string[] branchFields = new string[]
        {
            alt_Branch.Fields.alt_Name,
            alt_Branch.Fields.alt_BranchNumber,
            alt_Branch.Fields.alt_BranchName,
        };

        public static string[] columns = new[]
        {
            alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode,
            alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersForStageJoiningBit,
            alt_DigitalFormVerification.Fields.alt_BeneficiaryDeclarationControlExistsBit,
            alt_DigitalFormVerification.Fields.alt_VerifiedAccountHoldersStageManagementBit,
            alt_DigitalFormVerification.Fields.Id,
            alt_DigitalFormVerification.Fields.alt_DigitalFormNumber,
            alt_DigitalFormVerification.Fields.alt_FormStatusCode,
            alt_DigitalFormVerification.Fields.alt_PrimaryAccountHolderId
        };

        public static IEnumerable<Entity> GetDigitalFormVerificationsByBankDetails(string bankAccountNumber, string bankCode, string branchNumber)
        {
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
                        new ConditionExpression(alt_DigitalFormVerification.Fields.alt_BankAccountNumber, ConditionOperator.Equal, bankAccountNumber),
                    }
                },
            };
            query.Orders.Add(new OrderExpression(alt_DigitalFormVerification.Fields.CreatedOn, OrderType.Descending));

            var bankLinkEntity = query.AddLink(alt_Bank.EntityLogicalName, alt_DigitalFormVerification.Fields.alt_BankId, alt_Bank.Fields.Id, JoinOperator.Inner);
            bankLinkEntity.EntityAlias = bankEntityAlias;
            bankLinkEntity.Columns.AddColumns(bankFields);
            bankLinkEntity.LinkCriteria.AddCondition(alt_Bank.Fields.alt_Code, ConditionOperator.Equal, bankCode);

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

            return ExecuteQuery<alt_DigitalFormVerification>(query);
        }

        //private static string bankAccountDetailsAlias = "bankAccountDetails";
        //private static string bankEntityAlias = "bank";
        //private static string branchEntityAlias = "branch";
        //public static string beneficiaryAccountHolderEntityAlias = "beneficiaryAccountHolder";
        //public static string mainAccountHolderEntityAlias = "mainAccountHolder";

        //private static string[] portfolioFields = new string[]
        //{
        //    alt_Portfolio.Fields.Id,
        //    alt_Portfolio.Fields.alt_JoiningProcessNumber,
        //    alt_Portfolio.Fields.alt_ShenhavAccountNumber,
        //    alt_Portfolio.Fields.alt_ShenhavStatusCode
        //};

        //private static string[] bankAccountDetailsFields = new string[]
        //{
        //    alt_BankAccountDetails.Fields.alt_BankAccountDetailsId
        //};

        //private static string[] bankFields = new string[]
        //{
        //    alt_Bank.Fields.alt_Name,
        //    alt_Bank.Fields.alt_Code
        //};

        //private static string[] branchFields = new string[]
        //{
        //    alt_Branch.Fields.alt_Name,
        //    alt_Branch.Fields.alt_BranchNumber,
        //    alt_Branch.Fields.alt_BranchName,
        //};

        //public static IEnumerable<Entity> GetPortfoliosByBankDetails(string bankAccountNumber, string bankCode, string branchNumber)
        //{
        //    QueryExpression query = new QueryExpression
        //    {
        //        EntityName = alt_Portfolio.EntityLogicalName,
        //        ColumnSet = new ColumnSet(portfolioFields),
        //        NoLock = true,
        //        Criteria = new FilterExpression(LogicalOperator.And)
        //        {
        //            Conditions =
        //            {
        //                new ConditionExpression(alt_Portfolio.Fields.StateCode, ConditionOperator.Equal, (int)alt_PortfolioState.Active),
        //            }
        //        },
        //    };
        //    query.Orders.Add(new OrderExpression(alt_Portfolio.Fields.CreatedOn, OrderType.Descending));

        //    var bankAccountDetailsLinkEntity = query.AddLink(alt_BankAccountDetails.EntityLogicalName, alt_Portfolio.Fields.Id, alt_BankAccountDetails.Fields.alt_PortfolioId, JoinOperator.Inner);
        //    bankAccountDetailsLinkEntity.EntityAlias = bankAccountDetailsAlias;
        //    bankAccountDetailsLinkEntity.Columns.AddColumns(bankAccountDetailsFields);
        //    bankAccountDetailsLinkEntity.LinkCriteria.AddCondition(alt_BankAccountDetails.Fields.alt_Name, ConditionOperator.Equal, bankAccountNumber);

        //    var bankLinkEntity = bankAccountDetailsLinkEntity.AddLink(alt_Bank.EntityLogicalName, alt_BankAccountDetails.Fields.alt_BankId, alt_Bank.Fields.Id, JoinOperator.Inner);
        //    bankLinkEntity.EntityAlias = bankEntityAlias;
        //    bankLinkEntity.Columns.AddColumns(bankFields);
        //    bankLinkEntity.LinkCriteria.AddCondition(alt_Bank.Fields.alt_Code, ConditionOperator.Equal, bankCode);

        //    var branchLinkEntity = bankAccountDetailsLinkEntity.AddLink(alt_Branch.EntityLogicalName, alt_BankAccountDetails.Fields.alt_BranchId, alt_Branch.Fields.Id, JoinOperator.Inner);
        //    branchLinkEntity.EntityAlias = branchEntityAlias;
        //    branchLinkEntity.Columns.AddColumns(branchFields);
        //    branchLinkEntity.LinkCriteria.AddCondition(alt_Branch.Fields.alt_BranchNumber, ConditionOperator.Equal, branchNumber);

        //    var mainAccountHolderLinkEntity = query.AddLink(alt_AccountHolder.EntityLogicalName, alt_Portfolio.Fields.alt_PortfolioId, alt_AccountHolder.Fields.alt_PortfolioId, JoinOperator.Inner);
        //    mainAccountHolderLinkEntity.EntityAlias = mainAccountHolderEntityAlias;
        //    mainAccountHolderLinkEntity.Columns.AddColumns(alt_AccountHolder.Fields.alt_Name);
        //    mainAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal,
        //        (int)AccountHolderTypeCode.Owner);
        //    mainAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_MainAccountHolderBit, ConditionOperator.Equal,
        //        true);

        //    var beneficiaryAccountHolderLinkEntity = query.AddLink(alt_AccountHolder.EntityLogicalName, alt_Portfolio.Fields.alt_PortfolioId, alt_AccountHolder.Fields.alt_PortfolioId, JoinOperator.LeftOuter);
        //    beneficiaryAccountHolderLinkEntity.EntityAlias = beneficiaryAccountHolderEntityAlias;
        //    beneficiaryAccountHolderLinkEntity.Columns.AddColumns(alt_AccountHolder.Fields.alt_Name);
        //    beneficiaryAccountHolderLinkEntity.LinkCriteria.AddCondition(alt_AccountHolder.Fields.alt_AccountHolderTypeCode, ConditionOperator.Equal,
        //        (int)AccountHolderTypeCode.Beneficiary);

        //    return ExecuteQuery<alt_Portfolio>(query);
        //}


        //public static string GetParsedMessage(string message, string regardingObjectEntityLogicalName , string regardingObjectId, IEntityValueResolver entityValueResolver = null)
        //{
        //    if (!string.IsNullOrWhiteSpace(message))
        //    {
        //        Parser parser = new Parser(new ParserSettings()
        //        {
        //            RegardingObjectId = regardingObjectId,
        //            RegardingObjectEntityLogicalName = regardingObjectEntityLogicalName,
        //            MessageToParse = message,
        //            EntityValueResolver = entityValueResolver,
        //            ValueToParseInEmptyOrInvalidPlaceHolders = " "
        //        });

        //        return parser.GetParsedMessage(ExecuteQuery<Entity>);
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        protected static IEnumerable<Entity> ExecuteQuery<T>(QueryBase query)
        {
            CrmServiceClient serviceClient = new CrmServiceClient(connectionString);
            return serviceClient.RetrieveMultiple(query).Entities?.ToList();
        }
    }
}
