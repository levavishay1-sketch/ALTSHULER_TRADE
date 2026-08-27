using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.Utils;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class DocumentBL : CrmBaseBL
    {
        public DocumentBL(GlobalContext globalContext) : base(globalContext) { }

        public void UploadFileFromCustomAction(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();

            FileUploadObject fileUploadObject = JsonUtils.Deserialize<FileUploadObject>((string)inputParameters["Data"]);
            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            Guid documentId = new Guid(fileUploadObject.DocumentId);

            byte[] fileContentAsBytes = Convert.FromBase64String(fileUploadObject.Content);
            var initializeFileBlocksUploadResponse = documentDAL.ExecuteFileBlockUploadRequest(documentId, fileUploadObject.FileName);
            var listBlock = new List<string>();
            int chunkSize = 1024 * 1024 * 4;

            for (int i = 0; i < (double)fileContentAsBytes.Length / chunkSize; i++)
            {
                var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                listBlock.Add(blockId);

                var uploadBlockResponse = documentDAL.ExecuteUploadBlockRequest(
                    blockId,
                    fileContentAsBytes.Skip(i * chunkSize).Take(chunkSize).ToArray(),
                    initializeFileBlocksUploadResponse.FileContinuationToken
                );
            }

            var commitFileBlocksUploadResponse = documentDAL.ExecuteCommitFileBlocksUploadRequest(
                initializeFileBlocksUploadResponse.FileContinuationToken,
                fileUploadObject.FileName,
                fileUploadObject.MimeType,
                listBlock.ToArray()
            );

            this.HandlePostUploadFile(documentId, fileUploadObject.FileName, fileUploadObject.MimeType);
        }

        public void DownloadFileFromCustomAction(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, string> data = JsonUtils.Deserialize<Dictionary<string, string>>((string)inputParameters["Data"]);
            this.HandlePostDownloadFile(new Guid(data["DocumentId"]));
        }

        public void HandleFileUploadStatus(alt_Document targetDocument)
        {
            this.GlobalContext.LogEntry();
            if (targetDocument.AttributeHasValue<OptionSetValue>(alt_Document.Fields.alt_ArchiveTransferStatusCode) &&
                targetDocument.alt_ArchiveTransferStatusCode.Value == (int)TransferStatusCode.Send)
            {
                targetDocument.alt_ArchiveTransferStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
            }
        }

        public void HandleFileDownloadStatus(alt_Document targetDocument)
        {
            this.GlobalContext.LogEntry();
            if (targetDocument.AttributeHasValue<OptionSetValue>(alt_Document.Fields.alt_ArchiveDownloadStatusCode) &&
                targetDocument.alt_ArchiveDownloadStatusCode.Value == (int)TransferStatusCode.Send)
            {
                targetDocument.alt_ArchiveDownloadStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
            }
        }

        public void HandleFileUpdateStatus(alt_Document targetDocument)
        {
            this.GlobalContext.LogEntry();
            if (targetDocument.AttributeHasValue<EntityReference>(alt_Document.Fields.alt_RegardingId) ||
                targetDocument.AttributeHasValue<bool?>(alt_Document.Fields.alt_Publish))
            {
                targetDocument.alt_ArchiveUpdateStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
            }
        }

        public void HandlePostUploadFile(Guid documentId, string fileName, string mimeType)
        {
            this.GlobalContext.LogEntry();

            CommonDAL documentDAL = new CommonDAL(this.GlobalContext, alt_Document.EntityLogicalName);
            Entity documentToUpdate = new Entity(alt_Document.EntityLogicalName);
            documentToUpdate["alt_documentid"] = documentId;
            documentToUpdate["alt_archivetransferstatuscode"] = new OptionSetValue((int)TransferStatusCode.Send);
            documentToUpdate["alt_name"] = fileName;
            documentToUpdate["alt_mimetype"] = mimeType;
            documentDAL.Update(documentToUpdate);
        }

        public void HandlePostDownloadFile(Guid documentId)
        {
            CommonDAL documentDAL = new CommonDAL(this.GlobalContext, alt_Document.EntityLogicalName);
            Entity documentToUpdate = new Entity(alt_Document.EntityLogicalName);
            documentToUpdate["alt_documentid"] = documentId;
            documentToUpdate["alt_archivedownloadstatuscode"] = new OptionSetValue((int)TransferStatusCode.Send);
            documentDAL.Update(documentToUpdate);
        }

        public void ReplaceDocumentsRegardingId(EntityReference fromRegardingId, EntityReference toRegardingId)
        {
            this.GlobalContext.LogEntry();

            DocumentDAL documentDal = new DocumentDAL(this.GlobalContext);
            var documents = documentDal.GetByAttribute(alt_Document.Fields.alt_RegardingId, fromRegardingId?.Id, new string[] { alt_Document.Fields.Id });
            if (documents != null && documents.Count > 0)
            {
                foreach (var document in documents)
                {
                    documentDal.Update(new alt_Document
                    {
                        Id = document.Id,
                        alt_RegardingId = toRegardingId
                    });
                }
            }
        }

        public void PopulateOwnerId(alt_Document targetDocument)
        {
            this.GlobalContext.LogEntry();

            var globalParameterValue = this.GlobalContext.CacheManager.GetGlobalParameter<string>("ArchiveDocumentProductToOwnerIdMapping");
            var mapper = JsonSerializer.Deserialize<Dictionary<string, string>>(globalParameterValue);

            if (targetDocument.AttributeHasValue<OptionSetValue>(alt_Document.Fields.alt_ProductTypeCode) &&
                mapper.TryGetValue(targetDocument.alt_ProductTypeCode.Value.ToString(), out string teamCode))
            {
                RetrieveRequest retrieveRequest = new RetrieveRequest
                {
                    ColumnSet = new ColumnSet(new string[] { Team.Fields.Id }),
                    Target = new EntityReference(Team.EntityLogicalName, "alt_teamcodeint", int.Parse(teamCode))
                };
                RetrieveResponse retrieveResponse = (RetrieveResponse)this.GlobalContext.OrganizationService.Execute(retrieveRequest);
                if (retrieveResponse.Entity != null)
                {
                    targetDocument.OwnerId = new EntityReference(Team.EntityLogicalName, retrieveResponse.Entity.Id);
                }
            }
        }
    }
}