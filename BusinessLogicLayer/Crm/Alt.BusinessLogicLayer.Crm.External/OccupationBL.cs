using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class OccupationBL : ExternalBLBase
    {
        public OccupationBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult Get()
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            OccupationDAL occupationDal = new OccupationDAL(this.GlobalContext);
            List<ApiOccupation> occupations = occupationDal.GetAll();
            actionResult.ReturnObject = occupations;
            return actionResult;
        }
    }
}
