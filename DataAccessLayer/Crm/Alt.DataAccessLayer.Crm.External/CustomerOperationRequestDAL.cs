using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class CustomerOperationRequestDAL : CrmExternalBaseDAL<ApiCustomerOperationRequest>
    {
        public CustomerOperationRequestDAL(GlobalContext globalContext) 
            : base(globalContext, ApiCustomerOperationRequest.EntityLogicalName) { }

        public void Delete(Guid id)
        {
            this.Delete(id);
        }

        public int GetSendResultAttributeMaxLength()
        {
            this.GlobalContext.LogEntry();

            string cacheName = "CustomerOperationRequest_SendResult_MaxLength";
            var response = GlobalContext.CacheManager.GetCachedItem<RetrieveAttributeResponse>(cacheName
                , () => { return base.GetAttributeMetadata("alt_sendresult"); }, 60);
     
            MemoAttributeMetadata stringAttributeMetadata = (MemoAttributeMetadata)response.AttributeMetadata;

            return stringAttributeMetadata.MaxLength.Value;
        }

        public ApiCustomerOperationRequest GetCustomerOperationRequestByTemplateCodeAndRelated(int code, Guid relatedId)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = ApiCustomerOperationRequest.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                NoLock = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("alt_relatedrecordid", ConditionOperator.Equal, relatedId),
                        new ConditionExpression("alt_customeroperationtemplatecodeint", ConditionOperator.Equal, code),
                        new ConditionExpression("statuscode", ConditionOperator.Equal, (int)CustomerOperationRequestStatusCode.Fail)
                    }
                }
            };

            return this.GetMultiple(query).FirstOrDefault();
        }
    }
}
