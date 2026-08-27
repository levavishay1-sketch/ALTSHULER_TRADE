using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.ScheduledOperation
{
    public class PreValidationCreateScheduledOperation: PluginBase
    {
        public PreValidationCreateScheduledOperation(string unsecure, string secure)
            : base(typeof(PreValidationCreateScheduledOperation))
        {
        }
        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            alt_ScheduledOperation targetScheduledOperation = localcontext.TargetEntity != null ?
                localcontext.TargetEntity.ToEntity<alt_ScheduledOperation>() : null;

            ScheduledOperationBL scheduledOperationBl = new ScheduledOperationBL(localcontext.ToGlobal());
            scheduledOperationBl.Validate(targetScheduledOperation);
            scheduledOperationBl.HandleScheduledOperationData(targetScheduledOperation);
            scheduledOperationBl.HandleScheduledOperationStateCode(targetScheduledOperation);
        }
    }
}
