using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class EmailTemplateDAL : CrmBaseDAL<alt_EmailTemplate>
    {
        public EmailTemplateDAL(GlobalContext globalContext) : base(globalContext, alt_EmailTemplate.EntityLogicalName)
        {
        }
    }
}
