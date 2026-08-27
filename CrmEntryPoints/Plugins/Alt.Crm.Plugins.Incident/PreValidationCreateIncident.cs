using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
    public class PreValidationCreateIncident: PluginBase
    {
        public PreValidationCreateIncident(string unsecure, string secure) : base(typeof(PreValidationCreateIncident)) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localcontext.TargetEntity != null ?
               localcontext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            IncidentBL incidentBl = new IncidentBL(localcontext.ToGlobal());
            incidentBl.HandleAutoIncidentCreation(targetIncident);
            incidentBl.ValidateIncidentOnCreate(targetIncident);
        }
    }
}
