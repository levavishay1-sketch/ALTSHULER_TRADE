using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Document
{
    public class PreValidationCreateDocument : PluginBase
    {
        public PreValidationCreateDocument(string unsecure, string secure) : base(typeof(PreValidationCreateDocument)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_Document targetDocument = localContext.TargetEntity?.ToEntity<alt_Document>();
            DocumentBL documentBL = new DocumentBL(localContext.ToGlobal());
            documentBL.PopulateOwnerId(targetDocument);
        }
    }
}
