using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalForm
{
    public class AsyncCreateDigitalForm : PluginBase
    {
        public AsyncCreateDigitalForm(string unsecure, string secure) : base(typeof(AsyncCreateDigitalForm)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalForm targetDigitalForm = localContext.TargetEntity?.ToEntity<alt_DigitalForm>();

            DigitalFormBL digitalFormBl = new DigitalFormBL(localContext.ToGlobal());
            digitalFormBl.HandleDigitalFormLink(targetDigitalForm);
        }
    }
}
