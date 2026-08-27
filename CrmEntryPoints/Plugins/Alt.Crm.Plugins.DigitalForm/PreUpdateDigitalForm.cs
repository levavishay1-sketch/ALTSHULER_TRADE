using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalForm
{
    public class PreUpdateDigitalForm : PluginBase
    {
        public PreUpdateDigitalForm(string unsecure, string secure) : base(typeof(PreUpdateDigitalForm)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalForm targetDigitalForm = localContext.TargetEntity?.ToEntity<alt_DigitalForm>();
            alt_DigitalForm preDigitalForm = localContext.PreEntity?.ToEntity<alt_DigitalForm>();

            DigitalFormBL digitalFormBl = new DigitalFormBL(localContext.ToGlobal());
            digitalFormBl.HandleAbandonedJoiningProcess(targetDigitalForm, preDigitalForm);
            digitalFormBl.HandleTransferToOutSystemStatusCode(targetDigitalForm, preDigitalForm);
        }
    }
}
