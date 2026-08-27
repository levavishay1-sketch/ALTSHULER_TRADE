using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class SystemUserBL : ExternalBLBase
    {
        public SystemUserBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult Get(Guid id)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            SystemUserDAL systemUserDal = new SystemUserDAL(this.GlobalContext);
            ApiSystemUser apiSystemUser = systemUserDal.Get(id, new string[] { "domainname" });
            actionResult.ReturnObject = new ApiSystemUser() { DomainName = apiSystemUser.DomainName };
            return actionResult;
        }
    }
}
