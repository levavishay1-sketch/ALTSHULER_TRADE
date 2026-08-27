using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Crm.Plugins.IncidentStatusLog
{
    public class PreCreateIncidentStatusLog : PluginBase
    {
        public PreCreateIncidentStatusLog(string unsecure, string secure)
            : base(typeof(PreCreateIncidentStatusLog))
        {

        }

        protected override void ExecuteCrmPlugin(LocalContext localcontext)
        {
            alt_IncidentStatusLog targetIncidentStatusLog = localcontext.TargetEntity != null ?
                   localcontext.TargetEntity.ToEntity<alt_IncidentStatusLog>() : null;

            IncidentStatusLogBL incidentStatusLogBl = new IncidentStatusLogBL(localcontext.ToGlobal());

            incidentStatusLogBl.ValidateStatusLog(targetIncidentStatusLog);
            incidentStatusLogBl.MapFieldsFromIncident(targetIncidentStatusLog);
            incidentStatusLogBl.SetIncidentStatusLogTitle(targetIncidentStatusLog);
        }
    }
}
