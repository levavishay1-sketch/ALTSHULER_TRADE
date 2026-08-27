using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class DocumentBL : ExternalBLBase, ICrmOutgoing<ApiDocument>
    {
        private string ERROR_FILE_EXISTS_MESSSAGE = "ExternalID already Exists";

        public DocumentBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiDocument> apiContext)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            if (this.ApiConfiguration != null)
            {
                ApiConfigurationCode apiConfigurationCode = (ApiConfigurationCode)this.ApiConfiguration.Code.Value;
                switch (apiConfigurationCode)
                {
                    case ApiConfigurationCode.DocumentUpload:
                        {
                            actionResult = this.UploadFileToArchiveHandler(apiContext.Target);
                            break;
                        }
                    case ApiConfigurationCode.DocumentDownload:
                        {
                            actionResult = this.DownloadFileFromArchiveHandler(apiContext.Target);
                            break;
                        }
                    case ApiConfigurationCode.DocumentFilingUpdate:
                        {
                            actionResult = this.UpdateFileAtArchiveHandler(apiContext.Target);
                            break;
                        }
                    default:
                        {
                            actionResult.SetToFailedActionResult(CustomErrorCodes.UnrecognizedApiCodeForDocument);
                            break;
                        }
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }

            return actionResult;
        }

        private ActionResult UploadFileToArchiveHandler(ApiDocument apiDocument)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            ApiDocument retrievedDocument = documentDAL.GetDocumentDetails(apiDocument.Id.Value);
            retrievedDocument.BodyBase64 = this.GetFileBodyBase64String(apiDocument.Id.Value, apiDocument.LogicalName, documentDAL);
            retrievedDocument.CustomerIdentityNumber = this.HandleGetArchieveCustomerId(retrievedDocument);
            retrievedDocument.ProcessCode = this.GetProcessCodeFromRegardingObject(retrievedDocument);

            ESBDocumentUploadDAL eSBDocumentUploadDAL = new ESBDocumentUploadDAL(this.GlobalContext, this.ApiConfiguration);
            actionResult = eSBDocumentUploadDAL.ExecuteRequest(retrievedDocument);

            this.HandleUploadResult(apiDocument, actionResult);

            return actionResult;
        }

        private ActionResult DownloadFileFromArchiveHandler(ApiDocument apiDocument)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            ApiDocument documentTodownload = documentDAL.Get(
                apiDocument.Id.Value,
                new string[]
                {
                    "alt_filearchiveidentifier",
                    "alt_name",
                    "alt_mimetype"
                }
            );

            ESBDocumentDownloadDAL eSBDocumentDownloadDAL = new ESBDocumentDownloadDAL(this.GlobalContext, this.ApiConfiguration);
            actionResult = eSBDocumentDownloadDAL.ExecuteRequest(documentTodownload);

            this.HandleDownloadResult(documentTodownload, actionResult);

            return actionResult;
        }

        private ActionResult UpdateFileAtArchiveHandler(ApiDocument apiDocument)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            ApiDocument documentToUpdate = documentDAL.Get(
                apiDocument.Id.Value,
                new string[]
                {
                    "alt_publish",
                    "alt_regardingid",
                    "alt_filearchiveidentifier"
                }
            );
            documentToUpdate.ProcessCode = this.GetProcessCodeFromRegardingObject(documentToUpdate);

            ESBDocumentUpdateDAL eSBDocumentUpdateDAL = new ESBDocumentUpdateDAL(GlobalContext, this.ApiConfiguration);
            actionResult = eSBDocumentUpdateDAL.ExecuteRequest(documentToUpdate);

            this.HandleUpdateResult(apiDocument, actionResult);

            return actionResult;
        }

        private void HandleUploadResult(ApiDocument apiDocument, ActionResult actionResult)
        {
            this.GlobalContext.LogEntry();

            ApiDocument apiDocumentToUpdate = new ApiDocument { Id = apiDocument.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;
            string errorMessage;
            string openTextIdFromArchive = null;

            if (actionResult.IsSuccess)
            {
                ESBResponse<ESBDocumentResponse> uploadResponse = JsonUtils.Deserialize<ESBResponse<ESBDocumentResponse>>(actionResult.ReturnObject.ToString());
                resultStatus = uploadResponse.ResultStatusCode;
                errorMessage = uploadResponse.ErrorMessage;
                openTextIdFromArchive = uploadResponse.ResponseData.OpenTextID;

                if (resultStatus == ESBResultStatusCode.Error && errorMessage.Contains(ERROR_FILE_EXISTS_MESSSAGE))
                {
                    resultStatus = ESBResultStatusCode.Success;
                    openTextIdFromArchive = apiDocument.Id.ToString().Replace("-", "");
                }
            }

            apiDocumentToUpdate.ArchiveTransferStatusCode = resultStatus != null && resultStatus == ESBResultStatusCode.Success
                ? (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;
            apiDocumentToUpdate.FileArchiveIdentifier = openTextIdFromArchive;

            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            documentDAL.Update(apiDocumentToUpdate);
        }

        private void HandleDownloadResult(ApiDocument apiDocument, ActionResult actionResult)
        {
            this.GlobalContext.LogEntry();

            ApiDocument apiDocumentToUpdate = new ApiDocument { Id = apiDocument.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;

            if (actionResult.IsSuccess)
            {
                ESBResponse<ESBDocumentResponse> downloadResponse = JsonUtils.Deserialize<ESBResponse<ESBDocumentResponse>>(actionResult.ReturnObject.ToString());
                resultStatus = downloadResponse.ResultStatusCode;

                if (resultStatus != null && resultStatus == ESBResultStatusCode.Success)
                {
                    if (this.ApiConfiguration.DebugMode.Value)
                    {
                        this.PopulateFileFieldInDocument(apiDocument.Id.Value, downloadResponse.ResponseData.DocBase64, "dummy.txt", "text/plain");
                    }
                    else
                    {
                        this.PopulateFileFieldInDocument(apiDocument.Id.Value, downloadResponse.ResponseData.DocBase64, apiDocument.Name, apiDocument.MimeType);
                    }
                }
            }

            apiDocumentToUpdate.ArchiveDownloadStatusCode = resultStatus != null && resultStatus == ESBResultStatusCode.Success
                ? (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;

            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            documentDAL.Update(apiDocumentToUpdate);
        }

        private void HandleUpdateResult(ApiDocument apiDocument, ActionResult actionResult)
        {
            this.GlobalContext.LogEntry();

            ApiDocument apiDocumentToUpdate = new ApiDocument { Id = apiDocument.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;

            if (actionResult.IsSuccess)
            {
                ESBResponse<ESBDocumentResponse> updateResponse = JsonUtils.Deserialize<ESBResponse<ESBDocumentResponse>>(actionResult.ReturnObject.ToString());
                resultStatus = updateResponse.ResultStatusCode;
            }

            apiDocumentToUpdate.ArchiveUpdateStatusCode = resultStatus != null && resultStatus == ESBResultStatusCode.Success
                ? (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;

            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);
            documentDAL.Update(apiDocumentToUpdate);
        }

        private string GetFileBodyBase64String(Guid documentId, string entityLogicalName, DocumentDAL documentDAL)
        {
            var initializeFileBlocksDownloadResponse = documentDAL.ExecuteInitializeFileBlocksDownloadRequest(
                documentId,
                entityLogicalName,
                "alt_file"
            );

            string fileContinuationToken = initializeFileBlocksDownloadResponse.FileContinuationToken;
            long fileSizeInBytes = initializeFileBlocksDownloadResponse.FileSizeInBytes;
            List<byte> fileBytes = new List<byte>((int)fileSizeInBytes);
            long offset = 0;
            long blockSizeDownload = !initializeFileBlocksDownloadResponse.IsChunkingSupported ? fileSizeInBytes : 4 * 1024 * 1024;

            if (fileSizeInBytes < blockSizeDownload)
            {
                blockSizeDownload = fileSizeInBytes;
            }

            while (fileSizeInBytes > 0)
            {
                var downloadBlockResponse = documentDAL.ExecuteDownloadBlockRequest(blockSizeDownload, fileContinuationToken, offset);
                fileBytes.AddRange(downloadBlockResponse.Data);
                fileSizeInBytes -= (int)blockSizeDownload;
                offset += blockSizeDownload;
            }

            string bodyBase64 = Convert.ToBase64String(fileBytes.ToArray());
            return bodyBase64;
        }

        internal void PopulateFileFieldInDocument(Guid documentGuid, string base64, string fileName, string mimeType)
        {
            DocumentDAL documentDAL = new DocumentDAL(this.GlobalContext);

            byte[] fileContentAsBytes = Convert.FromBase64String(base64);
            var initializeFileBlocksUploadResponse = documentDAL.ExecuteFileBlockUploadRequest(documentGuid, fileName);
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
                fileName,
                mimeType,
                listBlock.ToArray()
            );
        }

        private string HandleGetArchieveCustomerId(ApiDocument apiDocument)
        {
            CustomerDAL customerDAL = new CustomerDAL(this.GlobalContext, apiDocument.CustomerID.LogicalName);
            string customerIdentity = customerDAL.GetCustomerArchiveIdentifier(apiDocument.CustomerID);
            return customerIdentity;
        }

        private string GetProcessCodeFromRegardingObject(ApiDocument apiDocument)
        {
            string regardingLogicalName = apiDocument.Regarding.LogicalName;
            Guid regardingID = apiDocument.Regarding.Id.Value;
            string processCode = string.Empty;

            switch (regardingLogicalName)
            {
                case ApiOpportunity.EntityLogicalName:
                    {
                        OpportunityDAL opportunityDAL = new OpportunityDAL(this.GlobalContext);
                        ApiOpportunity apiOpportunity = opportunityDAL.Get(regardingID, new string[] { "alt_opportunityidentitynumber" });
                        processCode = apiOpportunity.OpportunityIdentityNumber;
                        break;
                    }
                case ApiDigitalFormVerification.EntityLogicalName:
                    {
                        DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                        ApiDigitalFormVerification apiDigitalFormVerification = digitalFormVerificationDAL.Get(regardingID, new string[] { "alt_digitalformnumber" });
                        processCode = apiDigitalFormVerification.DigitalFormNumber;
                        break;
                    }
                case ApiPortfolio.EntityLogicalName:
                    {
                        PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
                        ApiPortfolio apiPortfolio = portfolioDAL.Get(regardingID, new string[] { "alt_shenhavaccountnumber" });
                        processCode = apiPortfolio.ShenhavAccountNumber;
                        break;
                    }
                default:
                    break;
            }

            return processCode;
        }
    }
}
