using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalForm
{
   public class PreCreateDigitalForm: PluginBase
    {
        public PreCreateDigitalForm(string unsecure, string secure) : base(typeof(PreCreateDigitalForm)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalForm targetDigitalForm = localContext.TargetEntity?.ToEntity<alt_DigitalForm>();

            DigitalFormBL digitalFormBl = new DigitalFormBL(localContext.ToGlobal());
            digitalFormBl.HandleRegardingObject(targetDigitalForm);
            digitalFormBl.HandleTransferToOutSystemStatusCode(targetDigitalForm, targetDigitalForm);
        }
    }
}
