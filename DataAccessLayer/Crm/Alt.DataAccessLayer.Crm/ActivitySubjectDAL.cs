using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class ActivitySubjectDAL : CrmBaseDAL<alt_ActivitySubject>
    {
        public ActivitySubjectDAL(GlobalContext globalContext) : base(globalContext, alt_ActivitySubject.EntityLogicalName)
        {
        }
    }
}
