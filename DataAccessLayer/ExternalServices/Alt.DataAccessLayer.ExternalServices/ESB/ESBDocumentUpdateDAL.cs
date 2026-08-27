using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBDocumentUpdateDAL : ExternalServicesBaseDAL<ESBDocumentUpdate, ApiDocument>
    {
        public ESBDocumentUpdateDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBDocumentUpdate MapApiEntityToTargetModel(ApiDocument apiEntity)
        {
            return new ESBDocumentUpdate
            {
                OpenTextID = apiEntity.FileArchiveIdentifier,
                CustomerID = string.Empty,
                ProductCode = string.Empty,
                ProductDesc = string.Empty,
                ProcessCode = apiEntity.ProcessCode,
                ProcessDesc = apiEntity.Regarding != null ? apiEntity.Regarding.LogicalName : string.Empty,
                DocType = string.Empty,
                Publish = apiEntity?.Publish != null ? Convert.ToInt32(apiEntity.Publish).ToString() : string.Empty,
                DocDate = string.Empty,
                AgentName = string.Empty
            };
        }
    }
}
