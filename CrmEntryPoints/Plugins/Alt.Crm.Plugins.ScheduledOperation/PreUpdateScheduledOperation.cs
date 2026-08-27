using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Crm.Plugins.ScheduledOperation
{
    public class PreUpdateScheduledOperation : PluginBase
    {
        public PreUpdateScheduledOperation(string unsecure, string secure)
            : base(typeof(PreUpdateScheduledOperation))
        {
        }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_ScheduledOperation targetScheduledOperation = localContext.TargetEntity?.ToEntity<alt_ScheduledOperation>();
            alt_ScheduledOperation preScheduledOperation = localContext.PreEntity?.ToEntity<alt_ScheduledOperation>();

            ScheduledOperationBL scheduledOperationBl = new ScheduledOperationBL(localContext.ToGlobal());
            scheduledOperationBl.SetScheduledOperationOperationStartTime(targetScheduledOperation);
            scheduledOperationBl.SetScheduledOperationOperationRunTime(targetScheduledOperation, preScheduledOperation);
        }
    }
}
