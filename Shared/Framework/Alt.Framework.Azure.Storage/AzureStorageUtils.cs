using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Alt.Framework.Azure.Storage
{
    public class AzureStorageUtils
    {
        private BlobContainerClient containerClient;

        public AzureStorageUtils(string blobContainerUri)
        {
            this.containerClient = new BlobContainerClient(new Uri(blobContainerUri), new DefaultAzureCredential(), null);
            this.containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public void HandleExportToAzureStorageAsync(string tableName, List<Dictionary<string, object>> tableData)
        {
            CreateCsvFileAndUpload(tableName, tableData);
            //CreateJsonFileAndUpload(tableName, tableData);

            //var headers = tableData.First().Keys;
            //var csvContent = new StringBuilder();
            //csvContent.AppendLine(string.Join(",", headers));

            //foreach (var record in tableData)
            //{
            //    var line = string.Join(",", headers.Select(header => record.ContainsKey(header) ? record[header] : ""));
            //    csvContent.AppendLine(line);
            //}

            //byte[] byteArray = Encoding.ASCII.GetBytes(csvContent.ToString());
            //MemoryStream stream = new MemoryStream(byteArray);

            //string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            //string fileName = currentDate + "/" + tableName + ".csv";

            //BlobClient blobClient = containerClient.GetBlobClient(fileName);
            //blobClient.Upload(stream, true);
        }
        private void CreateCsvFileAndUpload(string tableName, List<Dictionary<string, object>> tableData)
        {
            var headers = tableData.FirstOrDefault()?.Keys.ToArray();
            if (headers == null || headers.Length == 0)
            {
                Console.WriteLine($"No records found for table '{tableName}', skipping CSV generation.");
                return;
            }

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField(headers);
                csv.NextRecord();

                foreach (var record in tableData)
                {
                    foreach (var header in headers)
                    {
                        csv.WriteField(record.ContainsKey(header) ? record[header] : string.Empty);
                    }
                    csv.NextRecord();
                }

                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string fileName = currentDate + "/" + tableName + ".csv";
                BlobClient blobClient = this.containerClient.GetBlobClient(fileName);
                blobClient.Upload(memoryStream, true);
            }
        }

        private void CreateJsonFileAndUpload(string tableName, List<Dictionary<string, object>> tableData)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string fileName = currentDate + "/" + tableName + ".json";

            try
            {
                string jsonContent = JsonConvert.SerializeObject(tableData, Formatting.Indented);
                byte[] byteArray = Encoding.ASCII.GetBytes(jsonContent.ToString());
                MemoryStream memoryStream = new MemoryStream(byteArray);
                BlobClient blobClient = this.containerClient.GetBlobClient(fileName);
                blobClient.Upload(memoryStream, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create JSON file for table '{tableName}': {ex.Message}");
            }
        }

        public void UploadContentToBlob(string folderPath, string fileName, string content, string contentType)
        {
            string blobPath = $"{folderPath}/{fileName}";
            BlobClient blobClient = this.containerClient.GetBlobClient(blobPath);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                blobClient.Upload(memoryStream, new BlobHttpHeaders { ContentType = contentType });
            }
            Console.WriteLine($"Uploaded file to Blob: {blobPath}");
        }
    }
}
