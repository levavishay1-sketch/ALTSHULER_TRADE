using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.DataAccessLayer.Crm
{
    public class DocumentDAL : CrmBaseDAL<alt_Document>
    {
        public DocumentDAL(GlobalContext globalContext) : base(globalContext, alt_Document.EntityLogicalName) { }

        public InitializeFileBlocksUploadResponse ExecuteFileBlockUploadRequest(Guid documentId, string fileName)
        {
            InitializeFileBlocksUploadRequest initializeFileBlocksUploadRequest = new InitializeFileBlocksUploadRequest()
            {
                Target = new EntityReference(alt_Document.EntityLogicalName, documentId),
                FileAttributeName = alt_Document.Fields.alt_File,
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
