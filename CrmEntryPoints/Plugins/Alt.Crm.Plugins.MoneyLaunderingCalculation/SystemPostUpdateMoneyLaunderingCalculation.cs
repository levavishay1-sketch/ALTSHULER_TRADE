using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.MoneyLaunderingCalculation
{
    public class SystemPostUpdateMoneyLaunderingCalculation : PluginBase
    {
        public SystemPostUpdateMoneyLaunderingCalculation(string unsecure, string secure)
            : base(typeof(SystemPostUpdateMoneyLaunderingCalculation), false)
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_MoneyLaunderingCalculation targetMoneyLaunderingCalculation =  localContext.TargetEntity?.ToEntity<alt_MoneyLaunderingCalculation>();

            alt_MoneyLaunderingCalculation preMoneyLaunderingCalculation =    localContext.PreEntity?.ToEntity<alt_MoneyLaunderingCalculation>();

            ManagerControlChangeTrackingBL managerControlChangeTrackingBL = new ManagerControlChangeTrackingBL(localContext.ToGlobal());

            managerControlChangeTrackingBL.TrackChanges(  targetMoneyLaunderingCalculation,  preMoneyLaunderingCalculation);
            managerControlChangeTrackingBL.MoveLastAuthorizationManagementBack(targetMoneyLaunderingCalculation, preMoneyLaunderingCalculation);

        }
    }
}