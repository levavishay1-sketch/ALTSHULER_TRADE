using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Portfolio
{
    public class PostCreatePortfolio : PluginBase
    {
        public PostCreatePortfolio(string unsecure, string secure) : base(typeof(PostCreatePortfolio)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_Portfolio targetPortfolio = localContext.TargetEntity?.ToEntity<alt_Portfolio>();

            PortfolioBL portfolioBl = new PortfolioBL(localContext.ToGlobal());
            portfolioBl.LinkAccountHoldersToPortfolio(targetPortfolio);
        }
    }
}
