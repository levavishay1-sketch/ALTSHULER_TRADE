//using Alt.BusinessLogicLayer.Crm;
//using Alt.DataModel.Crm.Entities;
//using Alt.Framework.EntryPoints.Crm;

//namespace Alt.Crm.Plugins.Incident
//{
//    public class PostUpdateIncident : PluginBase
//    {
//        public PostUpdateIncident(string unsecure, string secure) : base(typeof(PostUpdateIncident)) { }

//        protected override void ExecuteCrmPlugin(LocalContext localcontext)
//        {
//            DataModel.Crm.Entities.Incident targetIncident = localcontext.TargetEntity != null ?
//                localcontext.TargetEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

//            DataModel.Crm.Entities.Incident preIncident = localcontext.PreEntity != null ?
//             localcontext.PreEntity.ToEntity<DataModel.Crm.Entities.Incident>() : null;

//            string parentPrimaryEntityName = localcontext.PluginExecutionContext.ParentContext != null ?
//                localcontext.PluginExecutionContext.ParentContext.PrimaryEntityName : null;
//            if (parentPrimaryEntityName != alt_IncidentStatusLog.EntityLogicalName)
//            {
//                IncidentBL incidentBl = new IncidentBL(localcontext.ToGlobal());
//                incidentBl.HandleIncidentStatusLogCreate(targetIncident, preIncident);
//            }
//        }
//    }
//}
