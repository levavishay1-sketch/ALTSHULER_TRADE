using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.IncidentStatusLog
{
    public class SystemAsyncCreateIncidentStatusLog : PluginBase
    {
        public SystemAsyncCreateIncidentStatusLog(string unsecure, string secure) : base(typeof(SystemAsyncCreateIncidentStatusLog), false) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            alt_IncidentStatusLog targetIncidentStatusLog = localcontext.TargetEntity != null ?
                   localcontext.TargetEntity.ToEntity<alt_IncidentStatusLog>() : null;

            IncidentStatusLogBL incidentStatusLogBl = new IncidentStatusLogBL(localcontext.ToGlobal());
            incidentStatusLogBl.HandleCreateSmsAndEmails(targetIncidentStatusLog);
        }
    }
}
