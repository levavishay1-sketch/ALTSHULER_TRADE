using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Actions.TemplateGenerator
{
    public class TemplateGenerator : PluginBase
    {

        private string crmUrl;
        public TemplateGenerator(string unsecure, string secure)
            : base(typeof(TemplateGenerator))
        {
            this.crmUrl = secure;
        }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            ParseActivityMessageBL parseActivityMessageBL = new ParseActivityMessageBL(localcontext.ToGlobal());
            parseActivityMessageBL.HandleParseActivityMessageHandler(localcontext.PluginExecutionContext.InputParameters, localcontext.PluginExecutionContext.OutputParameters, this.crmUrl);
        }
    }
}
