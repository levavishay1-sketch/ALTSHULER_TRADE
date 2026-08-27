using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.IncidentStatusLog
{
    public class PreValidationCreateIncidentStatusLog: PluginBase
    {
        public PreValidationCreateIncidentStatusLog(string unsecure, string secure): base(typeof(PreValidationCreateIncidentStatusLog)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_IncidentStatusLog targetIncidentStatusLog = localContext.TargetEntity != null ?
                   localContext.TargetEntity.ToEntity<alt_IncidentStatusLog>() : null;

            IncidentStatusLogBL incidentStatusLogBl = new IncidentStatusLogBL(localContext.ToGlobal());
            incidentStatusLogBl.HandleOwner(targetIncidentStatusLog);
        }
    }
}
