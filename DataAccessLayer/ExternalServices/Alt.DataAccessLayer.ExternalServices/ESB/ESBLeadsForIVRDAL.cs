using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBLeadsForIVRDAL : ExternalServicesBaseDAL<ESBLeadsForIVR, ApiEntity>
    {
        List<ApiLead> leadsForIVR;

        public ESBLeadsForIVRDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration) { }

        public ActionResult SetLeads(List<ApiLead> leads)
        {
            this.GlobalContext.LogEntry();

            this.leadsForIVR = leads;
            return base.Post(new ApiEntity());
        }

        protected override ESBLeadsForIVR MapApiEntityToTargetModel(ApiEntity apiEntity)
        {
            this.GlobalContext.LogEntry();

            List<ESBLeadIVR> leadsIVR = new List<ESBLeadIVR>();
            foreach (ApiLead lead in this.leadsForIVR)
            {
                leadsIVR.Add(new ESBLeadIVR
                {
                    LeadId = lead.Id.ToString(),
                    MobilePhone = lead.MobilePhone
                });
            }

            ESBLeadsForIVR eSBLeadsForIVR = new ESBLeadsForIVR
            {
                Leads = leadsIVR
            };

            return eSBLeadsForIVR;
        }
    }
}
