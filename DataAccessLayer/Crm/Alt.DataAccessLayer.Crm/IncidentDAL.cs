using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.DataAccessLayer.Crm
{
    public class IncidentDAL : CrmBaseDAL<Incident>
    {
        public IncidentDAL(GlobalContext globalContext) : base(globalContext, Incident.EntityLogicalName)
        {
        }
    
        public void ResolveIncident(Incident targetIncident, EntityReference owner)
        {
            this.GlobalContext.LogEntry();

            Entity incidentResolution = new Entity("incidentresolution");
            incidentResolution.Attributes["subject"] = "Incident Closed";
            incidentResolution.Attributes["incidentid"] = targetIncident.ToEntityReference();
            incidentResolution.Attributes["ownerid"] = owner;

            CloseIncidentRequest closeIncidentRequest = new CloseIncidentRequest()
            {
                Status = targetIncident.alt_StatusToChangeCode,
                IncidentResolution = incidentResolution
            };

            base.Execute(closeIncidentRequest);
        }

        public SetStateResponse SetStateRequest(Entity entity, OptionSetValue statusCode, OptionSetValue stateCode)
        {
            SetStateRequest request = new SetStateRequest()
            {
                EntityMoniker = entity.ToEntityReference(),
                State = stateCode,
                Status = statusCode
            };
            return (SetStateResponse)base.Execute(request);
        }
    }
}
