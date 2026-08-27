using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Portfolio
{
    public class AsyncCreatePortfolio : PluginBase
    {
        public AsyncCreatePortfolio(string unsecure, string secure) : base(typeof(AsyncCreatePortfolio)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_Portfolio targetPortfolio = localContext.TargetEntity?.ToEntity<alt_Portfolio>();

            PortfolioBL portfolioBl = new PortfolioBL(localContext.ToGlobal());
            portfolioBl.CompleteJoiningProcessOnCreateViaSSIS(targetPortfolio);
        }
    }
}
