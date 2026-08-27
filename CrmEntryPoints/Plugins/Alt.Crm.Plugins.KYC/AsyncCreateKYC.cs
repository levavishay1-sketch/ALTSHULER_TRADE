using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.KYC
{
    public class AsyncCreateKYC : PluginBase
    {
        public AsyncCreateKYC(string unsecure, string secure) : base(typeof(AsyncCreateKYC))
        {
        }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_KYC targetKYC = localContext.TargetEntity?.ToEntity<alt_KYC>();

            KYCBL kycBl = new KYCBL(localContext.ToGlobal());
            kycBl.UpdateDigitalFormVerification(targetKYC, targetKYC);
       

        }
    }
}