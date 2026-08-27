using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class SystemPostCreateAccountHolder : PluginBase
    {
        public SystemPostCreateAccountHolder(string unsecure, string secure) : base(typeof(SystemPostCreateAccountHolder), false) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();
            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());
            managerControlChangeTrackingBL.TrackChanges(targetAccountHolder);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetAccountHolder);

        }
    }
}