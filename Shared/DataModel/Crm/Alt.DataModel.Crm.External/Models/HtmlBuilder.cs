using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Alt.DataModel.Crm.External.Models
{
    public class HtmlBuilder
    {

        public string CreateParagraph(string text)
        {
            XElement rootElement = new XElement("root");
            XElement reportHeader = new XElement(HtmlTags.p);
            reportHeader.SetValue(text);
            reportHeader.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.HeaderStyle);
            rootElement.Add(reportHeader);

            return rootElement.InnerXml().Replace("\"", "'");
        }
        public string CreateHeader(string header)
        {
            XElement rootElement = new XElement("root");
            XElement reportHeader = new XElement(HtmlTags.h4);
            reportHeader.SetValue(header);
            reportHeader.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.HeaderStyle);
            rootElement.Add(reportHeader);

            return rootElement.InnerXml().Replace("\"", "'");
        }

        public string CreateTable<T>(List<T> data) where T : class
        {
            XElement rootElement = new XElement("root");
            XElement divTable = new XElement(HtmlTags.div);
            rootElement.Add(divTable);

            XElement table = new XElement(HtmlTags.table);
            table.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.TableStyle);
            divTable.Add(table);

            XElement thead = new XElement(HtmlTags.thead);
            XElement theadRow = new XElement(HtmlTags.tr);
            theadRow.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.TheadRowStyle);
            thead.Add(theadRow);

            var headerColumns = this.GetHeaderContent<T>(data[0]);
            foreach (var item in headerColumns)
            {
               // theadRow.Add(CreateHeaderColumn(item, InlineStyleBuilder.MinWidth200px));
                theadRow.Add(CreateHeaderColumn(item));
            }
            XElement tbody = new XElement(HtmlTags.tbody);

            foreach (var row in data)
            {
                XElement tbodyRow = new XElement(HtmlTags.tr);
                var rowValues = this.GetRowValues<T>(row);

                foreach (var value in rowValues)
                {
                    tbodyRow.Add(CreateTableColumn(value));
                }
                tbody.Add(tbodyRow);
            }
            table.Add(thead);
            table.Add(tbody);
            return rootElement.InnerXml().Replace("\"", "'");

        }

        List<string> GetHeaderContent<T>(T data) where T : class
        {
            List<string> headers = new List<string>();

            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])property.
               GetCustomAttributes(typeof(DescriptionAttribute), false);
                string descriptionResult = attributes != null && attributes.Length > 0 ? attributes[0].Description : string.Empty;
                headers.Add(descriptionResult);
            }

            return headers;
        }

        List<string> GetRowValues<T>(T data) where T : class
        {
            List<string> values = new List<string>();
            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                object value = property.GetValue(data, null);
                string stringValue = value != null ? value.ToString() : string.Empty;
                values.Add(stringValue);
            }
            return values;
        }

        public string CreateErrorLogsTable(List<ErrorLogModel> errorsToWrite)
        {
            XElement rootElement = new XElement("root");
            XElement divTable = new XElement(HtmlTags.div);
            rootElement.Add(divTable);

            XElement table = new XElement(HtmlTags.table);
            table.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.TableStyle);
            divTable.Add(table);

            XElement thead = new XElement(HtmlTags.thead);
            XElement theadRow = new XElement(HtmlTags.tr);
            theadRow.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.TheadRowStyle);
            thead.Add(theadRow);

            theadRow.Add(CreateHeaderColumn("שם לוג",InlineStyleBuilder.MaxWidth300px));
            theadRow.Add(CreateHeaderColumn("הודעת שגיאה", InlineStyleBuilder.MinWidth200px));
            theadRow.Add(CreateHeaderColumn("סוג הודעה", InlineStyleBuilder.MinWidth100px));
            theadRow.Add(CreateHeaderColumn("מקור", InlineStyleBuilder.MinWidth100px));
            theadRow.Add(CreateHeaderColumn("כמות"));
            theadRow.Add(CreateHeaderColumn("תיאור", InlineStyleBuilder.MinWidth200px));
            theadRow.Add(CreateHeaderColumn("לינק", InlineStyleBuilder.MinWidth100px));

            XElement tbody = new XElement(HtmlTags.tbody);

            foreach (var errorToWrite in errorsToWrite)
            {
                XElement tbodyRow = new XElement(HtmlTags.tr);

                tbodyRow.Add(CreateErrorLogNameColumn(errorToWrite.Name));
                tbodyRow.Add(CreateTableColumn(errorToWrite.Message, InlineStyleBuilder.LtrDirection));
                tbodyRow.Add(CreateTableColumn(errorToWrite.MessageLevel));
                tbodyRow.Add(CreateTableColumn(errorToWrite.Source));
                tbodyRow.Add(CreateTableColumn(errorToWrite.Count.ToString()));
                tbodyRow.Add(CreateTableColumn(errorToWrite.Description));
                tbodyRow.Add(CreateTableUrlColumn(errorToWrite.Url));

                tbody.Add(tbodyRow);
            }
            table.Add(thead);
            table.Add(tbody);
            return rootElement.InnerXml().Replace("\"", "'");
        }

        public string CreateTableByRecordsList(List<Dictionary<string, object>> RecordsList, string organizationURL = null)
        {
            XElement rootElement = new XElement("root");
            XElement divTable = new XElement(HtmlTags.div);
            rootElement.Add(divTable);

            XElement table = new XElement(HtmlTags.table);
            table.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.TableStyle);
            divTable.Add(table);

            XElement thead = new XElement(HtmlTags.thead);
            XElement theadRow = new XElement(HtmlTags.tr);
            theadRow.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.TheadRowStyle);
            thead.Add(theadRow);

            foreach (var property in RecordsList.First())
            {
                theadRow.Add(CreateHeaderColumn(property.Key, InlineStyleBuilder.MaxWidth300px));
            }

            XElement tbody = new XElement(HtmlTags.tbody);

            foreach (var record in RecordsList)
            {
                XElement tbodyRow = new XElement(HtmlTags.tr);
                foreach (var property in record)
                {
                    if (property.Value != null)
                    {
                        if (property.Value is EntityReference)
                        {
                            EntityReference entityRef = (EntityReference)property.Value;
                            string recordURL = CreateRecordUrl(organizationURL, entityRef.LogicalName, entityRef.Id.ToString());
                            tbodyRow.Add(CreateTableURLColumn(recordURL, entityRef.Name));
                        }
                        else
                        {
                            tbodyRow.Add(CreateTableColumn(property.Value.ToString()));
                        }
                    }
                }
                tbody.Add(tbodyRow);
            }
            
            table.Add(thead);
            table.Add(tbody);
            return rootElement.InnerXml().Replace("\"", "'");
        }

        private XElement CreateErrorLogNameColumn(string value)
        {
            XElement column = new XElement(HtmlTags.td);

            string columnStyle; ;

            if (value.StartsWith("Alt.Crm"))
            {
                columnStyle = InlineStyleBuilder.PluginsCollumnStyle;
            }
            else if (value.StartsWith("Alt.External.WebJobs"))
            {
                columnStyle = InlineStyleBuilder.OutgoingCollumnStyle;
            }
            else if (value.StartsWith("Alt.External.Services"))
            {
                columnStyle = InlineStyleBuilder.CrmApiCollumnStyle;
            }
            else
            {
                columnStyle = string.Empty;
            }
            column.SetAttributeValue(InlineStyleBuilder.Style, $"{InlineStyleBuilder.MaxWidth300px}{InlineStyleBuilder.WordWrapBreakWord}{columnStyle}");
            column.SetValue(value);
            return column;
        }

        private XElement CreateTableUrlColumn(string value, string spetialStyle = null)
        {
            XElement column = new XElement(HtmlTags.td);

            string columnStyle = $"{InlineStyleBuilder.Padding}{InlineStyleBuilder.TbodyBorderBottom}{spetialStyle ?? string.Empty}";
            column.SetAttributeValue(InlineStyleBuilder.Style, columnStyle);
            XElement link = new XElement(HtmlTags.a);
            link.SetAttributeValue("href", value ?? string.Empty);
            link.SetValue("קישור ללוג");
            column.Add(link);

            return column;
        }
        
        private XElement CreateTableURLColumn(string urlValue, string urlLabel, string specialStyle = null)
        {
            XElement column = new XElement(HtmlTags.td);

            string columnStyle = $"{InlineStyleBuilder.Padding}{InlineStyleBuilder.TbodyBorderBottom}{specialStyle ?? string.Empty}";
            column.SetAttributeValue(InlineStyleBuilder.Style, columnStyle);
            XElement link = new XElement(HtmlTags.a);
            link.SetAttributeValue("href", urlValue ?? string.Empty);
            link.SetValue(urlLabel ?? "ללא שם");
            column.Add(link);

            return column;
        }

        private XElement CreateTableColumn(string value, string spetialStyle = null)
        {
            XElement column = new XElement(HtmlTags.td);
            string columnStyle = $"{InlineStyleBuilder.Padding}{InlineStyleBuilder.TbodyBorderBottom}{spetialStyle ?? string.Empty}";
            column.SetAttributeValue(InlineStyleBuilder.Style, columnStyle);
            column.SetValue(value ?? string.Empty);

            return column;
        }

        private XElement CreateHeaderColumn(string value, string spetialStyle = null)
        {
            XElement column = new XElement(HtmlTags.td);
            string columnStyle = $"{InlineStyleBuilder.Padding}{InlineStyleBuilder.TheadBorderBottom}{spetialStyle ?? string.Empty}";
            column.SetAttributeValue(InlineStyleBuilder.Style, columnStyle);
            column.SetValue(value ?? string.Empty);

            return column;
        }

        public string CreateTableByPropertiesList(string tableHeader, List<Dictionary<string, object>> recordsPropertiesList, string emptyResultMessage = null, string organizationURL = null)
        {
            string header = CreateHeader(tableHeader);

            return recordsPropertiesList?.Count > 0 ?
                $"{header}{CreateTableByRecordsList(recordsPropertiesList, organizationURL)}" : $"{header}{emptyResultMessage}";
        }

        internal string CreateErrorLogsCommentsList(Dictionary<string, List<Comment>> comments, string listTitle)
        {
            XElement rootElement = new XElement("root");
            XElement divList = new XElement(HtmlTags.div);
            rootElement.Add(divList);

            XElement orderedList = new XElement(HtmlTags.ol);
            orderedList.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.ErrorLogsCommentsListStyle);
            orderedList.SetValue(listTitle);
            divList.Add(orderedList);

            if (comments != null && comments.Count > 0)
            {
                foreach (var item in comments)
                {
                    XElement listItem = new XElement(HtmlTags.li);
                    listItem.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.Margin);
                    orderedList.Add(listItem);
                    string listItemValue = item.Key;
                    listItem.SetValue(listItemValue);

                    if (item.Value != null && item.Value.Count > 0)
                    {
                        XElement unorderedList = new XElement(HtmlTags.ul);
                        unorderedList.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.Margin);
                        listItem.Add(unorderedList);
                        foreach (Comment comment in item.Value)
                        {
                            XElement unorderedListItem = new XElement(HtmlTags.li);
                            unorderedListItem.SetAttributeValue(InlineStyleBuilder.Style, InlineStyleBuilder.MarginRight);
                            unorderedListItem.SetValue(GetContentAsString(comment.Content));
                            unorderedList.Add(unorderedListItem);
                        }
                    }
                }
                divList.Add(new XElement(HtmlTags.NewLine));
                divList.Add(new XElement(HtmlTags.NewLine));
            }
            return rootElement.InnerXml().Replace("&gt;", ">").Replace("&lt;", "<").Replace("\"", "'");
        }

        private string GetContentAsString(List<string> content)
        {
            string value = string.Empty;
            string newLine = "<br/>";
            if (content != null)
            {
                foreach (string contentItem in content)
                {
                    value += $"{contentItem}{newLine}";
                }
            }
            return value;
        }

        private string CreateRecordUrl(string organizationURL, string entityName, string id)
        {
            return $"{organizationURL}/main.aspx?etn={entityName}&id={id}&newWindow=true&pagetype=entityrecord";
        }
    }
}
