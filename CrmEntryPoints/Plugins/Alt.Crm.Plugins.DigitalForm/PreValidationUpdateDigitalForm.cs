using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalForm
{
    public class PreValidationUpdateDigitalForm : PluginBase
    {
        public PreValidationUpdateDigitalForm(string unsecure, string secure) : base(typeof(PreValidationUpdateDigitalForm)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalForm targetDigitalForm = localContext.TargetEntity?.ToEntity<alt_DigitalForm>();
            alt_DigitalForm preDigitalForm = localContext.PreEntity?.ToEntity<alt_DigitalForm>();

            DigitalFormBL digitalFormBl = new DigitalFormBL(localContext.ToGlobal());
            digitalFormBl.HandleDataRecipientRetry(targetDigitalForm);
            digitalFormBl.SetDigitalFormStatusByDataReceptionStatus(targetDigitalForm, preDigitalForm);
            digitalFormBl.SetDigitalFormStatusOnManualCreateInOS(targetDigitalForm);
            digitalFormBl.SetDigitalFormStatusByDataDuplicateDigitalFormLink(targetDigitalForm, preDigitalForm);
            digitalFormBl.SetStateCodeAndStatusCodeByDigitalFormStatusId(targetDigitalForm, preDigitalForm);
        }
    }
}
