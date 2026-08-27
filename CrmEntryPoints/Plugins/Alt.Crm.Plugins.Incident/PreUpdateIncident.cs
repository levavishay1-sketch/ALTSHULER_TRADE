using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
    public class PreUpdateIncident : PluginBase
    {
        public PreUpdateIncident(string unsecure, string secure) : base(typeof(PreUpdateIncident)) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localcontext.TargetEntity != null ?
                localcontext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            DataModel.Crm.Entities.Incident preIncident = localcontext.PreEntity != null ?
             localcontext.PreEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            IncidentBL incidentBl = new IncidentBL(localcontext.ToGlobal());
            incidentBl.HandleIncidentStatusIdChanges(targetIncident, preIncident);
            incidentBl.HandleBpf(targetIncident, preIncident);
            incidentBl.SetResponsibleSystemUser(targetIncident, preIncident);
        }
    }
}
