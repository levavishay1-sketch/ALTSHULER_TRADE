using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.IncidentStatusLog
{
    public class PostCreateIncidentStatusLog: PluginBase
    {
        public PostCreateIncidentStatusLog(string unsecure, string secure): base(typeof(PostCreateIncidentStatusLog), false) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            alt_IncidentStatusLog targetIncidentStatusLog = localcontext.TargetEntity != null ?
                   localcontext.TargetEntity.ToEntity<alt_IncidentStatusLog>() : null;

            IncidentStatusLogBL incidentStatusLogBL = new IncidentStatusLogBL(localcontext.ToGlobal());
            incidentStatusLogBL.HandleRelatedIncidentUpdate(targetIncidentStatusLog);
        }
    }
}
