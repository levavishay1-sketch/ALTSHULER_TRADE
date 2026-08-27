using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class SystemUserBL : CrmBaseBL
    {
        public SystemUserBL(GlobalContext globalContext) : base(globalContext) { }

        public bool IsCallingUserApplicationUser()
        {
            this.GlobalContext.LogEntry();

            SystemUserDAL systemUserDal = new SystemUserDAL(this.GlobalContext);
            List<SystemUser> applicationSystemUsers = systemUserDal.GetApplicationUsers();
            return applicationSystemUsers.Where(s => s.Id == this.GlobalContext.UserId).FirstOrDefault() != null;
        }
    }
}
