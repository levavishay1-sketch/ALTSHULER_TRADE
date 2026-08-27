using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class OpportunityDAL : CrmBaseDAL<Opportunity>
    {
        public OpportunityDAL(GlobalContext globalContext) : base(globalContext, Opportunity.EntityLogicalName)
        {
        }

        public LoseOpportunityResponse CloseOpportunityAsLost(Opportunity targetOpportunity)
        {
            return this.CloseOpportunityAsLost(targetOpportunity.ToEntityReference(), targetOpportunity.StatusCode);
        }

        public WinOpportunityResponse CloseOpportunityAsWon(Opportunity targetOpportunity)
        {
           return this.CloseOpportunityAsWon(targetOpportunity.ToEntityReference(), targetOpportunity.StatusCode);
        }

        public WinOpportunityResponse CloseOpportunityAsWon(EntityReference opportunityEntityReference, OptionSetValue statusCode)
        {
            this.GlobalContext.LogEntry();
            var winOppRequest = new WinOpportunityRequest
            {
                Status = statusCode,
                OpportunityClose = this.GenerateOpportunityCloseEntity(opportunityEntityReference)
            };
            WinOpportunityResponse response = (WinOpportunityResponse)this.Execute(winOppRequest);
            return response;
        }

        public LoseOpportunityResponse CloseOpportunityAsLost(EntityReference opportunityEntityReference, OptionSetValue statusCode)
        {
            this.GlobalContext.LogEntry();
            var lostOppRequest = new LoseOpportunityRequest
            {
                Status = statusCode,
                OpportunityClose = this.GenerateOpportunityCloseEntity(opportunityEntityReference)

            };
            LoseOpportunityResponse response = (LoseOpportunityResponse)this.Execute(lostOppRequest);
            return response;
        }

        public void AssignOpportunity(Guid opportunityId, Guid userId)
        {
            this.GlobalContext.LogEntry();

            var assignRequest = new AssignRequest
            {
                Assignee = new EntityReference(SystemUser.EntityLogicalName, userId),
                Target = new EntityReference(Opportunity.EntityLogicalName, opportunityId)
            };

            var assignResponse = (AssignResponse)this.Execute(assignRequest);
        }

        private Entity GenerateOpportunityCloseEntity(EntityReference opportunityEntityReference)
        {
            Entity opportunityClose = new Entity("opportunityclose");
            opportunityClose.Attributes.Add("opportunityid", opportunityEntityReference);

            return opportunityClose;
        }
    }
}
