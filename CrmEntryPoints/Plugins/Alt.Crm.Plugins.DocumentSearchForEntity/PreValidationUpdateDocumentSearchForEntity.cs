//using Alt.BusinessLogicLayer.Crm;
//using Alt.DataModel.Crm.Entities;
//using Alt.Framework.EntryPoints.Crm;

//namespace Alt.Crm.Plugins.DocumentSearchForEntity
//{
//    public class PreValidationUpdateDocumentSearchForEntity : PluginBase
//    {
//        public PreValidationUpdateDocumentSearchForEntity(string unsecure, string secure) : base(typeof(PreValidationUpdateDocumentSearchForEntity)) { }

//        protected override void ExecuteCrmPlugin(LocalContext localContext)
//        {
//            alt_DocumentSearchForEntity targetDocumentSearchForEntity = localContext.TargetEntity.ToEntity<alt_DocumentSearchForEntity>();
//            DocumentSearchForEntityBL documentSearchForEntityBL = new DocumentSearchForEntityBL(localContext.ToGlobal());
//            documentSearchForEntityBL.HandleSearchStatus(targetDocumentSearchForEntity);
//        }
//    }
//}
