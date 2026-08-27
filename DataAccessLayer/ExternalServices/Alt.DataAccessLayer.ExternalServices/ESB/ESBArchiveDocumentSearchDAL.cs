using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBArchiveDocumentSearchDAL : ExternalServicesBaseDAL<ESBArchiveDocumentSearch, ApiArchiveDocumentSearch>
    {
        public ESBArchiveDocumentSearchDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBArchiveDocumentSearch MapApiEntityToTargetModel(ApiArchiveDocumentSearch apiDocumentSearchForEntity)
        {
            return new ESBArchiveDocumentSearch
            {
                CustomerID = apiDocumentSearchForEntity.Customer.CustomerIdentity,
                ProductCode = string.Empty,
                ProductDesc = string.Empty,
                ProcessCode = apiDocumentSearchForEntity.ProcessCode,
                ProcessDesc = string.Empty,
                SystemCode = string.Empty,
                DocType = string.Empty,
                Publish = string.Empty,
                DocDate = string.Empty,
                ExternalID = string.Empty,
                Creator = string.Empty,
                AgentName = string.Empty
            };
        }
    }
}
