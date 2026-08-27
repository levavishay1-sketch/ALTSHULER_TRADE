using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace OneTimeConsole.DataAccessLayer
{
    public class PortfolioDAL : CrmBaseDAL<alt_Portfolio>
    {
        private string[] fieldsForPortfolioConversionTime = new string[]
        {
            alt_Portfolio.Fields.CreatedOn
        };

        private string[] fieldsForLinkedLeadToPortfolio = new string[]
        {
            Lead.Fields.CreatedOn
        };

        public PortfolioDAL(GlobalContext globalContext, string entityName = null) : base(globalContext, entityName) { }

        public List<alt_Portfolio> RetrieveAllPortfoliosWithEmptyConversionTime()
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression()
            {
                EntityName = entityLogicalName,
                ColumnSet = new ColumnSet(fieldsForPortfolioConversionTime),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(alt_Portfolio.Fields.StateCode, ConditionOperator.Equal, (int)alt_PortfolioState.Active),
                        new ConditionExpression(alt_Portfolio.Fields.alt_JoiningProcessNumber, ConditionOperator.NotNull),
                        new ConditionExpression(alt_Portfolio.Fields.alt_ConversionTimeInDaysInt, ConditionOperator.Null)
                    }
                }
            };

            LinkEntity linkLeadEntity = query.AddLink(
                Lead.EntityLogicalName,
                alt_Portfolio.Fields.alt_JoiningProcessNumber,
                Lead.Fields.alt_LeadIdentityNumber
            );
            linkLeadEntity.JoinOperator = JoinOperator.Inner;
            linkLeadEntity.EntityAlias = Lead.EntityLogicalName;
            linkLeadEntity.Columns = new ColumnSet(fieldsForLinkedLeadToPortfolio);

            List<alt_Portfolio> retrievedPortfoliosWithLeads = GetMultipleWithPaging(query);
            GlobalContext.Log.Info($"RetrievedL Portfolios: {retrievedPortfoliosWithLeads?.Count}");
            return retrievedPortfoliosWithLeads;
        }
    }
}
