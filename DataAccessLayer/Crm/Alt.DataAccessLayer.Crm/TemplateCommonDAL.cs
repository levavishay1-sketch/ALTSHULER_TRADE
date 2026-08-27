using Alt.DataModel.Crm.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk;


namespace Alt.DataAccessLayer.Crm
{
    public class TemplateCommonDAL : CrmBaseDAL<Entity>
    {
        public TemplateCommonDAL(GlobalContext globalContext, string templateLogicalName) : base(globalContext, templateLogicalName) { }

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
    }
}
