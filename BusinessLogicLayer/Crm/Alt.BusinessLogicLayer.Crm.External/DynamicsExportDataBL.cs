using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using Alt.Framework.Azure.Storage;
using Alt.Framework.External.Utils;
using Alt.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class DynamicsExportDataBL : ExternalBLBase
    {
        public DynamicsExportDataBL(GlobalContext globalContext)
           : base(globalContext) { }

        public ActionResult HandleDynamicsDataExport()
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();

            string tablesToRetrieve = this.GlobalContext.CacheManager.GetGlobalParameter<string>("TablesToRetrieve");
            string blobContainerClientURI = this.GlobalContext.CacheManager.GetGlobalParameter<string>("BlobContainerClientURI");
            string constantColumnsToAdd = this.GlobalContext.CacheManager.GetGlobalParameter<string>("FieldsToAddTableExportCSV");

            this.GlobalContext.Log.Info($"{Environment.NewLine}Tables to retrieve: {tablesToRetrieve}{Environment.NewLine}Blob container client URI: {blobContainerClientURI}");

            List<string> tableNamesValue = tablesToRetrieve.Split(',').Select(name => name.Trim()).ToList();
            CSVExportExtraColumns extraColumns = JsonUtils.Deserialize<CSVExportExtraColumns>(constantColumnsToAdd);

            foreach (var tableName in tableNamesValue)
            {
                try
                {
                    this.ExportTable(tableName, blobContainerClientURI, extraColumns);
                }
                catch (Exception ex)
                {
                    string errorMassage = $"Failed to upload {tableName} table to {blobContainerClientURI}.{Environment.NewLine}{ex}";
                    this.GlobalContext.Log.Warning(errorMassage);
                    if (actionResult.IsSuccess)
                    {
                        actionResult.SetToFailedActionResult($"One or more errors occured in export data.");
                    }
                }
            }

            return actionResult;
        }

        private void ExportTable(string tableName, string blobContainerClientURI, CSVExportExtraColumns extraColumns)
        {
            this.GlobalContext.LogEntry($"Table Name: {tableName}");

            DynamicsExportDataDAL dynamicsExportDataDAL = new DynamicsExportDataDAL(this.GlobalContext);
            (List<Dictionary<string, object>> tableData, string[] headers) = dynamicsExportDataDAL.GetExportData(tableName, extraColumns);

            UploadFileToBlobStorage(blobContainerClientURI, tableName, tableData, headers);
        }

        private void UploadFileToBlobStorage(string blobContainerClientURI, string tableName, List<Dictionary<string, object>> tableData, string[] headers)
        {
            this.GlobalContext.LogEntry($"Table Name: {tableName}");

            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string folderPath = $"{currentDate}";
            this.GlobalContext.Log.Info($"Folder path: {folderPath}");
            string csvContent = CsvUtils.GenerateCsvContentFromTableData(tableData, headers);

            AzureStorageUtils azureStorageUtils = new AzureStorageUtils(blobContainerClientURI);
            azureStorageUtils.UploadContentToBlob(folderPath, $"{tableName}.csv", csvContent, "text/csv");
        }
    }
}
