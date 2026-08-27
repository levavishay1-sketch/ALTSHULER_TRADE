using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.MoneyLaunderingCalculation
{
    public class SystemPostCreateMoneyLaunderingCalculation : PluginBase
    {
        public SystemPostCreateMoneyLaunderingCalculation(string unsecure, string secure) : base(typeof(SystemPostCreateMoneyLaunderingCalculation),false) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            var forceTypeLoad = typeof(alt_DigitalFormVerification);
            alt_MoneyLaunderingCalculation targetMoneyLaunderingCalculation = localContext.TargetEntity?.ToEntity<alt_MoneyLaunderingCalculation>();
            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());

            managerControlChangeTrackingBL.TrackChanges(targetMoneyLaunderingCalculation);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetMoneyLaunderingCalculation);

        }
    }
}