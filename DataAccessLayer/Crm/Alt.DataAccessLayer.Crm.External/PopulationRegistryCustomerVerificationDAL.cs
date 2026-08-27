using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Alt.DataAccessLayer.Crm.External
{
    public class PopulationRegistryCustomerVerificationDAL : CrmExternalBaseDAL<ApiPopulationRegistryCustomerVerification>
    {
        public PopulationRegistryCustomerVerificationDAL(GlobalContext globalContext) 
            : base(globalContext, ApiPopulationRegistryCustomerVerification.EntityLogicalName) { }

        public int GetErrorMessageDetailsMaxLength()
        {
            this.GlobalContext.LogEntry();

            string cacheName = "PopulationRegistryCustomerVerification_ErrorMessageDetails_MaxLength";
            var response = GlobalContext.CacheManager.GetCachedItem<RetrieveAttributeResponse>(cacheName
                , () => { return base.GetAttributeMetadata("alt_errormessagedetails"); }, 160);

            StringAttributeMetadata stringAttributeMetadata = (StringAttributeMetadata)response.AttributeMetadata;

            return stringAttributeMetadata.MaxLength.Value;
        }
    }
}
