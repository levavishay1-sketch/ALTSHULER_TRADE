using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.KYC
{
    public class PreUpdateKYC : PluginBase
    {
        public PreUpdateKYC(string unsecure, string secure) : base(typeof(PreUpdateKYC)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.alt_KYC targetKYC = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.alt_KYC>();
            DataModel.Crm.Entities.alt_KYC preKYC = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.alt_KYC>();

            KYCBL kycBl = new KYCBL(localContext.ToGlobal());
            kycBl.SetKYCName(targetKYC, preKYC);
            kycBl.HandleBankServiceDenialUpdate(targetKYC);
            kycBl.HandleAdditionalAccountExistsatAltshulerUpdate(targetKYC);
            kycBl.HandleFundsSourceUpdate(targetKYC);
            kycBl.HandleEmploymentTypeCode(targetKYC, preKYC);
            kycBl.HandleMonthlyIncomeLevelNIS(targetKYC, preKYC);
            kycBl.HandleEmploymentCategoryOccupationId(targetKYC);
            kycBl.HandleManualHandlingReasonsCode(targetKYC, preKYC);
            kycBl.SetFildsScoreTheCalculatorSection(targetKYC);
            kycBl.SetRelatedPortfolioCustomerId(targetKYC);
        }
    }
}