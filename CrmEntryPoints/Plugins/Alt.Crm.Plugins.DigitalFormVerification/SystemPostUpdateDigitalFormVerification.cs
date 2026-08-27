using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.DigitalFormVerification
{
    public class SystemPostUpdateDigitalFormVerification : PluginBase
    {
        public SystemPostUpdateDigitalFormVerification(string unsecure, string secure) : base(typeof(SystemPostUpdateDigitalFormVerification),false) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_DigitalFormVerification targetDigitalFormVerification = localContext.TargetEntity?.ToEntity<alt_DigitalFormVerification>();
            alt_DigitalFormVerification preDigitalFormVerification = localContext.PreEntity?.ToEntity<alt_DigitalFormVerification>();

            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());
            managerControlChangeTrackingBL.TrackChanges(targetDigitalFormVerification, preDigitalFormVerification);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetDigitalFormVerification, preDigitalFormVerification);
        }
    }
}