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
    public class DocumentSearchForEntityBL : CrmBaseBL
    {
        public DocumentSearchForEntityBL(GlobalContext globalContext) : base(globalContext) { }

        //public void SearchFilesFromCustomAction(ParameterCollection inputParameters)
        //{
        //    Dictionary<string, string> data = JsonUtils.Deserialize<Dictionary<string, string>>((string)inputParameters["Data"]);
        //    alt_DocumentSearchForEntity retrievedDocumentSearchForEntity = GetDocumentSearchForEntity(data);
        //    if (CheckIfSearchTimeThresholdPassedOrLastSearchTimeNotExist(retrievedDocumentSearchForEntity))
        //    {
        //        UpdateSearchStatusToSend(data);
        //    }
        //}

        //public void HandleSearchStatus(alt_DocumentSearchForEntity targetDocumentSearchForEntity)
        //{
        //    this.GlobalContext.LogEntry();
        //    if (targetDocumentSearchForEntity.AttributeHasValue<OptionSetValue>(alt_DocumentSearchForEntity.Fields.alt_SearchFromArchiveStatus) &&
        //        targetDocumentSearchForEntity.alt_SearchFromArchiveStatus.Value == (int)TransferStatusCode.Send)
        //    {
        //        targetDocumentSearchForEntity.alt_SearchFromArchiveStatus = new OptionSetValue((int)TransferStatusCode.Sending);
        //    }
        //}

        //private alt_DocumentSearchForEntity GetDocumentSearchForEntity(Dictionary<string, string> data)
        //{
        //    alt_DocumentSearchForEntity documentSearchForEntityDAL = new alt_DocumentSearchForEntity(GlobalContext);
        //    Guid documentSearchForEntityID = new Guid(data["EntityID"]);
        //    string[] columns = new string[]
        //    {
        //        alt_DocumentSearchForEntity.Fields.alt_LastSearchDate,
        //        alt_DocumentSearchForEntity.Fields.alt_SearchFromArchiveStatus
        //    };

        //    return documentSearchForEntityDAL.Get(documentSearchForEntityID, columns);
        //}

        //private bool CheckIfSearchTimeThresholdPassedOrLastSearchTimeNotExist(alt_DocumentSearchForEntity retrievedDocumentSearchForEntity)
        //{
        //    string waitingtimetrheshold = this.GlobalContext.CacheManager.GetGlobalParameter<string>("DocumentSearchInArchiveWaitingThreshold");
        //    return !retrievedDocumentSearchForEntity.alt_LastSearchDate.HasValue ||
        //        DateTime.Now.Subtract(retrievedDocumentSearchForEntity.alt_LastSearchDate.Value).TotalMinutes > double.Parse(waitingtimetrheshold);
        //}

        //private void UpdateSearchStatusToSend(Dictionary<string, string> data)
        //{
        //    CommonDAL commonDAL = new CommonDAL(GlobalContext, alt_DocumentSearchForEntity.EntityLogicalName);
        //    Entity documentSearchForEntityToUpdate = new Entity(alt_DocumentSearchForEntity.EntityLogicalName);
        //    documentSearchForEntityToUpdate[alt_DocumentSearchForEntity.Fields.ActivityId] = new Guid(data["EntityID"]);
        //    documentSearchForEntityToUpdate[alt_DocumentSearchForEntity.Fields.alt_SearchFromArchiveStatus] = new OptionSetValue((int)TransferStatusCode.Send);
        //    commonDAL.Update(documentSearchForEntityToUpdate);
        //}
    }
}
