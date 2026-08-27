using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalFormVerification
{
    public class PreValidationAssignDigitalFormVerification : PluginBase
    {
        public PreValidationAssignDigitalFormVerification(string unsecure, string secure) 
            : base(typeof(PreValidationAssignDigitalFormVerification)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalFormVerification targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();

            DigitalFormVerificationBL digitalFormVerificationBl = new DigitalFormVerificationBL(localContext.ToGlobal());
            digitalFormVerificationBl.ChangeAssigneeWhenAssignedToUser(localContext.PluginExecutionContext.InputParameters);
        }
    }
}
