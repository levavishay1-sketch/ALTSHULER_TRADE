using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalFormVerification
{
    public class SystemAsyncUpdateDigitalFormVerification : PluginBase
    {
        public SystemAsyncUpdateDigitalFormVerification(string unsecure, string secure)
            : base(typeof(SystemAsyncUpdateDigitalFormVerification), false) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalFormVerification targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();
            alt_DigitalFormVerification preDigitalFormVerification = localContext.PreEntity?.ToEntity<alt_DigitalFormVerification>();

            DigitalFormVerificationBL digitalFormVerificationBl = new DigitalFormVerificationBL(localContext.ToGlobal());
            digitalFormVerificationBl.HandleRepresentativeRewardCreate(targetDigitalFormVerification, preDigitalFormVerification);
            digitalFormVerificationBl.LinkRepresentativeRewardsToPortfolio(targetDigitalFormVerification, preDigitalFormVerification);
            digitalFormVerificationBl.HandleLinkPortfolioToJoiningProcessSummary(targetDigitalFormVerification, preDigitalFormVerification);
        }
    }
}
