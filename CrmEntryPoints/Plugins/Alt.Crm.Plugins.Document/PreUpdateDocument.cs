using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Document
{
    public class PreUpdateDocument : PluginBase
    {
        public PreUpdateDocument(string unsecure, string secure) : base(typeof(PreUpdateDocument)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_Document targetDocumentEntity = localContext.TargetEntity?.ToEntity<alt_Document>();
            DocumentBL documentBL = new DocumentBL(localContext.ToGlobal());
            documentBL.HandleFileUploadStatus(targetDocumentEntity);
            documentBL.HandleFileDownloadStatus(targetDocumentEntity);
            documentBL.HandleFileUpdateStatus(targetDocumentEntity);
        }
    }
}
