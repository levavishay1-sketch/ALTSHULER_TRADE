using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class SmsTemplateDAL : CrmExternalBaseDAL<ApiSmsTemplate>
    {
        public SmsTemplateDAL(GlobalContext globalContext) : base(globalContext, ApiSmsTemplate.EntityLogicalName) { }
    }
}
