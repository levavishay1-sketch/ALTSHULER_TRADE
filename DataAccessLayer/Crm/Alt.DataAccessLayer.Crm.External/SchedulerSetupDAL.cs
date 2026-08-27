using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class SchedulerSetupDAL : CrmExternalBaseDAL<ApiSchedulerSetup>
    {
        public SchedulerSetupDAL(GlobalContext globalContext)
            : base(globalContext, ApiSchedulerSetup.EntityLogicalName) { }
    }
}
