using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class PhoneCallDAL : CrmExternalBaseDAL<ApiPhoneCall>
    {
        public PhoneCallDAL(GlobalContext globalContext) : base(globalContext, ApiPhoneCall.EntityLogicalName) { }
    }
}
