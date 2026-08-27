using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class DepositDAL : CrmExternalBaseDAL<ApiDeposit>
    {
        private string zipFileNameFieldName = "alt_zipfiledate";

        public DepositDAL(GlobalContext globalContext) : base(globalContext, ApiDeposit.EntityLogicalName) { }

        public List<ApiDeposit> GetDepositsByXMLDateRange(DateTime startDate, DateTime endDate)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiDeposit.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression(
                            zipFileNameFieldName,
                            ConditionOperator.Between,
                                new object[] { startDate, endDate }
                            )
                    }
                },
                NoLock = true
            };
            return this.GetMultipleWithPaging(query);
        }

        public Dictionary<string, AttributeMetadata> GetDepositMetaData()
        {
            this.GlobalContext.LogEntry();
            var request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = ApiDeposit.EntityLogicalName
            };

            var response = (RetrieveEntityResponse)OrganizationService.Execute(request);
            return response.EntityMetadata.Attributes
                .ToDictionary(attr => attr.LogicalName, attr => attr);
        }
    }
}
