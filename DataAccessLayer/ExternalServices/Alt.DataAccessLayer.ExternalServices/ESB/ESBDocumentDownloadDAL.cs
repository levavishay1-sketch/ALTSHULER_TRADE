using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBDocumentDownloadDAL : ExternalServicesBaseDAL<ESBDocumentDownload, ApiDocument>
    {
        public ESBDocumentDownloadDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBDocumentDownload MapApiEntityToTargetModel(ApiDocument apiEntity)
        {
            return new ESBDocumentDownload
            {
                OpenTextID = apiEntity.FileArchiveIdentifier 
            };
        }
    }
}
