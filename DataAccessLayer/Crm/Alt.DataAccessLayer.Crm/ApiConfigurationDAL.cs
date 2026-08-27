using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class ApiConfigurationDAL : CrmBaseDAL<alt_ApiConfiguration>
    {
        public ApiConfigurationDAL(GlobalContext globalContext) : base(globalContext, alt_ApiConfiguration.EntityLogicalName)
        {
        }
    }
}
