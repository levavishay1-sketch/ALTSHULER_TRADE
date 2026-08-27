using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Task
{
    public class AsyncUpdateTask : PluginBase
    {
        public AsyncUpdateTask(string secure, string unsecure) : base(typeof(AsyncUpdateTask)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Task targetTask = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Task>();
            DataModel.Crm.Entities.Task preTask = localContext.PreEntity?.ToEntity<DataModel.Crm.Entities.Task>();

            TaskBL taskBL = new TaskBL(localContext.ToGlobal());
            taskBL.HandleLeadScheduledTreatmentDate(targetTask, preTask);
        }
    }
}
