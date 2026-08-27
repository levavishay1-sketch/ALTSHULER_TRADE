using Alt.Framework;
using OneTimeConsole.Enums;

namespace OneTimeConsole
{
    public class OperationBase
    {
        protected GlobalContext GlobalContext;
        protected OperationCode OperationCode;

        public OperationBase(GlobalContext globalContext, OperationCode operationCode)
        {
            this.GlobalContext = globalContext;
            this.OperationCode = operationCode;
        }

        public virtual void Run() { }
    }
}