using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;


namespace Alt.DataAccessLayer.Crm.External
{
    public class CustomerOperationTemplateDAL : CrmExternalBaseDAL<ApiCustomerOperationTemplate>
    {
        public CustomerOperationTemplateDAL(GlobalContext globalContext) : base(globalContext, ApiCustomerOperationTemplate.EntityLogicalName) { }

    }
}
