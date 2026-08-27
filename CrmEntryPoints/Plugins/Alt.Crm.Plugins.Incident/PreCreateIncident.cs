using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
    public class PreCreateIncident : PluginBase
    {
        public PreCreateIncident(string unsecure, string secure): base(typeof(PreCreateIncident)) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localcontext.TargetEntity != null ?
                localcontext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            IncidentBL incidentBl = new IncidentBL(localcontext.ToGlobal());

            incidentBl.SetIncidentTitle(targetIncident);
            incidentBl.SetResponsibleSystemUser(targetIncident, targetIncident);
            incidentBl.HandleIncidentStatusIdChanges(targetIncident, targetIncident);
            incidentBl.HandleBpf(targetIncident, targetIncident);
        }
    }
}
