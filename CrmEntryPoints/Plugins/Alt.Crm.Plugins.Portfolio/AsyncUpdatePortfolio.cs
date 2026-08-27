using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Portfolio
{
    public class AsyncUpdatePortfolio : PluginBase
    {
        public AsyncUpdatePortfolio(string unsecure, string secure) : base(typeof(AsyncUpdatePortfolio)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_Portfolio targetPortfolio = localContext.TargetEntity?.ToEntity<alt_Portfolio>();

            PortfolioBL portfolioBl = new PortfolioBL(localContext.ToGlobal());
            portfolioBl.HandleDocuments(targetPortfolio);
        }
    }
}
