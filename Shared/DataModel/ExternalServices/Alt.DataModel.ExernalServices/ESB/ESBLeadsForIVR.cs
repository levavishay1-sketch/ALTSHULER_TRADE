using System.Collections.Generic;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBLeadsForIVR : ExternalEntityBase
    {
        private List<ESBLeadIVR> leads;
        public List<ESBLeadIVR> Leads
        {
            get => leads;
            set
            {
                this.SetProperty(value);
                leads = value;
            }
        }
    }

    public class ESBLeadIVR : ExternalEntityBase
    {
        private string leadId;
        public string LeadId
        {
            get => leadId;
            set
            {
                this.SetProperty(value);
                leadId = value;
            }
        }

        private string mobilePhone;
        public string MobilePhone
        {
            get => mobilePhone;
            set
            {
                this.SetProperty(value);
                mobilePhone = value;
            }
        }
    }
}
