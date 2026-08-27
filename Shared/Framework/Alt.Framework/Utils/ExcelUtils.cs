using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Alt.Framework.Utils
{
    public static class ExcelUtils
    {
        public static byte[] ConvertToExcel(
            byte[] firstPage, byte[] secondPage, byte[] thirdPage,
            string firstPageName, string secondPageName, string thirdPageName
        )
        {
            var rows1 = ParseCsv(Encoding.UTF8.GetString(firstPage));
            var rows2 = ParseCsv(Encoding.UTF8.GetString(secondPage));
            var rows3 = ParseCsv(Encoding.UTF8.GetString(thirdPage)); ;

            using (var stream = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    // Create workbook
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    // Create shared string table (correct way to store text)
                    var sharedStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
                    sharedStringPart.SharedStringTable = new SharedStringTable();

                    // Sheets container
                    var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                    // Add sheets
                    CreateSheet(workbookPart, sharedStringPart, sheets, rows1, firstPageName, 1);
                    CreateSheet(workbookPart, sharedStringPart, sheets, rows2, secondPageName, 2);
                    CreateSheet(workbookPart, sharedStringPart, sheets, rows3, thirdPageName, 3);

                    // Save shared strings + workbook
                    sharedStringPart.SharedStringTable.Save();
                    workbookPart.Workbook.Save();
                }
                // IMPORTANT: document is fully closed here

                return stream.ToArray(); // NOW safe to read
            }
        }


        private static void CreateSheet(
            WorkbookPart workbookPart,
            SharedStringTablePart sharedStringPart,
            Sheets sheets,
            IEnumerable<string[]> rows,
            string sheetName,
            uint sheetId)
        {
            var wsPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            wsPart.Worksheet = new Worksheet(sheetData);

            // Register sheet
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(wsPart),
                SheetId = sheetId,
                Name = sheetName
            });

            // Fill rows
            foreach (var rowValues in rows)
            {
                var row = new Row();

                foreach (var cellValue in rowValues)
                    row.Append(CreateSharedStringCell(sharedStringPart, cellValue ?? string.Empty));

                sheetData.Append(row);
            }

            wsPart.Worksheet.Save();
        }

        private static Cell CreateSharedStringCell(SharedStringTablePart sharedStringPart, string text)
        {
            int index = InsertSharedString(sharedStringPart, text);

            return new Cell
            {
                DataType = CellValues.SharedString,
                CellValue = new CellValue(index.ToString())
            };
        }

        private static int InsertSharedString(SharedStringTablePart sharedStringPart, string text)
        {
            var table = sharedStringPart.SharedStringTable;

            int index = 0;

            // Compare using Text.Text (correct)
            foreach (SharedStringItem item in table.Elements<SharedStringItem>())
            {
                if (item.Text?.Text == text)
                    return index;

                index++;
            }

            // Add new shared string
            table.AppendChild(new SharedStringItem(new Text(text)));

            return index;
        }

        private static IEnumerable<string[]> ParseCsv(string csv)
        {
            var result = new List<string[]>();

            using (var reader = new StringReader(csv))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = new List<string>();
                    var sb = new StringBuilder();
                    bool inQuotes = false;

                    for (int i = 0; i < line.Length; i++)
                    {
                        char c = line[i];

                        if (c == '"')
                        {
                            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                            {
                                sb.Append('"');
                                i++;
                            }
                            else
                            {
                                inQuotes = !inQuotes;
                            }
                        }
                        else if (c == ',' && !inQuotes)
                        {
                            fields.Add(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    fields.Add(sb.ToString());
                    result.Add(fields.ToArray());
                }
            }

            return result;
        }
    }
}
