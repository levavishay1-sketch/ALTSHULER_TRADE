using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class OpportunityDAL : CrmExternalBaseDAL<ApiOpportunity>
    {
        public OpportunityDAL(GlobalContext globalContext) : base(globalContext, ApiOpportunity.EntityLogicalName) { }
    }
}
