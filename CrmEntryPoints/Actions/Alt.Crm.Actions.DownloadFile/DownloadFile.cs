using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Actions.DownloadFile
{
    public class DownloadFile : PluginBase
    {
        public DownloadFile(string unsecure, string secure) : base(typeof(DownloadFile)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DocumentBL documentBL = new DocumentBL(localContext.ToGlobal());
            documentBL.DownloadFileFromCustomAction(localContext.PluginExecutionContext.InputParameters);
            localContext.PluginExecutionContext.OutputParameters["IsSuccess"] = true;
            localContext.PluginExecutionContext.OutputParameters["Response"] = "test";
        }
    }
}
