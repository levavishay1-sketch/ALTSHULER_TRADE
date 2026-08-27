using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class AutomaticIncidentTemplateDAL : CrmBaseDAL<alt_AutomaticIncidentTemplate>
    {
        public AutomaticIncidentTemplateDAL(GlobalContext globalContext) : base(globalContext, alt_AutomaticIncidentTemplate.EntityLogicalName)
        {
        }
    }
}
