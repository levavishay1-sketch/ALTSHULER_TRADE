using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Contact
{
    public class PreCreateContact : PluginBase
    {
        public PreCreateContact(string unsecure, string secure) : base(typeof(PreCreateContact)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Contact targetContact = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Contact>();

            ContactBL contactBl = new ContactBL(localContext.ToGlobal());
            contactBl.SetInternalGovernmentIdHandler(targetContact);
        }
    }
}
