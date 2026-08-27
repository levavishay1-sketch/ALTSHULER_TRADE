using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using System;

namespace Alt.DataAccessLayer.Crm
{
    public class IncidentStatusDAL : CrmBaseDAL<alt_IncidentStatus>
    {
        public IncidentStatusDAL(GlobalContext globalContext) : base(globalContext, alt_IncidentStatus.EntityLogicalName)
        {
        }
    }
}
