using Alt.DataModel.Crm.External.Contracts;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External
{
    public class ApiPhoneCallList : ApiEntity
    {

        private List<ApiPhoneCall> callAttempts;
        public List<ApiPhoneCall> CallAttempts
        {
            get
            {
                return callAttempts;
            }
            set
            {
                this.SetProperty(value);
                this.callAttempts = value;
            }
        }
    }
}
