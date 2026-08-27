using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class DigitalFormStatusDAL : CrmBaseDAL<alt_DigitalFormStatus>
    {
        string[] attributesToRetrieve =
{
            alt_DigitalFormStatus.Fields.StateCode,
            alt_DigitalFormStatus.Fields.alt_DigitalFromStatusCode,
            alt_DigitalFormStatus.Fields.alt_Code,
            alt_DigitalFormStatus.Fields.alt_OpportunityStatusCode,
            alt_DigitalFormStatus.Fields.alt_Name,
            alt_DigitalFormStatus.Fields.alt_LeadStatusCode
        };
        public DigitalFormStatusDAL(GlobalContext globalContext) : base(globalContext, alt_DigitalFormStatus.EntityLogicalName)
        {
        }

        public alt_DigitalFormStatus GetDigitalFormStatusDetails(Guid id, string[] columns = null)
        {
            this.GlobalContext.LogEntry( $"Id : {id}");
            return base.Get(id, columns ?? attributesToRetrieve);
        }
    }
}
