using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Alt.Framework.External.Utils
{
    public class CsvUtils
    {
        public static List<dynamic> LoadFromCsvStream(TextReader textReader)
        {
            IReaderConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                BadDataFound = null,
                IgnoreBlankLines = true,
                MissingFieldFound = null,
                ReadingExceptionOccurred = null,
                Encoding = Encoding.GetEncoding("Windows-1255")
            };
            using (var csv = new CsvReader(textReader, configuration))
            {
                var records = csv.GetRecords<dynamic>();
                List<dynamic> list = Enumerable.ToList<dynamic>(records);
                return list;
            }
        }

        public static string GenerateCsvContentFromTableData(List<Dictionary<string, object>> tableData, string[] csvHeaders = null)
        {
            // Use a MemoryStream to handle UTF-8 encoding
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (tableData.Count > 0)
                {
                    // Write headers: Iterate through the keys of the first record to get headers, or from input parameter if not null
                    var headers = csvHeaders ?? tableData[0].Keys.ToArray();
                    csv.WriteField(headers);
                    csv.NextRecord();

                    // Write records: For each record, write its values
                    foreach (var record in tableData)
                    {
                        foreach (var header in headers)
                        {
                            // Check if the record contains the header key and write its value
                            csv.WriteField(record.ContainsKey(header) ? record[header]?.ToString() : string.Empty);
                        }
                        csv.NextRecord();
                    }
                }

                // Flush the writer to ensure all data is written to the MemoryStream
                writer.Flush();

                // Get the CSV content as a UTF-8 encoded string
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
