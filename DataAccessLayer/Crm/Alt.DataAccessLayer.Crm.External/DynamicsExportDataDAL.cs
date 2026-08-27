using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class DynamicsExportDataDAL : CrmExternalBaseDAL<ApiEntity>
    {
        public DynamicsExportDataDAL(GlobalContext globalContext) : base(globalContext, null) { }

        public (List<Dictionary<string, object>> tableData, string[] headers) GetExportData(string tableName, CSVExportExtraColumns extraColumns)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, AttributeMetadata> entityMetadata = GetEntityMetaData(tableName);
            EntityCollection entityRecords = GetEntityRecords(tableName);
            EntityInfo info = extraColumns.Entities.Find(entityInfo => entityInfo.LogicalName == tableName);
            string[] csvHeaders = GenerateHeadersForCSVFile(entityMetadata, info);
            return (GetEntityProperties(entityRecords, entityMetadata, info), csvHeaders);
        }

        public List<Dictionary<string, object>> GetEntityDataWithColumnLabels(string entityLogicalName, Dictionary<string, string> linkedEntityNameAliasPairs, string fetchXML)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, AttributeMetadata> entityMetadata = GetEntityMetaData(entityLogicalName);
            List<Dictionary<string, AttributeMetadata>> linkedEntitiesMetadata = null;
            if (linkedEntityNameAliasPairs?.Count > 0)
            {
                List<string> linkedEntityNames = linkedEntityNameAliasPairs.Select(linkedEntity => linkedEntity.Key).ToList();
                linkedEntitiesMetadata = GetMultipleEntitiesMetaData(linkedEntityNames);
            }

            EntityCollection retrievedEntityRecords = Fetch(new FetchExpression(fetchXML));

            List<Dictionary<string, object>> entityProperties = null;
            if (retrievedEntityRecords?.Entities?.Count > 0)
            {
                entityProperties = MapEntityPropertiesWithLabels(retrievedEntityRecords, entityMetadata, linkedEntitiesMetadata, linkedEntityNameAliasPairs);
            }

            return entityProperties;
        }

        private Dictionary<string, AttributeMetadata> GetEntityMetaData(string entityName)
        {
            this.GlobalContext.LogEntry();
            var request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityName
            };

            var response = (RetrieveEntityResponse)OrganizationService.Execute(request);
            return response.EntityMetadata.Attributes
                .ToDictionary(attr => attr.LogicalName, attr => attr);
        }

        private List<Dictionary<string, AttributeMetadata>> GetMultipleEntitiesMetaData(List<string> entitiesNames)
        {
            this.GlobalContext.LogEntry();
            OrganizationRequestCollection organizationRequestCollection = new OrganizationRequestCollection();
            foreach (string entityName in entitiesNames)
            {
                RetrieveEntityRequest request = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Attributes,
                    LogicalName = entityName
                };
                organizationRequestCollection.Add(request);
            }

            ExecuteMultipleResponse multipleResponse = ExecuteMultipleRequests(organizationRequestCollection, false);
            List<Dictionary<string, AttributeMetadata>> retrievedEntitiesMetadata = multipleResponse?.Responses
                                        .Select(response =>
                                            ((RetrieveEntityResponse)response.Response).EntityMetadata.Attributes
                                                .ToDictionary(attr => attr.LogicalName, attr => attr))
                                        .ToList();

            return retrievedEntitiesMetadata;
        }

        private EntityCollection GetEntityRecords(string entityName)
        {
            var query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(true)
            };

            return base.GetEntityCollectionWithPaging(query); // protected method from CrmExternalBaseDAL
        }

        private List<Dictionary<string, object>> MapEntityPropertiesWithLabels(EntityCollection entityRecords,
                                                                               Dictionary<string, AttributeMetadata> mainEntityMetadata,
                                                                               List<Dictionary<string, AttributeMetadata>> linkedEntitiesMetadataList,
                                                                               Dictionary<string, string> linkedEntityNameAliasPairs)
        {
            this.GlobalContext.LogEntry();
            List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();
            Dictionary<string, object> emptyRecordDataForColumnNamesOrder = new Dictionary<string, object>();

            foreach (Entity entity in entityRecords.Entities)
            {
                Dictionary<string, object> recordData = new Dictionary<string, object>();

                foreach (var entityAttribute in entity.Attributes)
                {
                    string[] linkedEntityAttributeAliasSplit = entityAttribute.Key.Split('.');
                    int aliasIndex = 0, columnIndex = 1;

                    AttributeMetadata attributeMetadata = null;
                    string linkedEntityDisplayName = string.Empty;
                    Dictionary<string, AttributeMetadata> linkedEntityMetadata = null;

                    if (linkedEntityAttributeAliasSplit.Length > 1 && linkedEntitiesMetadataList?.Count > 0)
                    {
                        string linkedEntityLogicalName = linkedEntityNameAliasPairs.First(pair => pair.Value == linkedEntityAttributeAliasSplit[aliasIndex]).Key;
                        linkedEntityMetadata = linkedEntitiesMetadataList.First(linkedMetadata =>
                                                                         linkedMetadata.Values.Any(linkedValue =>
                                                                                                   linkedValue.EntityLogicalName == linkedEntityLogicalName));

                        attributeMetadata = linkedEntityMetadata.First(attrMetadata => attrMetadata.Key == linkedEntityAttributeAliasSplit[columnIndex]).Value;

                        linkedEntityDisplayName = "(" + mainEntityMetadata.First(entMetadata => entMetadata.Key == linkedEntityLogicalName + "id")
                                                                          .Value.DisplayName.LocalizedLabels
                                                                          .First().Label + ")";
                    }
                    else
                    {
                        attributeMetadata = mainEntityMetadata.Values.First(metadata => metadata.LogicalName == entityAttribute.Key);
                    }

                    string attributeDisplayName = $"{linkedEntityDisplayName}{attributeMetadata.DisplayName.LocalizedLabels.FirstOrDefault()?.Label}";
                    if (!string.IsNullOrWhiteSpace(attributeDisplayName))
                    {
                        emptyRecordDataForColumnNamesOrder[attributeDisplayName] = string.Empty;
                        string attributeName = attributeMetadata.LogicalName;
                        bool isLinkedEntityValue = linkedEntityAttributeAliasSplit?.Length > 1 && entityAttribute.Value != null;

                        object attributeValue = isLinkedEntityValue ? ((AliasedValue)entityAttribute.Value).Value
                                                                    : entity.Attributes.ContainsKey(attributeName) ? entity.Attributes[attributeName]
                                                                    : null;

                        if (attributeMetadata.IsPrimaryName.HasValue && attributeMetadata.IsPrimaryName.Value)
                        {
                            attributeValue = new EntityReference(entity.LogicalName, entity.Id) { Name = entity.Attributes[attributeName].ToString() };
                        }

                        Dictionary<string, AttributeMetadata> entityMetadataToMap = isLinkedEntityValue ? linkedEntityMetadata
                                                                                                        : mainEntityMetadata;

                        KeyValuePair<string, object> recordColumnData = MapRecordColumnData(entityMetadataToMap, attributeDisplayName, attributeName, attributeValue);
                        recordData[recordColumnData.Key] = recordColumnData.Value;
                    }
                }

                if (recordData.Count > 0)
                {
                    tableData.Add(recordData);
                }
            }

            List<Dictionary<string, object>> orderedTableData = ReorderTableDataValuesByRecordData(tableData, emptyRecordDataForColumnNamesOrder);
            return orderedTableData;
        }

        public Dictionary<string, EntityMetadata> GetMultipleEntitiesMetadataNew(HashSet<string> entitiesNames)
        {
            this.GlobalContext.LogEntry();
            OrganizationRequestCollection organizationRequestCollection = new OrganizationRequestCollection();
            foreach (string entityName in entitiesNames)
            {
                RetrieveEntityRequest request = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Attributes,
                    LogicalName = entityName
                };
                organizationRequestCollection.Add(request);
            }

            ExecuteMultipleResponse multipleResponse = ExecuteMultipleRequests(organizationRequestCollection, false);

            Dictionary<string, EntityMetadata> retrievedEntitiesMetadata = multipleResponse?.Responses
                                        .Select(response => ((RetrieveEntityResponse)response.Response).EntityMetadata)
                                                .ToDictionary(metadata => metadata.LogicalName, metadata => metadata);

            return retrievedEntitiesMetadata;
        }

        public List<Entity> RetrieveByFetch(string fetchXML)
        {
            GlobalContext.LogEntry();
            EntityCollection retrievedEntityRecords = Fetch(new FetchExpression(fetchXML));
            retrievedEntityRecords.Entities.OrderBy(e => e["alt_zipfiledate"]);
            GlobalContext.Log.Info($"\nretrievedEntityRecords.Count: {retrievedEntityRecords?.Entities?.Count}");
            return retrievedEntityRecords?.Entities?.ToList();
        }

        private List<Dictionary<string, object>> ReorderTableDataValuesByRecordData(List<Dictionary<string, object>> tableData, Dictionary<string, object> emptyRecordDataForColumnNames)
        {
            GlobalContext.LogEntry();
            List<Dictionary<string, object>> orderedTableData = new List<Dictionary<string, object>>();
            foreach (var recordData in tableData)
            {
                Dictionary<string, object> orderedRecordData = new Dictionary<string, object>(emptyRecordDataForColumnNames);
                foreach (var record in recordData)
                {
                    orderedRecordData[record.Key] = record.Value;
                }

                orderedTableData.Add(orderedRecordData);
            }

            return orderedTableData;
        }

        private List<Dictionary<string, object>> ReorderTableDataValuesByLayoutXML(List<Dictionary<string, object>> tableData,
            Dictionary<string, AttributeMetadata> mainEntityMetadata,
            List<Dictionary<string, AttributeMetadata>> linkedEntitiesMetadataList, string layoutXML)
        {
            GlobalContext.LogEntry();
            List<Dictionary<string, object>> orderedTableData = null;

            if (!string.IsNullOrWhiteSpace(layoutXML))
            {
                var layoutDoc = System.Xml.Linq.XDocument.Parse(layoutXML);

                List<string> layoutColumnLogicalNames = layoutDoc
                    .Descendants("cell")
                    .Select(c => c.Attribute("name")?.Value)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                List<string> orderedDisplayLabels =
                    GenerateDisplayLabelsByMetadata(mainEntityMetadata, linkedEntitiesMetadataList, layoutColumnLogicalNames);

                orderedTableData = new List<Dictionary<string, object>>();

                foreach (var record in tableData)
                {
                    Dictionary<string, object> orderedRecord = new Dictionary<string, object>();

                    // Add layout columns first
                    foreach (string label in orderedDisplayLabels)
                    {
                        if (record.ContainsKey(label))
                            orderedRecord[label] = record[label];
                        else
                            orderedRecord[label] = null;
                    }

                    // Append any remaining columns
                    foreach (var attribute in record)
                    {
                        if (!orderedRecord.ContainsKey(attribute.Key))
                            orderedRecord[attribute.Key] = attribute.Value;
                    }

                    orderedTableData.Add(orderedRecord);
                }
            }

            return orderedTableData;
        }

        private List<string> GenerateDisplayLabelsByMetadata(Dictionary<string, AttributeMetadata> mainEntityMetadata, List<Dictionary<string, AttributeMetadata>> linkedEntitiesMetadataList, List<string> layoutColumnLogicalNames)
        {
            GlobalContext.LogEntry();
            List<string> orderedDisplayLabels = new List<string>();

            foreach (var logicalName in layoutColumnLogicalNames)
            {
                // Find matching display label from metadata
                var metadata = mainEntityMetadata.Values
                    .FirstOrDefault(m => m.LogicalName == logicalName);

                if (metadata != null)
                {
                    string label = metadata.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;

                    if (!string.IsNullOrWhiteSpace(label))
                        orderedDisplayLabels.Add(label);
                }
                else
                {
                    // Try linked entities
                    foreach (var linkedMetadata in linkedEntitiesMetadataList ?? new List<Dictionary<string, AttributeMetadata>>())
                    {
                        var linkedMatch = linkedMetadata.Values
                            .FirstOrDefault(m => m.LogicalName == logicalName);

                        if (linkedMatch != null)
                        {
                            string label = linkedMatch.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
                            if (!string.IsNullOrWhiteSpace(label))
                                orderedDisplayLabels.Add(label);

                            break;
                        }
                    }
                }
            }

            return orderedDisplayLabels;
        }


        private KeyValuePair<string, object> MapRecordColumnData(Dictionary<string, AttributeMetadata> entityMetadata,
                                                                 string attributeDisplayName,
                                                                 string attributeName,
                                                                 object attributeValue)
        {
            object columnValue = null;

            switch (attributeValue)
            {
                case EntityReference entityRef:
                    {
                        columnValue = entityRef;
                        break;
                    }
                case OptionSetValue optionSet:
                    {
                        string optionSetLabel = GetOptionSetLabel(entityMetadata, attributeName, optionSet.Value);
                        columnValue = string.IsNullOrWhiteSpace(optionSetLabel) ? string.Empty : optionSetLabel;
                        break;
                    }
                case IEnumerable<OptionSetValue> optionSetValues:
                    {
                        var values = optionSetValues.Select(opt => opt.Value.ToString()).Where(val => !string.IsNullOrWhiteSpace(val)).ToArray();
                        var labels = optionSetValues.Select(opt => GetOptionSetLabel(entityMetadata, attributeName, opt.Value)).Where(label => !string.IsNullOrWhiteSpace(label)).ToArray();

                        columnValue = string.Join(",", labels);
                        break;
                    }
                case Money money:
                    {
                        columnValue = money.Value;
                        break;
                    }
                case bool booleanValue:
                    {
                        columnValue = booleanValue ? "כן" : "לא";
                        break;
                    }
                case DateTime dateTimeValue:
                    {
                        columnValue = dateTimeValue.ToString("M-d-yyyy h:mm:ss tt");
                        break;
                    }
                default:
                    {
                        columnValue = attributeValue ?? string.Empty;
                        break;
                    }
            }

            KeyValuePair<string, object> recordColumnData = new KeyValuePair<string, object>(attributeDisplayName, columnValue);
            return recordColumnData;
        }

        private List<Dictionary<string, object>> GetEntityProperties(EntityCollection entityRecords, Dictionary<string, AttributeMetadata> entityMetadata, EntityInfo info)
        {
            this.GlobalContext.LogEntry();
            List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();
            foreach (var entity in entityRecords.Entities)
            {
                var recordData = new Dictionary<string, object>();

                foreach (var attributeMetadata in entityMetadata.Values)
                {
                    string attributeName = attributeMetadata.LogicalName;
                    object attributeValue = entity.Attributes.ContainsKey(attributeName) ? entity.Attributes[attributeName] : null;

                    switch (attributeValue)
                    {
                        case EntityReference entityRef:
                            {
                                recordData[attributeName] = entityRef.Id.ToString();
                                recordData[$"{attributeName}name"] = entityRef.Name ?? string.Empty;
                                break;
                            }
                        case OptionSetValue optionSet:
                            {
                                string optionSetLabel = GetOptionSetLabel(entityMetadata, attributeName, optionSet.Value);
                                recordData[attributeName] = optionSet.Value;
                                recordData[$"{attributeName}name"] = string.IsNullOrWhiteSpace(optionSetLabel) ? string.Empty : optionSetLabel;
                                break;
                            }
                        case IEnumerable<OptionSetValue> optionSetValues:
                            {
                                var values = optionSetValues.Select(opt => opt.Value.ToString()).Where(val => !string.IsNullOrWhiteSpace(val)).ToArray();
                                var labels = optionSetValues.Select(opt => GetOptionSetLabel(entityMetadata, attributeName, opt.Value)).Where(label => !string.IsNullOrWhiteSpace(label)).ToArray();

                                recordData[attributeName] = string.Join(",", values);
                                recordData[$"{attributeName}name"] = string.Join(",", labels);
                                break;
                            }
                        case Money money:
                            {
                                recordData[attributeName] = money.Value;
                                break;
                            }
                        case bool booleanValue:
                            {
                                recordData[attributeName] = booleanValue;
                                recordData[$"{attributeName}name"] = booleanValue ? "כן" : "לא";
                                break;
                            }
                        case DateTime dateTimeValue:
                            {
                                recordData[attributeName] = dateTimeValue.ToString("M-d-yyyy h:mm:ss tt");
                                break;
                            }
                        default:
                            {
                                if (!recordData.ContainsKey(attributeName))
                                    recordData[attributeName] = attributeValue ?? string.Empty;
                                break;
                            }
                    }
                }

                foreach (var field in info.Columns)
                {
                    if (!recordData.ContainsKey(field))
                    {
                        recordData[field] = string.Empty;
                    }
                }

                tableData.Add(recordData);
            }
            return tableData;
        }

        private string GetOptionSetLabel(Dictionary<string, AttributeMetadata> entityMetadata, string attributeName, int optionSetValue)
        {
            if (!entityMetadata.ContainsKey(attributeName)) return string.Empty;

            var metadata = entityMetadata[attributeName];
            switch (metadata)
            {
                case PicklistAttributeMetadata picklistMetadata:
                    return picklistMetadata.OptionSet.Options
                        .FirstOrDefault(option => option.Value == optionSetValue)?.Label.UserLocalizedLabel.Label ?? string.Empty;

                case StateAttributeMetadata stateMetadata:
                    return stateMetadata.OptionSet.Options
                        .FirstOrDefault(option => option.Value == optionSetValue)?.Label.UserLocalizedLabel.Label ?? string.Empty;

                case StatusAttributeMetadata statusMetadata:
                    return statusMetadata.OptionSet.Options
                        .FirstOrDefault(option => option.Value == optionSetValue)?.Label.UserLocalizedLabel.Label ?? string.Empty;

                case MultiSelectPicklistAttributeMetadata multiSelectMetadata:
                    // Multi-select picklist
                    return multiSelectMetadata.OptionSet.Options
                        .FirstOrDefault(option => option.Value == optionSetValue)?.Label.UserLocalizedLabel?.Label ?? string.Empty;

                default:
                    return string.Empty;
            }
        }

        private string[] GenerateHeadersForCSVFile(Dictionary<string, AttributeMetadata> entityMetadata, EntityInfo entityInfo)
        {
            List<string> headers = new List<string>();

            foreach (var fieldMetadata in entityMetadata.Values)
            {
                string fieldName = fieldMetadata.LogicalName;
                headers.Add(fieldName);
            }

            foreach (var item in entityInfo.Columns)
            {
                if (!headers.Contains(item))
                {
                    headers.Add(item);
                }
            }

            return headers.ToArray();
        }

        public List<Dictionary<string, object>> GetEntityDataWithColumnLabels(string entityLogicalName,
            Dictionary<string, string> linkedEntityNameAliasPairs, string fetchXML, string layoutXML = null)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, AttributeMetadata> entityMetadata = this.GetEntityMetadata(entityLogicalName);
            List<Dictionary<string, AttributeMetadata>> linkedEntitiesMetadata = null;
            if (linkedEntityNameAliasPairs?.Count > 0)
            {
                List<string> linkedEntityNames = linkedEntityNameAliasPairs.Select(linkedEntity => linkedEntity.Key).ToList();
                linkedEntitiesMetadata = GetMultipleEntitiesMetadata(linkedEntityNames);
            }

            EntityCollection retrievedEntityRecords = Fetch(new FetchExpression(fetchXML));

            List<Dictionary<string, object>> entityProperties = null;
            if (retrievedEntityRecords?.Entities?.Count > 0)
            {
                entityProperties = MapEntityPropertiesWithLabels(retrievedEntityRecords, entityMetadata, linkedEntitiesMetadata, linkedEntityNameAliasPairs, layoutXML);
            }

            return entityProperties;
        }

        private List<Dictionary<string, object>> MapEntityPropertiesWithLabels(EntityCollection entityRecords,
                                                                       Dictionary<string, AttributeMetadata> mainEntityMetadata,
                                                                       List<Dictionary<string, AttributeMetadata>> linkedEntitiesMetadataList,
                                                                       Dictionary<string, string> linkedEntityNameAliasPairs, string layoutXML = null)
        {
            this.GlobalContext.LogEntry();
            List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();
            Dictionary<string, object> emptyRecordDataForColumnNamesOrder = new Dictionary<string, object>();

            foreach (Entity entity in entityRecords.Entities)
            {
                Dictionary<string, object> recordData = new Dictionary<string, object>();

                foreach (var entityAttribute in entity.Attributes)
                {
                    string[] linkedEntityAttributeAliasSplit = entityAttribute.Key.Split('.');
                    int aliasIndex = 0, columnIndex = 1;

                    AttributeMetadata attributeMetadata = null;
                    string linkedEntityDisplayName = string.Empty;
                    Dictionary<string, AttributeMetadata> linkedEntityMetadata = null;

                    if (linkedEntityAttributeAliasSplit.Length > 1 && linkedEntitiesMetadataList?.Count > 0)
                    {
                        string linkedEntityLogicalName = linkedEntityNameAliasPairs.First(pair => pair.Value == linkedEntityAttributeAliasSplit[aliasIndex]).Key;
                        linkedEntityMetadata = linkedEntitiesMetadataList.First(linkedMetadata =>
                                                                         linkedMetadata.Values.Any(linkedValue =>
                                                                                                   linkedValue.EntityLogicalName == linkedEntityLogicalName));

                        attributeMetadata = linkedEntityMetadata.First(attrMetadata => attrMetadata.Key == linkedEntityAttributeAliasSplit[columnIndex]).Value;

                        linkedEntityDisplayName = "(" + mainEntityMetadata.First(entMetadata => entMetadata.Key == linkedEntityLogicalName + "id")
                                                                          .Value.DisplayName.LocalizedLabels
                                                                          .First().Label + ")";
                    }
                    else
                    {
                        attributeMetadata = mainEntityMetadata.Values.First(metadata => metadata.LogicalName == entityAttribute.Key);
                    }

                    string attributeDisplayName = $"{linkedEntityDisplayName}{attributeMetadata.DisplayName.LocalizedLabels.FirstOrDefault()?.Label}";
                    if (!string.IsNullOrWhiteSpace(attributeDisplayName))
                    {
                        emptyRecordDataForColumnNamesOrder[attributeDisplayName] = string.Empty;
                        string attributeName = attributeMetadata.LogicalName;
                        bool isLinkedEntityValue = linkedEntityAttributeAliasSplit?.Length > 1 && entityAttribute.Value != null;

                        object attributeValue = isLinkedEntityValue ? ((AliasedValue)entityAttribute.Value).Value
                                                                    : entity.Attributes.ContainsKey(attributeName) ? entity.Attributes[attributeName]
                                                                    : null;

                        if (attributeMetadata.IsPrimaryName.HasValue && attributeMetadata.IsPrimaryName.Value)
                        {
                            attributeValue = new EntityReference(entity.LogicalName, entity.Id) { Name = entity.Attributes[attributeName].ToString() };
                        }

                        Dictionary<string, AttributeMetadata> entityMetadataToMap = isLinkedEntityValue ? linkedEntityMetadata
                                                                                                        : mainEntityMetadata;

                        KeyValuePair<string, object> recordColumnData = MapRecordColumnData(entityMetadataToMap, attributeDisplayName, attributeName, attributeValue);
                        recordData[recordColumnData.Key] = recordColumnData.Value;
                    }
                }

                if (recordData.Count > 0)
                {
                    tableData.Add(recordData);
                }
            }

            List<Dictionary<string, object>> orderedTableData;
            if (layoutXML != null)
            {
                orderedTableData = ReorderTableDataValuesByLayoutXML(tableData, mainEntityMetadata, linkedEntitiesMetadataList, layoutXML);
            }
            else
            {
                orderedTableData = ReorderTableDataValuesByRecordData(tableData, emptyRecordDataForColumnNamesOrder);
            }

            return orderedTableData;
        }

        private Dictionary<string, AttributeMetadata> GetEntityMetadata(string entityName)
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityName
            };

            var response = (RetrieveEntityResponse)GlobalContext.OrganizationService.Execute(request);
            return response.EntityMetadata.Attributes
                .ToDictionary(attr => attr.LogicalName, attr => attr);
        }

        private List<Dictionary<string, AttributeMetadata>> GetMultipleEntitiesMetadata(List<string> entitiesNames)
        {
            this.GlobalContext.LogEntry();
            OrganizationRequestCollection organizationRequestCollection = new OrganizationRequestCollection();
            foreach (string entityName in entitiesNames)
            {
                RetrieveEntityRequest request = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Attributes,
                    LogicalName = entityName
                };
                organizationRequestCollection.Add(request);
            }

            ExecuteMultipleResponse multipleResponse = ExecuteMultipleRequests(organizationRequestCollection, false);
            List<Dictionary<string, AttributeMetadata>> retrievedEntitiesMetadata = multipleResponse?.Responses
                                        .Select(response =>
                                            ((RetrieveEntityResponse)response.Response).EntityMetadata.Attributes
                                                .ToDictionary(attr => attr.LogicalName, attr => attr))
                                        .ToList();

            return retrievedEntitiesMetadata;
        }
    }
}
