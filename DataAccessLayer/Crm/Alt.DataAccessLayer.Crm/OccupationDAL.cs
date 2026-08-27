using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class OccupationDAL : CrmBaseDAL<alt_Occupation>
    {
        public OccupationDAL(GlobalContext globalContext) : base(globalContext, alt_Occupation.EntityLogicalName)
        {
        }
    }
}
