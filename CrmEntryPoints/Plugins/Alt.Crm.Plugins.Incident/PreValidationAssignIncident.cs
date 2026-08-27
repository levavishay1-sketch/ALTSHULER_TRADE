using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Incident
{
    public class PreValidationAssignIncident : PluginBase
    {
        public PreValidationAssignIncident(string unsecure, string secure) : base(typeof(PreValidationAssignIncident)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            IncidentBL incidentBl = new IncidentBL(localContext.ToGlobal());
            incidentBl.ChangeAssigneeWhenAssignedToUser(localContext.PluginExecutionContext.InputParameters);
        }
    }
}
