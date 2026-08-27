using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBDocumentUploadDAL : ExternalServicesBaseDAL<ESBDocumentUpload, ApiDocument>
    {
        public ESBDocumentUploadDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBDocumentUpload MapApiEntityToTargetModel(ApiDocument apiEntity)
        {
            return new ESBDocumentUpload
            {
                CustomerID = apiEntity.CustomerIdentityNumber,
                ProductCode = "5",
                ProductDesc = "טרייד",
                ProcessCode = apiEntity.ProcessCode,
                ProcessDesc = apiEntity.Regarding.LogicalName,
                SystemCode = "CRM365",
                DocType = apiEntity.MimeType,
                Publish = "0",
                DocDate = apiEntity.CreatedOn.GetValueOrDefault().ToString(),
                ExternalID = apiEntity.Id.ToString().Replace("-", ""),
                StrFilePath = string.Empty,
                DocBase64 = apiEntity.BodyBase64,
                AgentName = apiEntity.CreatedBy.FullName,
                FileName = apiEntity.Name
            };
        }
    }
}
