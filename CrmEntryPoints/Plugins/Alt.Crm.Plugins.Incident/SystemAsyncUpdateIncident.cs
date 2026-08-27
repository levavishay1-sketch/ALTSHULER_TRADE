using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
   public class SystemAsyncUpdateIncident: PluginBase
    {
        public SystemAsyncUpdateIncident(string unsecure, string secure) : base(typeof(SystemAsyncUpdateIncident), false) { }


        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localcontext.TargetEntity != null ?
                localcontext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            DataModel.Crm.Entities.Incident preIncident = localcontext.PreEntity != null ?
             localcontext.PreEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            IncidentBL incidentBl = new IncidentBL(localcontext.ToGlobal());
            incidentBl.DeactivatePreviousIncidentStatusLog(targetIncident, preIncident);
        }
    }
}
