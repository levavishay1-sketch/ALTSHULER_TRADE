using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExternalServices.ESB;
using Alt.Framework;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBFeezbackLinkRequestDAL : ExternalServicesBaseDAL<ESBFeezbackLinkRequest, ApiAccountHolder>
    {
        public ESBFeezbackLinkRequestDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {

        }

        protected override ESBFeezbackLinkRequest MapApiEntityToTargetModel(ApiAccountHolder apiAccountHolder)
        {
            this.GlobalContext.LogEntry();

            ESBFeezbackLinkRequest eSBFeezbackLinkRequest = new ESBFeezbackLinkRequest
            {
                ContactIdNumber = apiAccountHolder.IdentificationNumber,
                FirstName = apiAccountHolder.FirstName,
                LastName = apiAccountHolder.LastName,
                Email = apiAccountHolder.Email,
                Phone = apiAccountHolder.MobilePhone,
                BankName = apiAccountHolder.DigitalFormVerification.Bank.Name,
                BankCode = apiAccountHolder.DigitalFormVerification.Bank.Code,
                BankBranchNumber = apiAccountHolder.DigitalFormVerification.Branch.BranchNumber,
                BankBranchName = apiAccountHolder.DigitalFormVerification.Branch.BranchName,
                BankAccountNumber = apiAccountHolder.DigitalFormVerification.BankAccountNumber,
                ProductAccountNumber = string.Empty,
                BankId = string.Empty,
                Amount = "5000",
            };
            if (base.ApiConfiguration.TryGetSettingsItemValue("Constants", out Dictionary<string, string> settings))
            {
                eSBFeezbackLinkRequest.ProductId = settings[nameof(eSBFeezbackLinkRequest.ProductId)];
                eSBFeezbackLinkRequest.AgentId = settings[nameof(eSBFeezbackLinkRequest.AgentId)];
                eSBFeezbackLinkRequest.CompanyId = settings[nameof(eSBFeezbackLinkRequest.CompanyId)];
                eSBFeezbackLinkRequest.Source = settings[nameof(eSBFeezbackLinkRequest.Source)];
            }

            return eSBFeezbackLinkRequest;
        }
    }
}
