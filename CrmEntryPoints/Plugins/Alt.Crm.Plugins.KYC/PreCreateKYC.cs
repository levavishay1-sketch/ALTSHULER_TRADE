using Alt.Framework.EntryPoints.Crm;
using Alt.BusinessLogicLayer.Crm;

namespace Alt.Crm.Plugins.KYC
{
    public class PreCreateKYC : PluginBase
    {
        public PreCreateKYC(string unsecure, string secure) : base(typeof(PreCreateKYC)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.alt_KYC targetKYC = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.alt_KYC>();

            KYCBL kycBl = new KYCBL(localContext.ToGlobal());
            kycBl.SetDefaultValues(targetKYC);
            kycBl.SetKYCName(targetKYC, targetKYC);
            kycBl.HandleBankServiceDenialUpdate(targetKYC);
            kycBl.HandleAdditionalAccountExistsatAltshulerUpdate(targetKYC);
            kycBl.HandleFundsSourceUpdate(targetKYC);
            kycBl.HandleEmploymentTypeCode(targetKYC);
            kycBl.HandleMonthlyIncomeLevelNIS(targetKYC);
            kycBl.HandleEmploymentCategoryOccupationId(targetKYC);
            kycBl.HandleManualHandlingReasonsCode(targetKYC, targetKYC);
            kycBl.SetFildsScoreTheCalculatorSection(targetKYC);
            kycBl.SetRelatedPortfolioCustomerId(targetKYC);
        }
    }
}