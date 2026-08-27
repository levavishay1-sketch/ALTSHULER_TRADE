using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Alt.BusinessLogicLayer.Crm.External.Reports
{
    public class FetchXMLReportBL : ExternalBLBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public FetchXMLReportBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult HandleFetchXMLReportScheduledOperation(ApiSchedulerSetup apiSchedulerSetup)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            FetchXMLReportConfig reportConfiguration = base.GetDeserializedContent<FetchXMLReportConfig>(apiSchedulerSetup.DevelopmentSettings);

            List<string> fetchXMLsList = new List<string>();
            if (reportConfiguration?.ReportsFetchParams?.Length > 0)
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (string parameterName in reportConfiguration.ReportsFetchParams)
                    {
                        string fetchXML = GlobalContext.CacheManager.GetGlobalParameter<string>(parameterName);
                        if (!string.IsNullOrWhiteSpace(fetchXML))
                        {
                            string table = this.GenerateReportTableAsString(fetchXML, parameterName, reportConfiguration.EmptyResultMessage);
                            if (!string.IsNullOrEmpty(table))
                            {
                                stringBuilder.Append($"{Environment.NewLine}{table}");
                                fetchXMLsList.Add(fetchXML);
                            }
                        }
                    }
                    actionResult.ReturnObject = stringBuilder?.ToString();
                }
                catch (Exception ex)
                {
                    actionResult.SetToFailedActionResult(ex.ToString());
                }

            }

            return actionResult;
        }

        public List<Dictionary<string, object>> GetCSVPage(string viewName)
        {
            this.GlobalContext.LogEntry();

            List<Dictionary<string, object>> propertiesList = new List<Dictionary<string, object>>();
            CommonDAL commonDAL = new CommonDAL(GlobalContext, null);
            Entity view = commonDAL.GetEntityViewByName(viewName);
            if (view == null)
            {
                view = commonDAL.GetEntityUserViewByName(viewName);
            }

            if (view != null)
            {
                view.TryGetAttributeValue("fetchxml", out string fetchXML);
                view.TryGetAttributeValue("layoutxml", out string layoutXML);

                propertiesList = FetchDataRecordsPropertiesList(fetchXML, layoutXML);
            }
            else
            {
                this.GlobalContext.Log.Warning($"View name not found:\n{viewName}");
            }

            return propertiesList;
        }


        private List<Dictionary<string, object>> FetchDataRecordsPropertiesList(string fetchXML, string layoutXML)
        {
            GlobalContext.LogEntry();
            List<Dictionary<string, object>> propertiesList = new List<Dictionary<string, object>>();

            DynamicsExportDataDAL dynamicsExportDataDAL = new DynamicsExportDataDAL(GlobalContext);
            List<Entity> retrievedRecords = dynamicsExportDataDAL.RetrieveByFetch(fetchXML);

            List<FetchAttributeInfo> attributesInfo = ExtractAttributesInfo(fetchXML);
            attributesInfo = SortAttributesInfoByLayoutXML(attributesInfo, layoutXML);

            HashSet<string> entityNames = new HashSet<string>(attributesInfo.Select(a => a.EntityLogicalName));
            Dictionary<string, EntityMetadata> metadata = dynamicsExportDataDAL.GetMultipleEntitiesMetadataNew(entityNames);

            if (retrievedRecords?.Count > 0)
            {
                propertiesList = GenerateRowsPropertiesWithColumnLabels(retrievedRecords, metadata, attributesInfo);
            }
            else
            {
                propertiesList.Add(GenerateEmptyRowPropertiesWithColumnLabels(metadata, attributesInfo));
            }

            return propertiesList;
        }

        private List<FetchAttributeInfo> ExtractAttributesInfo(string fetchXml)
        {
            GlobalContext.LogEntry();
            XDocument doc = XDocument.Parse(fetchXml);
            var result = new List<FetchAttributeInfo>();

            XElement primaryEntity = doc.Descendants("entity").FirstOrDefault();
            string primaryEntityName = primaryEntity.Attribute("name")?.Value;

            if (primaryEntity != null)
            {
                foreach (XElement attr in primaryEntity.Elements("attribute"))
                {
                    result.Add(new FetchAttributeInfo
                    {
                        EntityLogicalName = primaryEntityName,
                        AttributeLogicalName = attr.Attribute("name")?.Value,
                    });
                }
            }

            foreach (XElement link in doc.Descendants("link-entity"))
            {
                string linkedEntityName = link.Attribute("name")?.Value;
                string alias = link.Attribute("alias")?.Value;
                string to = link.Attribute("to")?.Value;

                foreach (XElement attr in link.Elements("attribute"))
                {
                    result.Add(new FetchAttributeInfo
                    {
                        EntityLogicalName = linkedEntityName,
                        AttributeLogicalName = attr.Attribute("name")?.Value,
                        Alias = alias,
                        PrimaryEntityAttributeLogicalName = to,
                        PrimaryEntityLogicalName = primaryEntityName,
                        IsLinkedEntity = true
                    });
                }
            }

            return result;
        }

        private List<FetchAttributeInfo> SortAttributesInfoByLayoutXML(List<FetchAttributeInfo> fetchAttributesInfo, string layoutXML)
        {
            GlobalContext.LogEntry();
            List<FetchAttributeInfo> orderedFetchAttributesInfo = null;

            if (!string.IsNullOrWhiteSpace(layoutXML))
            {
                XDocument layoutDoc = XDocument.Parse(layoutXML);

                List<string> layoutColumnLogicalNames = layoutDoc
                    .Descendants("cell")
                    .Select(c => c.Attribute("name")?.Value)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                orderedFetchAttributesInfo = new List<FetchAttributeInfo>();

                foreach (string logicalName in layoutColumnLogicalNames)
                {
                    string[] aliasAndName = logicalName.Split('.');

                    FetchAttributeInfo attributeInfo = aliasAndName.Length > 1 ?
                        fetchAttributesInfo.FirstOrDefault(f => f.Alias == aliasAndName[0] && f.AttributeLogicalName == aliasAndName[1])
                        : fetchAttributesInfo.FirstOrDefault(f => f.AttributeLogicalName == logicalName);

                    if (attributeInfo != null)
                    {
                        orderedFetchAttributesInfo.Add(attributeInfo);
                    }
                }

                string primaryIdAttributeName = layoutDoc.Descendants("row")
                    .Select(r => r.Attribute("id")?.Value)
                    .FirstOrDefault();
                if (primaryIdAttributeName != null)
                {
                    FetchAttributeInfo primaryIdAttribute = fetchAttributesInfo.FirstOrDefault(f => f.AttributeLogicalName == primaryIdAttributeName);
                    if (!orderedFetchAttributesInfo.Any(o => o.AttributeLogicalName == primaryIdAttributeName))
                    {
                        orderedFetchAttributesInfo.Add(primaryIdAttribute);
                    }
                }
            }

            return orderedFetchAttributesInfo;
        }

        private List<Dictionary<string, object>> GenerateRowsPropertiesWithColumnLabels(
            List<Entity> records,
            Dictionary<string, EntityMetadata> metadata,
            List<FetchAttributeInfo> orderedAttributes)
        {
            GlobalContext.LogEntry();
            List<Dictionary<string, object>> output = new List<Dictionary<string, object>>();
            string primaryEntityLogicalName = records.First().LogicalName;

            foreach (Entity record in records)
            {
                var row = new Dictionary<string, object>();

                foreach (FetchAttributeInfo attrInfo in orderedAttributes)
                {
                    string entityName = attrInfo.EntityLogicalName;
                    string logicalName = attrInfo.AttributeLogicalName;

                    object attributeValue = null;

                    if (attrInfo.IsLinkedEntity)
                    {
                        AliasedValue aliased = record.Attributes
                        .Where(a => a.Value is AliasedValue av &&
                                    (av.EntityLogicalName == attrInfo.EntityLogicalName ||
                                     a.Key.StartsWith(attrInfo.Alias + ".", StringComparison.OrdinalIgnoreCase)))
                        .Select(a => a.Value as AliasedValue)
                        .FirstOrDefault();

                        if (aliased != null)
                        {
                            attributeValue = aliased.Value;
                        }
                    }
                    else if (record.Attributes.ContainsKey(logicalName))
                    {
                        attributeValue = record[logicalName];
                    }

                    object formattedValue = FormatValue(attributeValue, metadata[entityName], logicalName);

                    AttributeMetadata attrMeta = metadata[entityName].Attributes.FirstOrDefault(a => a.LogicalName == logicalName);
                    string label = attrMeta?.DisplayName?.UserLocalizedLabel?.Label ?? logicalName;

                    if (attrInfo.IsLinkedEntity)
                    {
                        AttributeMetadata primaryAttrMeta = metadata[attrInfo.PrimaryEntityLogicalName].Attributes
                            .FirstOrDefault(a => a.LogicalName == attrInfo.PrimaryEntityAttributeLogicalName);
                        string entityDisplayName = primaryAttrMeta?.DisplayName?.UserLocalizedLabel?.Label ?? entityName;

                        label = $"{label} ({entityDisplayName})";
                    }

                    row[label] = formattedValue;
                }

                output.Add(row);
            }

            return output;
        }

        private Dictionary<string, object> GenerateEmptyRowPropertiesWithColumnLabels(
            Dictionary<string, EntityMetadata> metadata,
            List<FetchAttributeInfo> orderedAttributes)
        {
            GlobalContext.LogEntry();
            Dictionary<string, object> row = new Dictionary<string, object>();

            foreach (FetchAttributeInfo attrInfo in orderedAttributes)
            {
                string entityName = attrInfo.EntityLogicalName;
                string logicalName = attrInfo.AttributeLogicalName;

                AttributeMetadata attrMeta = metadata[entityName].Attributes.FirstOrDefault(a => a.LogicalName == logicalName);
                string label = attrMeta?.DisplayName?.UserLocalizedLabel?.Label ?? logicalName;

                if (attrInfo.IsLinkedEntity)
                {
                    AttributeMetadata primaryAttrMeta = metadata[attrInfo.PrimaryEntityLogicalName].Attributes
                            .FirstOrDefault(a => a.LogicalName == attrInfo.PrimaryEntityAttributeLogicalName);

                    string entityDisplayName = primaryAttrMeta?.DisplayName?.UserLocalizedLabel?.Label ?? entityName;
                    label = $"{label} ({entityDisplayName})";
                }

                row[label] = null;
            }

            return row;
        }

        private object FormatValue(object value, EntityMetadata entityMeta, string logicalName)
        {
            object formattedValue = null;
            if (value != null)
            {
                switch (value)
                {
                    case OptionSetValue opt:
                        {
                            EnumAttributeMetadata attrMeta = entityMeta.Attributes
                                .FirstOrDefault(a => a.LogicalName == logicalName) as EnumAttributeMetadata;

                            OptionMetadata option = attrMeta?.OptionSet?.Options.FirstOrDefault(o => o.Value == opt.Value);

                            formattedValue = option?.Label?.UserLocalizedLabel?.Label ?? opt.Value.ToString();
                            break;
                        }
                    case EntityReference lookup:
                        {
                            formattedValue = lookup.Name;
                            break;
                        }
                    case Money money:
                        {
                            formattedValue = money.Value;
                            break;
                        }
                    case DateTime dateTimeValue:
                        {
                            formattedValue = dateTimeValue.ToString("dd/MM/yyyy HH:mm");
                            break;
                        }
                    case bool booleanValue:
                        {
                            formattedValue = booleanValue ? "כן" : "לא";
                            break;
                        }
                    case string stringValue:
                        {
                            formattedValue = stringValue != null ? stringValue.Replace("\n", " ") : string.Empty;
                            break;
                        }
                    default:
                        formattedValue = value;
                        break;
                }
            }

            return formattedValue;
        }

        private List<Dictionary<string, object>> GetDataRecordsPropertiesList(string fetchXML, string layoutXML = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fetchXML);

            XmlNodeList entityNodeList = doc.GetElementsByTagName("entity");
            string entityLogicalName = entityNodeList[0].Attributes.GetNamedItem("name").Value;

            XmlNodeList linkedEntitiesNodeList = doc.GetElementsByTagName("link-entity");
            Dictionary<string, string> linkedEntityNameAliasPairs = new Dictionary<string, string>();
            if (linkedEntitiesNodeList?.Count > 0)
            {
                foreach (XmlNode linkedEntityNode in linkedEntitiesNodeList)
                {
                    string linkedEntityName = linkedEntityNode.Attributes.GetNamedItem("name").Value;
                    string linkedEntityAlias = linkedEntityNode.Attributes.GetNamedItem("alias").Value;
                    linkedEntityNameAliasPairs.Add(linkedEntityName, linkedEntityAlias);
                }
            }

            DynamicsExportDataDAL dynamicsExportDataDAL = new DynamicsExportDataDAL(GlobalContext);
            List<Dictionary<string, object>> recordsPropertiesList = dynamicsExportDataDAL.GetEntityDataWithColumnLabels(entityLogicalName, linkedEntityNameAliasPairs, fetchXML, layoutXML);
            return recordsPropertiesList;
        }

        public byte[] GetCSVAsByteArray(List<Dictionary<string, object>> tableData)
        {
            GlobalContext.LogEntry();
            byte[] byteArray = null;
            if (tableData?.Count > 0)
            {
                var headers = tableData.First().Keys;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(memoryStream, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join(",", headers));

                        foreach (var record in tableData)
                        {
                            string line = string.Join(",", headers.Select(header =>
                                record.ContainsKey(header) ? EscapeSpecialCharacters(record[header]) : ""));

                            writer.WriteLine(line);
                        }

                        writer.Flush();
                    }
                    byteArray = memoryStream.ToArray();

                    memoryStream.Flush();
                }
            }

            return byteArray;
        }

        private string EscapeSpecialCharacters(object value)
        {
            if (value == null)
                return "";

            string s = value.ToString();

            // Escape quotes by doubling them
            if (s.Contains("\""))
                s = s.Replace("\"", "\"\"");

            // If contains comma, newline, or quote → wrap in quotes
            if (s.Contains(",") || s.Contains("\n") || s.Contains("\""))
                s = $"\"{s}\"";

            return s;
        }


        private string GenerateReportTableAsString(string fetchXML, string fetchName, string emptyResultMessage)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fetchXML);

            XmlNodeList entityNodeList = doc.GetElementsByTagName("entity");
            string entityLogicalName = entityNodeList[0].Attributes.GetNamedItem("name").Value;

            XmlNodeList linkedEntitiesNodeList = doc.GetElementsByTagName("link-entity");
            Dictionary<string, string> linkedEntityNameAliasPairs = new Dictionary<string, string>();
            if (linkedEntitiesNodeList?.Count > 0)
            {
                foreach (XmlNode linkedEntityNode in linkedEntitiesNodeList)
                {
                    string linkedEntityName = linkedEntityNode.Attributes.GetNamedItem("name").Value;
                    string linkedEntityAlias = linkedEntityNode.Attributes.GetNamedItem("alias").Value;
                    linkedEntityNameAliasPairs.Add(linkedEntityName, linkedEntityAlias);
                }
            }

            DynamicsExportDataDAL dynamicsExportDataDAL = new DynamicsExportDataDAL(GlobalContext);
            List<Dictionary<string, object>> recordsPropertiesList = dynamicsExportDataDAL.GetEntityDataWithColumnLabels(entityLogicalName, linkedEntityNameAliasPairs, fetchXML);
            if (recordsPropertiesList == null)
            {
                return string.Empty;
            }
            else
            {
                HtmlBuilder htmlBuilder = new HtmlBuilder();
                return htmlBuilder.CreateTableByPropertiesList(fetchName, recordsPropertiesList, emptyResultMessage, GlobalContext.OrganizationUrl);
            }
        }
    }
}
