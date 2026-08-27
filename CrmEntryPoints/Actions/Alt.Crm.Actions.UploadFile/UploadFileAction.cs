using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Actions.UploadFile
{
    public class UploadFileAction : PluginBase
    {
        public UploadFileAction(string unsecure, string secure) : base(typeof(UploadFileAction)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DocumentBL documentBL = new DocumentBL(localContext.ToGlobal());
            documentBL.UploadFileFromCustomAction(localContext.PluginExecutionContext.InputParameters);
        }
    }
}
