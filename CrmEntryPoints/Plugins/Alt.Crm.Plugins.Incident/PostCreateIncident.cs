using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
    public class PostCreateIncident : PluginBase
    {
        public PostCreateIncident(string unsecure, string secure) : base(typeof(PostCreateIncident)) { }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            DataModel.Crm.Entities.Incident targetIncident = localcontext.TargetEntity != null ?
                localcontext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

            string parentPrimaryEntityName = localcontext.PluginExecutionContext.ParentContext != null ?
                localcontext.PluginExecutionContext.ParentContext.PrimaryEntityName : null;
            if (parentPrimaryEntityName != alt_IncidentStatusLog.EntityLogicalName)
            {
                IncidentBL incidentBl = new IncidentBL(localcontext.ToGlobal());
                incidentBl.HandleIncidentStatusLogCreate(targetIncident, targetIncident);
            }
        }
    }
}
