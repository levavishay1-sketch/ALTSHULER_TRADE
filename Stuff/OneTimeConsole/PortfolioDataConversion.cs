using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using OneTimeConsole.DataAccessLayer;
using OneTimeConsole.Enums;
using System;
using System.Collections.Generic;

namespace OneTimeConsole
{
    public class PortfolioDataConversion : OperationBase
    {
        public PortfolioDataConversion(GlobalContext globalContext, OperationCode operationCode)
            : base(globalContext, operationCode) { }

        public override void Run()
        {
            this.GlobalContext.LogEntry();
            switch (this.OperationCode)
            {
                case OperationCode.PortfoliosConversionTime:
                    {
                        this.HandlePortfoliosConversionTimeLogic();
                        break;
                    }
                default:
                    break;
            }
        }

        private void HandlePortfoliosConversionTimeLogic()
        {
            this.GlobalContext.LogEntry();

            PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
            List<alt_Portfolio> retrievedPortfolios = portfolioDAL.RetrieveAllPortfoliosWithEmptyConversionTime();
            List<alt_Portfolio> portfoliosToUpdate = new List<alt_Portfolio>();

            if (retrievedPortfolios.Count > 0)
            {
                foreach (alt_Portfolio retrievedPortfolio in retrievedPortfolios)
                {
                    DateTime portfolioCreatedOn = retrievedPortfolio.CreatedOn.Value;
                    DateTime leadCreatedOn =
                        retrievedPortfolio.GetAliasedAttributeValue<DateTime>(Lead.EntityLogicalName, Lead.Fields.CreatedOn);
                    int differenceInDays = (portfolioCreatedOn - leadCreatedOn).Days;

                    alt_Portfolio portfolioToUpdate = new alt_Portfolio()
                    {
                        Id = retrievedPortfolio.Id,
                        alt_ConversionTimeInDaysInt = differenceInDays
                    };
                    portfoliosToUpdate.Add(portfolioToUpdate);
                }
            }

            if (portfoliosToUpdate.Count > 0)
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, alt_Portfolio.EntityLogicalName);
                commonDAL.ExecuteMultipleRequestsInChunks(portfoliosToUpdate, RequestType.Update);
            }
        }
    }
}
