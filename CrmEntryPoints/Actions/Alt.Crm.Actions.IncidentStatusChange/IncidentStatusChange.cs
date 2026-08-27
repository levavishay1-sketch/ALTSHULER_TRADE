using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Actions.IncidentStatusChange
{
    public class IncidentStatusChange : PluginBase
    {
        public IncidentStatusChange(string unsecure, string secure)
        : base(typeof(IncidentStatusChange)){ }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            IncidentBL incidentBl = new IncidentBL(localContext.ToGlobal());
            incidentBl.ChangeIncidentStatusFromAction(localContext.PluginExecutionContext.InputParameters);
        }
    }
}
