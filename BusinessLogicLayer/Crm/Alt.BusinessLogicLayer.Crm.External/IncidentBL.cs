using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class IncidentBL : ExternalBLBase
    {
        public IncidentBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void Update(ApiIncident apiIncident)
        {
            this.GlobalContext.LogEntry();

            new CommonDAL(this.GlobalContext, ApiIncident.EntityLogicalName).Update(apiIncident);
        }
    }
}
