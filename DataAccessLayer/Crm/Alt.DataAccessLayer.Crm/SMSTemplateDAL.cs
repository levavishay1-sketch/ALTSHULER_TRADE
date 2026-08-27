using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class SMSTemplateDAL : CrmBaseDAL<alt_SMSTemplate>
    {
        public SMSTemplateDAL(GlobalContext globalContext) : base(globalContext, alt_SMSTemplate.EntityLogicalName)
        {
        }
    }
}
