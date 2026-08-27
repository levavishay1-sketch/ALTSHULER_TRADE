using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.ArchiveDocumentSearch
{
    public class PreUpdateArchiveDocumentSearch : PluginBase
    {
        public PreUpdateArchiveDocumentSearch(string unsecure, string secure) : base(typeof(PreUpdateArchiveDocumentSearch)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_ArchiveDocumentSearch targetArchiveDocumentSearch = localContext.TargetEntity.ToEntity<alt_ArchiveDocumentSearch>();
            ArchiveDocumentSearchBL archiveDocumentSearchBL = new ArchiveDocumentSearchBL(localContext.ToGlobal());
            archiveDocumentSearchBL.HandleSearchStatus(targetArchiveDocumentSearch);
        }
    }
}
