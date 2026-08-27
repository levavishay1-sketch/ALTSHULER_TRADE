using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.AccountHolder
{
    public class SystemPostUpdateAccountHolder : PluginBase
    {
        public SystemPostUpdateAccountHolder(string unsecure, string secure) : base(typeof(SystemPostUpdateAccountHolder), false) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_AccountHolder targetAccountHolder = localContext.TargetEntity?.ToEntity<alt_AccountHolder>();
            alt_AccountHolder preAccountHolder = localContext.PreEntity?.ToEntity<alt_AccountHolder>();
            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());
            managerControlChangeTrackingBL.TrackChanges(targetAccountHolder, preAccountHolder);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetAccountHolder, preAccountHolder);
        }
    }
}