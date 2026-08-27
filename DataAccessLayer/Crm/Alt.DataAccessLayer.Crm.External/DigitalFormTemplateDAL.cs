using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class DigitalFormTemplateDAL : CrmExternalBaseDAL<ApiDigitalFormTemplate>
    {
        public DigitalFormTemplateDAL(GlobalContext globalContext) 
            : base(globalContext, ApiDigitalFormTemplate.EntityLogicalName) { }
    }
}
