using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.ArchiveDocumentSearch
{
    public class PreCreateArchiveDocumentSearch : PluginBase
    {
        public PreCreateArchiveDocumentSearch(string unsecure, string secure) : base(typeof(PreCreateArchiveDocumentSearch)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_ArchiveDocumentSearch targetArchiveDocumentSearch = localContext.TargetEntity.ToEntity<alt_ArchiveDocumentSearch>();
            ArchiveDocumentSearchBL archiveDocumentSearchBL = new ArchiveDocumentSearchBL(localContext.ToGlobal());
            archiveDocumentSearchBL.PopulateOwnerId(targetArchiveDocumentSearch);
            archiveDocumentSearchBL.HandleSearchStatus(targetArchiveDocumentSearch);
        }
    }
}
