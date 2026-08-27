using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.Mapper;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class CommonDAL : CrmExternalBaseDAL<ApiEntity>
    {
        public CommonDAL(GlobalContext globalContext, string entityLogicalName) : base(globalContext, entityLogicalName) { }

        public string GetParsedPDFMessage(string jsonData, ApiEntity entryPoint)
        {
            this.GlobalContext.LogEntry();
            PDFReportValueResolver pdfFReportValueResolver = new PDFReportValueResolver();
            return this.GetParsedMessage(jsonData, entryPoint, pdfFReportValueResolver);
        }

        public Dictionary<string, string> GetValuesToCompare(Guid id, Dictionary<string, string> attributesToCompare)
        {
            this.GlobalContext.LogEntry();
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            Entity retrievedEntity = base.GetAsEntity(id, attributesToCompare.Keys.ToArray());
            var entityMetadata = this.GlobalContext.CacheManager.GetCachedItem($"Metadata({this.entityLogicalName})", () =>
            {
                return base.GetEntityMetadata();
            }, 10);

            foreach (var attribute in retrievedEntity.Attributes)
            {
                var attributeMetadata = entityMetadata.Attributes.Where(a => a.LogicalName == attribute.Key).FirstOrDefault();
                if (attributeMetadata != null && attributesToCompare.ContainsKey(attribute.Key))
                {
                    string value = this.ConvertValue(attribute.Value, attributeMetadata);
                    pairs.Add(attributesToCompare[attribute.Key], value);
                }
            }
            return pairs;
        }

        public void UpsertDynamicList(List<dynamic> records, CrmEntityBuilderConfiguration configuration, ETLCounter counter, List<ETLWarning> warnings)
        {
            this.GlobalContext.LogEntry();

            configuration.DataFlowName = $"{RequestType.Upsert} {this.entityLogicalName}";
            counter.Pipeline = records.Count;
            counter.Errors = 0;
            counter.DataFlowName = configuration.DataFlowName;

            List<Entity> entities = this.GenerateEntitiesFromDynamicList(records, configuration, counter, warnings);
            if (configuration.ExecuteMultipleRequests != null && configuration.ExecuteMultipleRequests.Value)
            {
                this.ExecuteUpsertMultipleRequestsInChunks(entities, configuration, counter, warnings);
            }
            else
            {
                this.ExecuteUpsertRequests(entities, configuration, counter, warnings);
            }
        }

        public Entity GetEntityViewByName(string viewName)
        {
            GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression("savedquery")
            {
                NoLock = true,
                TopCount = 1,
                ColumnSet = new ColumnSet("fetchxml", "layoutxml"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, viewName)
                    }
                }
            };

            Entity retrievedView = GetFirstOrDefaultAsEntity(query);
            return retrievedView;
        }

        public Entity GetEntityUserViewByName(string viewName)
        {
            GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression("userquery")
            {
                NoLock = true,
                TopCount = 1,
                ColumnSet = new ColumnSet("fetchxml", "layoutxml", "ownerid"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, viewName)
                    }
                }
            };

            Entity retrievedView = GetFirstOrDefaultAsEntity(query);
            return retrievedView;
        }

        private void ExecuteUpsertMultipleRequestsInChunks(List<Entity> entities, CrmEntityBuilderConfiguration configuration, ETLCounter counter, List<ETLWarning> warnings)
        {
            this.GlobalContext.LogEntry(configuration.DataFlowName);

            var multipleResponses = ExecuteMultipleRequestsInChunks(entities, RequestType.Upsert, configuration.ChunkSize ?? 50);
            foreach (var item in multipleResponses)
            {
                if (item.IsFaulted)
                {
                    foreach (var response in item.Responses)
                    {
                        var entity = entities[response.RequestIndex];
                        if (response?.Fault != null)
                        {
                            counter.Errors++;
                            warnings.Add(new ETLWarning
                            {
                                WarningMessage = response?.Fault.Message,
                                ErrorCode = response.Fault.ErrorCode,
                                WarningLevel = (int)MessageLevel.Critical,
                                DataFlowName = configuration.DataFlowName,
                                RecordKey = string.Join(",", configuration.RecordAlternateKeyAttributes.Select(k => entity.KeyAttributes[k]).ToList())
                            });
                        }
                    }
                }
            }
        }

        private List<Entity> GenerateEntitiesFromDynamicList(List<dynamic> records, CrmEntityBuilderConfiguration configuration, ETLCounter counter, List<ETLWarning> warnings)
        {
            List<Entity> entities = new List<Entity>();
            DynamicCrmMapper dynamicCrmMapper = new DynamicCrmMapper(base.GetEntityMetadata(), configuration);
            foreach (var record in records)
            {
                try
                {
                    entities.Add(this.MappDynamicToEntity(record, dynamicCrmMapper));
                }
                catch (Exception ex)
                {
                    var keyValuePairs = (IDictionary<string, object>)record;
                    string recordKey = string.Join(",", configuration.RecordAlternateKeyAttributes?
                        .Select(k => keyValuePairs[k]?.ToString()).ToList());
                    this.HandleException(ex, counter, warnings, configuration, recordKey);
                }
            }
            return entities;
        }

        public void ExecuteUpsertRequests(List<Entity> entities, CrmEntityBuilderConfiguration configuration, ETLCounter counter, List<ETLWarning> warnings)
        {
            this.GlobalContext.LogEntry(configuration.DataFlowName);
            counter.Created = 0;
            counter.Updated = 0;

            _ = Parallel.For(0, entities.Count, new ParallelOptions() { MaxDegreeOfParallelism = configuration.ThreadsCount ?? 20 }, (i) =>
            {
                Entity entity = entities[i];
                try
                {
                    UpsertResponse upsertResponse = Upsert(entity);
                    if (upsertResponse.RecordCreated)
                    {
                        counter.Created++;
                    }
                    else
                    {
                        counter.Updated++;
                    }
                }
                catch (Exception ex)
                {
                    this.GlobalContext.Log.Info(entity.SerializeAttributes());
                    string recordKey = string.Join(",", configuration.RecordAlternateKeyAttributes?
                        .Select(k => entity.KeyAttributes[k]).ToList());
                    this.HandleException(ex, counter, warnings, configuration, recordKey);
                }
            });
        }

        private void HandleException(Exception ex, ETLCounter counter, List<ETLWarning> warnings, CrmEntityBuilderConfiguration configuration, string recordKey = null)
        {
            this.GlobalContext.Log.Error(ex.ToString());
            counter.Errors++;
            warnings.Add(new ETLWarning
            {
                WarningMessage = ex.Message,
                DataFlowName = configuration.DataFlowName,
                ErrorCode = ex.InnerException?.HResult ?? ex.HResult,
                WarningLevel = (int)MessageLevel.Critical,
                RecordKey = recordKey
            });
        }

        private string ConvertValue(object value, AttributeMetadata attributeMetadata)
        {
            string valueStr = string.Empty;
            if (value != null)
            {
                AttributeTypeCode attributeTypeCode = (AttributeTypeCode)attributeMetadata.AttributeType;
                switch (attributeTypeCode)
                {
                    case AttributeTypeCode.Boolean:
                    case AttributeTypeCode.Integer:
                        {
                            valueStr = ((int)value).ToString();
                            break;
                        }
                    case AttributeTypeCode.DateTime:
                        {
                            valueStr = ((DateTime)value).ToString("dd/MM/yyyy");
                            break;
                        }
                    case AttributeTypeCode.String:
                        {
                            valueStr = value.ToString();
                            break;
                        }
                    case AttributeTypeCode.Picklist:
                        {
                            valueStr = ((OptionSetValue)value).Value.ToString();
                            break;
                        }
                    default:
                        break;
                }
            }
            return valueStr;
        }
    }
}
