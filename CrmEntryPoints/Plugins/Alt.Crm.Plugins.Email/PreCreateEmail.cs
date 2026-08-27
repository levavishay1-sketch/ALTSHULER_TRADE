using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Email
{
    public class PreCreateEmail : PluginBase
    {
        public PreCreateEmail(string unsecure, string secure) : base(typeof(PreCreateEmail)) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            var targetEmail = localcontext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Email>();
            EmailBL emailBl = new EmailBL(localcontext.ToGlobal());

            emailBl.HandleEmailAutomaticCreationByTemplateCode(targetEmail);
            emailBl.SetOwner(targetEmail);
        }
    }
}
