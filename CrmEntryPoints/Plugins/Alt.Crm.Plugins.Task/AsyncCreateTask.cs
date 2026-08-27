using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Task
{
    public class AsyncCreateTask : PluginBase
    {
        public AsyncCreateTask(string unsecure, string secure) : base(typeof(AsyncCreateTask)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Task targetTask = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Task>();

            TaskBL taskBL = new TaskBL(localContext.ToGlobal());
            taskBL.HandleLeadScheduledTreatmentDate(targetTask, null);
        }
    }
}
