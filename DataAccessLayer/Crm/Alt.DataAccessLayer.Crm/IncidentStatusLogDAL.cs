using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class IncidentStatusLogDAL : CrmBaseDAL<alt_IncidentStatusLog>
    {
        public IncidentStatusLogDAL(GlobalContext globalContext) : base(globalContext, alt_IncidentStatusLog.EntityLogicalName)
        {
        }
    }
}
