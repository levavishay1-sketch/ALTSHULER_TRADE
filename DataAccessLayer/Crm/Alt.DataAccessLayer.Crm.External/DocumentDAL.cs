using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Alt.DataAccessLayer.Crm.External
{
    public class DocumentDAL : CrmExternalBaseDAL<ApiDocument>
    {
        private string[] documentEntityAttributes =
        {
            "alt_documentid",
            "alt_archivetransferstatuscode",
            "createdby",
            "createdon",
            "alt_regardingid",
            "alt_customerid"
        };

        private string[] fileattachmentAttributes =
        {
            "mimetype",
            "filename"
        };

        public DocumentDAL(GlobalContext globalContext) : base(globalContext, ApiDocument.EntityLogicalName) { }

        public ApiDocument GetDocumentDetails(Guid documentId)
        {
            this.GlobalContext.LogEntry();

            RelationshipQueryCollection documentRelatedFileQueryCollection = new RelationshipQueryCollection
            {
                {
                    new Relationship(ApiDocument.EntityLogicalName + "_FileAttachments"),
                    new QueryExpression("fileattachment")
                    {
                        ColumnSet = new ColumnSet(fileattachmentAttributes)
                    }
                }
            };

            RetrieveRequest retrieveRequest = new RetrieveRequest
            {
                ColumnSet = new ColumnSet(documentEntityAttributes),
                RelatedEntitiesQuery = documentRelatedFileQueryCollection,
                Target = new EntityReference(ApiDocument.EntityLogicalName, documentId)
            };

            RetrieveResponse retrieveResponse = (RetrieveResponse)this.Execute(retrieveRequest);
            var document = retrieveResponse.Entity;
            var fileMetadata = document.RelatedEntities[new Relationship("alt_document_FileAttachments")][0];

            return document != null ? this.MapToApiDocument(document, fileMetadata) : null;
        } 

        public InitializeFileBlocksDownloadResponse ExecuteInitializeFileBlocksDownloadRequest(Guid documentId, string entityLogicalName, string fileAttributeName)
        {
            InitializeFileBlocksDownloadRequest initializeFileBlocksDownloadRequest = new InitializeFileBlocksDownloadRequest
            {
                Target = new EntityReference(entityLogicalName, documentId),
                FileAttributeName = fileAttributeName
            };

            var initializeFileBlocksDownloadResponse = (InitializeFileBlocksDownloadResponse)this.Execute(initializeFileBlocksDownloadRequest);
            return initializeFileBlocksDownloadResponse;
        }

        public DownloadBlockResponse ExecuteDownloadBlockRequest(long blockSizeDownload, string fileContinuationToken, long offset)
        {
            DownloadBlockRequest downLoadBlockRequest = new DownloadBlockRequest
            {
                BlockLength = blockSizeDownload,
                FileContinuationToken = fileContinuationToken,
                Offset = offset
            };

            var downloadBlockResponse = (DownloadBlockResponse)this.Execute(downLoadBlockRequest);
            return downloadBlockResponse;
        }

        private ApiDocument MapToApiDocument(Entity document, Entity fileMetadata)
        {
            ApiDocument apiDocument = base.MappCrmEntityToApiEntity(document);
            this.MapManualAttributes(apiDocument, document, fileMetadata);
            return apiDocument;
        }

        private void MapManualAttributes(ApiDocument apiDocument, Entity document, Entity fileMetadata)
        {
            apiDocument.MimeType = fileMetadata["mimetype"].ToString();
            apiDocument.Name = fileMetadata["filename"].ToString();
            apiDocument.CreatedBy.FullName = ((EntityReference)document["createdby"]).Name;
        }
        
        public InitializeFileBlocksUploadResponse ExecuteFileBlockUploadRequest(Guid documentId, string fileName)
        {
            InitializeFileBlocksUploadRequest initializeFileBlocksUploadRequest = new InitializeFileBlocksUploadRequest()
            {
                Target = new EntityReference(ApiDocument.EntityLogicalName, documentId),
                FileAttributeName = "alt_file",
                FileName = fileName
            };

            var initializeFileBlocksUploadResponse = (InitializeFileBlocksUploadResponse)base.Execute(initializeFileBlocksUploadRequest);
            return initializeFileBlocksUploadResponse;
        }

        public UploadBlockResponse ExecuteUploadBlockRequest(string blockId, byte[] blockData, string fileContinuationToken)
        {
            UploadBlockRequest uploadBlockRequest = new UploadBlockRequest
            {
                BlockId = blockId,
                BlockData = blockData,
                FileContinuationToken = fileContinuationToken
            };

            var uploadBlockResponse = (UploadBlockResponse)base.Execute(uploadBlockRequest);
            return uploadBlockResponse;
        }

        public CommitFileBlocksUploadResponse ExecuteCommitFileBlocksUploadRequest(string fileContinuationToken, string fileName, string mimeType, string[] blockList)
        {
            CommitFileBlocksUploadRequest commitFileBlocksUploadRequest = new CommitFileBlocksUploadRequest
            {
                FileContinuationToken = fileContinuationToken,
                FileName = fileName,
                MimeType = mimeType,
                BlockList = blockList
            };

            var commitFileBlocksUploadResponse = (CommitFileBlocksUploadResponse)base.Execute(commitFileBlocksUploadRequest);
            return commitFileBlocksUploadResponse;
        }
    }
}
