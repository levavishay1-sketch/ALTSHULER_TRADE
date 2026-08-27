using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.ScheduledOperation
{
    public class PostUpdateScheduledOperation : PluginBase
    {
        public PostUpdateScheduledOperation(string unsecure, string secure)
          : base(typeof(PostUpdateScheduledOperation))
        {
        }
        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            alt_ScheduledOperation targetScheduledOperation = localcontext.TargetEntity != null ?
                localcontext.TargetEntity.ToEntity<alt_ScheduledOperation>() : null;
            alt_ScheduledOperation preScheduledOperation = localcontext.PreEntity != null ?
                localcontext.PreEntity.ToEntity<alt_ScheduledOperation>() : null;

            ScheduledOperationBL scheduledOperationBl = new ScheduledOperationBL(localcontext.ToGlobal());
            scheduledOperationBl.HanldeSetupStatusChangeBasedOnOperationStatus(targetScheduledOperation, preScheduledOperation);
        }
    }
}
