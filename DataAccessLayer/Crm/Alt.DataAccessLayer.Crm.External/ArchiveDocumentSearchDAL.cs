using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm.External
{
    public class ArchiveDocumentSearchDAL : CrmExternalBaseDAL<ApiArchiveDocumentSearch>
    {
        public ArchiveDocumentSearchDAL(GlobalContext globalContext) : base(globalContext, ApiArchiveDocumentSearch.EntityLogicalName) { }

        public Dictionary<Guid, ApiArchiveDocumentSearch> GetArchiveDocumentSearchesCreatedInTheLastXDaysHandler(int daysRange)
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression
            {
                EntityName = ApiArchiveDocumentSearch.EntityLogicalName,
                ColumnSet = new ColumnSet("activityid", "regardingobjectid"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = {
                        new ConditionExpression("createdon", ConditionOperator.LastXDays, daysRange)
                    }
                },
            };
            query.NoLock = true;

            var digitalFormVerificationLinkEntity = query.AddLink(ApiDigitalFormVerification.EntityLogicalName, "regardingobjectid", "alt_digitalformverificationid", JoinOperator.Inner);
            digitalFormVerificationLinkEntity.EntityAlias = ApiDigitalFormVerification.EntityLogicalName;
            digitalFormVerificationLinkEntity.Columns.AddColumns("alt_digitalformverificationid");

            var archiveDocumentSearches = this.GetMultipleAsEntity(query).Entities;

            Dictionary<Guid, ApiArchiveDocumentSearch> digitalFormVerificationWithArchiveDocumentSearch = new Dictionary<Guid, ApiArchiveDocumentSearch>();

            foreach (Entity archiveDocumentSearch in archiveDocumentSearches)
            {
                Guid regardingId = archiveDocumentSearch.GetAttributeValue<EntityReference>("regardingobjectid").Id;
                if (!digitalFormVerificationWithArchiveDocumentSearch.ContainsKey(regardingId))
                {
                    digitalFormVerificationWithArchiveDocumentSearch.Add(regardingId, base.MappCrmEntityToApiEntity(archiveDocumentSearch));
                }
            }

            return digitalFormVerificationWithArchiveDocumentSearch;
        }
    }
}
