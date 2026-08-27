using Alt.DataModel.Crm.Contracts;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm
{
    public class ActivityCommonDAL : CrmBaseDAL<Entity>
    {
        public ActivityCommonDAL(GlobalContext globalContext, string templateLogicalName) : base(globalContext, templateLogicalName)
        {

        }

        public OrganizationResponse CallTemplateGeneratorAction(TemplateGeneratorDTO templateGeneratorDTO)
        {
            this.GlobalContext.LogEntry(templateGeneratorDTO.ToString());
            OrganizationRequest request = new OrganizationRequest("alt_TemplateGenerator");
            request["TemplateId"] = templateGeneratorDTO.TempLateIdInput;
            request["TemplateType"] = templateGeneratorDTO.TemplateTypeInput;
            request["RegardingObjectId"] = templateGeneratorDTO.RegardingObjectIdInput;
            request["RegardingObjectName"] = templateGeneratorDTO.RegardingObjectEntityNameInput;

            OrganizationResponse response = this.Execute(request);
            return response;
        }

        public List<ActivityPointer> GetActivitiesByRegardingObject(Entity regardingObject, string[] activityTypeCodes)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression("activitypointer")
            {
                ColumnSet = new ColumnSet(new string[] { "activityid", "subject", "activitytypecode" })
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition("regardingobjectid", ConditionOperator.Equal, regardingObject.Id);
            filter.AddCondition("activitytypecode", ConditionOperator.In, activityTypeCodes);
            query.Criteria.AddFilter(filter);

            return this.GetMultipleWithPaging(query).Select(e => e.ToEntity<ActivityPointer>()).ToList();
        }

        public void SetActivityState(ActivityPointer activityPointer, int stateCode, int statusCode)
        {
            this.GlobalContext.LogEntry();
            var request = new SetStateRequest
            {
                EntityMoniker = new EntityReference(activityPointer.ActivityTypeCode, activityPointer.ActivityId.Value),
                State = new OptionSetValue(stateCode),
                Status = new OptionSetValue(statusCode)
            };

            OrganizationService.Execute(request);
        }
    }
}
