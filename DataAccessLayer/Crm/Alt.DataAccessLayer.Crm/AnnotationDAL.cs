using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class AnnotationDAL : CrmBaseDAL<Annotation>
    {
        public AnnotationDAL(GlobalContext globalContext) : base(globalContext, Annotation.EntityLogicalName)
        {
        }
    }
}
