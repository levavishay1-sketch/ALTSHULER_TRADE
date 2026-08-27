using Alt.Framework;

namespace Alt.BusinessLogicLayer.Crm
{
    public abstract class CrmBaseBL
    {
        protected GlobalContext GlobalContext { get; private set; }

        public CrmBaseBL(GlobalContext globalContext)
        {
            GlobalContext = globalContext;
        }
    }
}
