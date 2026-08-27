using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class PortfolioDAL : CrmExternalBaseDAL<ApiPortfolio>
    {
        public PortfolioDAL(GlobalContext globalContext) : base(globalContext, ApiPortfolio.EntityLogicalName) { }
    }
}
