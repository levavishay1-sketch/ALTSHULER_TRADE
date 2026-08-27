using Alt.Framework;
using Microsoft.Xrm.Sdk;

namespace Alt.DataAccessLayer.Crm
{
    public class ParseActivityMessageDAL : CrmBaseDAL<Entity>
    {
        public ParseActivityMessageDAL(GlobalContext globalContext) : base(globalContext, null) { }
    }
}
