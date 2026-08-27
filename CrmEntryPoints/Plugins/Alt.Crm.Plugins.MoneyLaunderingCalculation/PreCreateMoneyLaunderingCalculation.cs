using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.MoneyLaunderingCalculation
{
    public class PreCreateMoneyLaunderingCalculation : PluginBase
    {
        public PreCreateMoneyLaunderingCalculation(string unsecure, string secure) : base(typeof(PreCreateMoneyLaunderingCalculation)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_MoneyLaunderingCalculation targetMoneyLaunderingCalculation = localContext.TargetEntity?.ToEntity<alt_MoneyLaunderingCalculation>();
            MoneyLaunderingCalculationBL moneyLaunderingCalculationBL = new MoneyLaunderingCalculationBL(localContext.ToGlobal());
            moneyLaunderingCalculationBL.PopulateFieldsForMoneyLaunderingCalcultion(targetMoneyLaunderingCalculation);
        }
    }
}
