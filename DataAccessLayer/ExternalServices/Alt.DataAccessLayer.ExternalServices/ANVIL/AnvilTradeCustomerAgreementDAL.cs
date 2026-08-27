using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ANVIL
{
    public class AnvilTradeCustomerAgreementDAL : AnvilBaseDAL<ApiAccountHolder>
    {

        public AnvilTradeCustomerAgreementDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration, ApiPDFProductionTemplate pdfTemplateSetting, string pdfParsedData = null)
            : base(globalContext, apiConfiguration, pdfTemplateSetting, pdfParsedData)
        {
        }

        protected override dynamic GeneratePdfData(ApiAccountHolder apiEntity)
        {
            this.GlobalContext.LogEntry();
            throw new NotImplementedException();
        }   
    }
}
