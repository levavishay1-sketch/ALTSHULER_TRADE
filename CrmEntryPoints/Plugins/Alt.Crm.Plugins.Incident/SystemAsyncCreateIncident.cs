using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
    /// <summary>
    /// THIS PLUGIN RUNS ON SYSTEM!!!
    /// </summary>
    public class SystemAsyncCreateIncident : PluginBase
    {
        public SystemAsyncCreateIncident(string unsecure, string secure) : base(typeof(SystemAsyncCreateIncident), false) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localContext.TargetEntity != null ?
               localContext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            IncidentBL incidentBl = new IncidentBL(localContext.ToGlobal()); ;
            incidentBl.HandleSubject2AnnotationCreation(targetIncident);
        }
    }
}
