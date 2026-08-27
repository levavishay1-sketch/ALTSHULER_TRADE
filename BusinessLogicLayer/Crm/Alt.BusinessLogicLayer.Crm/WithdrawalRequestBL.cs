using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class WithdrawalRequestBL : CrmBaseBL
    {
        public WithdrawalRequestBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetWithdrawalRequestName(alt_WithdrawalRequest targetWithdrawalRequest)
        {
            this.GlobalContext.LogEntry();

            List<string> nameParts = new List<string>();

            if (targetWithdrawalRequest.AttributeHasValue<EntityReference>(alt_WithdrawalRequest.Fields.alt_CustomerId))
            {
                nameParts.Add(new CustomerBL(this.GlobalContext).
                    GetCustomerName(targetWithdrawalRequest.alt_CustomerId));
            }

            if (targetWithdrawalRequest.AttributeHasValue<EntityReference>(alt_WithdrawalRequest.Fields.alt_PortfolioId))
            {
                PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
                alt_Portfolio retrievedPortfolio = portfolioDAL.Get(targetWithdrawalRequest.alt_PortfolioId.Id, new string[] { alt_Portfolio.Fields.alt_ShenhavAccountNumber });
                nameParts.Add(retrievedPortfolio.alt_ShenhavAccountNumber);
            }

            targetWithdrawalRequest.alt_Name = string.Join(" - ", nameParts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
