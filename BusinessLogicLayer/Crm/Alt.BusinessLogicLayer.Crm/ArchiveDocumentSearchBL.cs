using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.Utils;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Alt.BusinessLogicLayer.Crm
{
    public class ArchiveDocumentSearchBL : CrmBaseBL
    {
        public ArchiveDocumentSearchBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleSearchStatus(alt_ArchiveDocumentSearch targetArchiveDocumentSearch)
        {
            this.GlobalContext.LogEntry();
            if (targetArchiveDocumentSearch.AttributeHasValue<OptionSetValue>(alt_ArchiveDocumentSearch.Fields.alt_SearchFromArchiveStatusCode) &&
                targetArchiveDocumentSearch.alt_SearchFromArchiveStatusCode.Value == (int)TransferStatusCode.Send)
            {
                targetArchiveDocumentSearch.alt_SearchFromArchiveStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
            }
        }

        public void SearchFilesFromCustomAction(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, string> data = JsonUtils.Deserialize<Dictionary<string, string>>((string)inputParameters["Data"]);
            alt_ArchiveDocumentSearch retrievedArchiveDocumentSearch = GetDocumentSearchForEntity(data);
            if (CheckIfSearchTimeThresholdPassedOrLastSearchTimeNotExist(retrievedArchiveDocumentSearch))
            {
                UpdateSearchStatusToSend(data);
            }
        }

        public void PopulateOwnerId(alt_ArchiveDocumentSearch targetArchiveDocumentSearch)
        {
            this.GlobalContext.LogEntry();

            if (targetArchiveDocumentSearch.AttributeHasValue<EntityReference>(alt_ArchiveDocumentSearch.Fields.RegardingObjectId))
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, targetArchiveDocumentSearch.RegardingObjectId.LogicalName);
                var retrievedRegardingObject = commonDAL.Get(targetArchiveDocumentSearch.RegardingObjectId.Id, new string[] { "ownerid" });
                targetArchiveDocumentSearch.OwnerId = retrievedRegardingObject["ownerid"] as EntityReference;
            }     
        }

        private alt_ArchiveDocumentSearch GetDocumentSearchForEntity(Dictionary<string, string> data)
        {
            this.GlobalContext.LogEntry();

            ArchiveDocumentSearchDAL archiveDocumentSearchDAL = new ArchiveDocumentSearchDAL(GlobalContext);
            Guid documentSearchForEntityID = new Guid(data["EntityID"]);
            string[] columns = new string[]
            {
                alt_ArchiveDocumentSearch.Fields.alt_LastSearchDate,
                alt_ArchiveDocumentSearch.Fields.alt_SearchFromArchiveStatusCode
            };

            return archiveDocumentSearchDAL.Get(documentSearchForEntityID, columns);
        }

        private bool CheckIfSearchTimeThresholdPassedOrLastSearchTimeNotExist(alt_ArchiveDocumentSearch retrievedArchiveDocumentSearch)
        {
            this.GlobalContext.LogEntry();

            string waitingtimetrheshold = this.GlobalContext.CacheManager.GetGlobalParameter<string>("DocumentSearchInArchiveWaitingThreshold");
            return !retrievedArchiveDocumentSearch.alt_LastSearchDate.HasValue ||
                DateTime.Now.Subtract(retrievedArchiveDocumentSearch.alt_LastSearchDate.Value).TotalMinutes > double.Parse(waitingtimetrheshold);
        }

        private void UpdateSearchStatusToSend(Dictionary<string, string> data)
        {
            this.GlobalContext.LogEntry();

            CommonDAL commonDAL = new CommonDAL(GlobalContext, alt_ArchiveDocumentSearch.EntityLogicalName);
            Entity documentSearchForEntityToUpdate = new Entity(alt_ArchiveDocumentSearch.EntityLogicalName);
            documentSearchForEntityToUpdate[alt_ArchiveDocumentSearch.Fields.ActivityId] = new Guid(data["EntityID"]);
            documentSearchForEntityToUpdate[alt_ArchiveDocumentSearch.Fields.alt_SearchFromArchiveStatusCode] = new OptionSetValue((int)TransferStatusCode.Send);
            commonDAL.Update(documentSearchForEntityToUpdate);
        }
    }
}
