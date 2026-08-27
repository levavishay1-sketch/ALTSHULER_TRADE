using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;
using Alt.Framework.Extensions;

namespace Alt.Crm.Plugins.Incident
{
    public class PreValidationUpdateIncident : PluginBase
    {
        public PreValidationUpdateIncident(string unsecure, string secure) : base(typeof(PreValidationUpdateIncident)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localContext.TargetEntity != null ?
                localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            DataModel.Crm.Entities.Incident preIncident = localContext.PreEntity != null ?
             localContext.PreEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            IncidentBL incidentBl = new IncidentBL(localContext.ToGlobal());

            incidentBl.ValidateIncidentOnUpdate(targetIncident);
            incidentBl.HandleReOpenClosedIncident(targetIncident, preIncident);
        }
    }
}
