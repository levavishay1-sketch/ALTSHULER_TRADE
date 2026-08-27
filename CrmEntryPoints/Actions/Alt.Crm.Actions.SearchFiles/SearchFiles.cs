using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Actions.SearchFiles
{
    public class SearchFiles : PluginBase
    {
        public SearchFiles(string unsecure, string secure) : base(typeof(SearchFiles)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            ArchiveDocumentSearchBL archiveDocumentSearchBL = new ArchiveDocumentSearchBL(localContext.ToGlobal());
            archiveDocumentSearchBL.SearchFilesFromCustomAction(localContext.PluginExecutionContext.InputParameters);
        }
    }
}
