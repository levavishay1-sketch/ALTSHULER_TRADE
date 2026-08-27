using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Core.Interfaces;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.Mapper;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class CrmExternalBaseDAL<TApiEntity> : GlobalContext, IRepository<TApiEntity>
         where TApiEntity : ApiEntityBase
    {
        protected readonly string entityLogicalName;
        protected GlobalContext GlobalContext { get; private set; }
        protected CrmEntityMapper<TApiEntity> crmEntityMapper = new CrmEntityMapper<TApiEntity>();
        protected Retry retry;
        private readonly object lockObject = new object();

        public CrmExternalBaseDAL(GlobalContext globalContext, string entityLogicalName) : base(globalContext)
        {
            this.GlobalContext = globalContext;
            this.entityLogicalName = entityLogicalName;
            this.retry = new Retry(this.GlobalContext.Log);
        }

        public virtual Guid Create(TApiEntity entity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            Entity crmEntity = MappApiEntityToCrmEntity(entity, true);
            if (crmEntity.KeyAttributes != null)
            {
                crmEntity.KeyAttributes = null;
            }
            CreateResponse createResponse = (CreateResponse)this.Execute(new CreateRequest { Target = crmEntity });
            return createResponse.id;
        }

        public virtual Guid Create(TApiEntity entity, bool suppressDuplicateDetection = false)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            Entity crmEntity = MappApiEntityToCrmEntity(entity, true);
            if (crmEntity.KeyAttributes != null)
            {
                crmEntity.KeyAttributes = null;
            }
            CreateRequest createRequest = new CreateRequest { Target = crmEntity };
            if (suppressDuplicateDetection)
            {
                createRequest.Parameters.Add("SuppressDuplicateDetection", true);
            }
            CreateResponse createResponse = (CreateResponse)this.Execute(createRequest);
            return createResponse.id;
        }

        protected ExecuteTransactionResponse Create(IEnumerable<TApiEntity> entities)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var requests = this.GenerateRequestsCollection(entities, RequestType.Create);
            return this.ExecuteTransactionRequests(requests);
        }

        protected Guid Create(Entity entity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            CreateResponse createResponse = (CreateResponse)this.Execute(new CreateRequest { Target = entity });
            return createResponse.id;
        }

        public void Update(TApiEntity entity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            Entity crmEntity = MappApiEntityToCrmEntity(entity);
            this.Execute(new UpdateRequest { Target = crmEntity });
        }

        public UpsertResponse Upsert(TApiEntity entity)
        {
            Entity crmEntity = MappApiEntityToCrmEntity(entity);
            return this.Upsert(crmEntity);
        }

        public UpsertResponse Upsert(Entity entity)
        {
            var request = new UpsertRequest()
            {
                Target = entity
            };
            return (UpsertResponse)Execute(request);
        }

        public void UpdateOnlyDeltaModifiedProperties(TApiEntity entity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            Entity crmEntity = MappOnlyDeltaModifiedPropertiesOnUpdate(entity);
            if (crmEntity == null)
            {
                this.GlobalContext.Log.Warning("No delta properties exist");
                return;
            }

            this.Execute(new UpdateRequest { Target = crmEntity });
        }

        public void Delete(TApiEntity entity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            this.Execute(new DeleteRequest { Target = new EntityReference(entity.LogicalName, entity.Id.Value) });
        }

        public ExecuteTransactionResponse Delete(IEnumerable<TApiEntity> entities)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var requests = this.GenerateRequestsCollection(entities, RequestType.Delete);
            return this.ExecuteTransactionRequests(requests);
        }

        public TApiEntity Get(Guid id, string[] columns)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var columnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true);
            RetrieveResponse retrieveResponse = (RetrieveResponse)this.Execute(new RetrieveRequest { Target = new EntityReference(this.entityLogicalName, id), ColumnSet = columnSet });
            return MappCrmEntityToApiEntity(retrieveResponse.Entity);
        }

        public Entity GetAsEntity(Guid id, string[] columns)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var columnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true);
            RetrieveResponse retrieveResponse = (RetrieveResponse)this.Execute(new RetrieveRequest { Target = new EntityReference(this.entityLogicalName, id), ColumnSet = columnSet });
            return retrieveResponse.Entity;
        }

        public TApiEntity GetFirstOrDefaultByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return this.GetByAttribute<T1>(attributeName, attributeValue, columns, noLock)?.FirstOrDefault();
        }

        public TApiEntity GetFirstActivetOrDefaultByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return this.GetActiveByAttribute<T1>(attributeName, attributeValue, columns, noLock)?.FirstOrDefault();
        }

        public List<TApiEntity> GetActiveByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            QueryExpression query = new QueryExpression()
            {
                EntityName = this.entityLogicalName,
                ColumnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true),
                NoLock = noLock,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression(attributeName, ConditionOperator.Equal, attributeValue),
                        (entityLogicalName == "systemuser" ?  new ConditionExpression("isdisabled", ConditionOperator.Equal, false)
                        : new ConditionExpression("statecode", ConditionOperator.Equal, 0))
                    }
                }
            };

            return this.GetMultiple(query);
        }

        public List<TApiEntity> GetByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            QueryExpression query = new QueryExpression()
            {
                EntityName = this.entityLogicalName,
                ColumnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true),
                NoLock = noLock,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression(attributeName, ConditionOperator.Equal, attributeValue)
                    }
                }
            };


            return this.GetMultiple(query);
        }

        public virtual ActionResult ExecuteMultipleRequestsInChunks(List<TApiEntity> apiEntityList, RequestType crmRequestType, int chunksAmount = 10, bool logErrors = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            List<Entity> entityList = new List<Entity>();
            ActionResult apiActionResult = new ActionResult();
            var errorList = new List<string>();
            foreach (var apiEntity in apiEntityList)
            {
                entityList.Add(this.MappApiEntityToCrmEntity(apiEntity));
            }
            var responsesList = this.ExecuteMultipleRequestsInChunks(entityList, crmRequestType, chunksAmount);

            foreach (var response in responsesList) // check chunks responses
            {
                var responseErrorList = this.ExtractExecuteMultipleRequestsFaults(response);
                if (responseErrorList != null && responseErrorList.Count > 0)
                {
                    errorList.AddRange(responseErrorList);
                }
            }

            if (errorList.Count > 0)
            {
                string errorMessage = $"ExecuteMultipleRequestsInChunks Errors:{string.Join($"{Environment.NewLine},", errorList)}";
                apiActionResult.SetToFailedActionResult(errorMessage);
                apiActionResult.ReturnObject = errorList;
                if (logErrors)
                {
                    this.GlobalContext.Log.Error(errorMessage);
                }
            }
            else
            {
                apiActionResult.ReturnObject = $"Records Count to {crmRequestType}: {apiEntityList.Count}";
            }

            return apiActionResult;
        }

        public TApiEntity GetAllMapperableProperties(ApiEntity apiEntity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return Get(new Guid(apiEntity.Id.ToString()), this.GetMapperableProperties());
        }

        protected ExecuteTransactionResponse Update(IEnumerable<TApiEntity> entities)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var requests = this.GenerateRequestsCollection(entities, RequestType.Update);
            return this.ExecuteTransactionRequests(requests);
        }

        protected List<TApiEntity> GetMultipleWithPaging(QueryExpression query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return this.GetEntityCollectionWithPaging(query)?.Entities?.Select(x => MappCrmEntityToApiEntity(x))?.ToList();
        }

        protected EntityCollection GetEntityCollectionWithPaging(QueryExpression query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            EntityCollection finalEntityCollection = new EntityCollection();

            query.NoLock = true;
            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 5000;
            query.PageInfo.PageNumber = 1;
            query.PageInfo.PagingCookie = null;

            while (true)
            {
                RetrieveMultipleResponse retrieveMultipleResponse = (RetrieveMultipleResponse)this.Execute(new RetrieveMultipleRequest { Query = query });

                EntityCollection entityCollection = retrieveMultipleResponse.EntityCollection;
                if (entityCollection?.Entities?.Count > 0)
                {
                    finalEntityCollection.Entities.AddRange(entityCollection.Entities);
                }

                if (entityCollection.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = entityCollection.PagingCookie;
                }
                else
                {
                    break;
                }
            }
            return finalEntityCollection?.Entities?.Count > 0 ? finalEntityCollection : null;
        }

        protected string[] GetMapperableProperties(bool validToMappFromCrm = true, bool validToMappToCrm = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            List<string> mapperableProperties = new List<string>();

            var props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is CrmEntityMapperAttribute attribute)
                    {
                        if (attribute.MappFromCrm == validToMappFromCrm && attribute.MappToCrm == validToMappToCrm)
                        {
                            mapperableProperties.Add(attribute.CrmPropertyName);
                        }
                    }
                }
            }
            return mapperableProperties.ToArray();
        }

        protected Entity GetFirstOrDefaultAsEntity(QueryBase query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return this.GetMultipleAsEntity(query)?.Entities?.FirstOrDefault();
        }

        protected EntityCollection GetMultipleAsEntity(QueryBase query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            RetrieveMultipleResponse retrieveMultipleResponse = (RetrieveMultipleResponse)this.Execute(new RetrieveMultipleRequest { Query = query });
            return retrieveMultipleResponse?.EntityCollection;
        }

        protected virtual Entity MappApiEntityToCrmEntity(TApiEntity apiEntity, bool ignoreNull = false)
        {
            return crmEntityMapper.MappApiEntityToCrmEntity(apiEntity, ignoreNull);
        }

        protected virtual TApiEntity MappCrmEntityToApiEntity(Entity crmEntity) // for manual mapping you can override this method
        {
            return crmEntityMapper.MappCrmEntityToApiEntity(crmEntity);
        }

        protected OrganizationResponse Execute(OrganizationRequest request)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            request.RequestId = this.GlobalContext.RequestId;
            var organizationService = this.GlobalContext.OrganizationService;
            return organizationService.Execute(request);
        }

        protected EntityCollection Fetch(QueryBase query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            RetrieveMultipleResponse retrieveMultipleResponse = (RetrieveMultipleResponse)this.Execute(new RetrieveMultipleRequest { Query = query });
            return retrieveMultipleResponse?.EntityCollection;
        }

        protected List<TApiEntity> GetMultiple(QueryBase query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            RetrieveMultipleResponse retrieveMultipleResponse = (RetrieveMultipleResponse)this.Execute(new RetrieveMultipleRequest { Query = query });
            return retrieveMultipleResponse?.EntityCollection?.Entities?.Select(x => MappCrmEntityToApiEntity(x))?.ToList();
        }

        protected List<Entity> RetrieveMultiple(QueryBase query)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            RetrieveMultipleResponse retrieveMultipleResponse = (RetrieveMultipleResponse)this.Execute(new RetrieveMultipleRequest { Query = query });
            return retrieveMultipleResponse?.EntityCollection?.Entities?.ToList();
        }

        protected ExecuteMultipleResponse ExecuteMultipleRequests(OrganizationRequestCollection collectionRequests, bool continueOnError = true, bool returnResponses = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            var request = new ExecuteMultipleRequest();

            request.Requests = collectionRequests;
            request.Settings = new ExecuteMultipleSettings
            {
                ContinueOnError = continueOnError,
                ReturnResponses = returnResponses
            };

            ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)this.Execute(request);
            this.ExtractExecuteMultipleRequestsFaults(multipleResponse);
            return multipleResponse;
        }

        protected List<string> ExecuteMultipleRequestss(OrganizationRequestCollection collectionRequests)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            var request = new ExecuteMultipleRequest
            {
                Requests = collectionRequests,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                }
            };
            return this.ExtractExecuteMultipleRequestsFaults((ExecuteMultipleResponse)this.Execute(request));
        }

        protected List<string> ExtractExecuteMultipleRequestsFaults(ExecuteMultipleResponse MultipleResponse)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            List<string> errorList = null;
            if (MultipleResponse != null)
            {
                errorList = new List<string>();
                if (MultipleResponse.IsFaulted)// check for errors
                {
                    foreach (var response in MultipleResponse.Responses)
                    {
                        if (response?.Fault != null)
                        {
                            string stackTraceErrorMessage = string.Empty;
                            var exceptionDetails = response?.Fault?.ErrorDetails?.FirstOrDefault(t => t.Key == "ApiOriginalExceptionKey");
                            if (exceptionDetails != null && exceptionDetails.HasValue && exceptionDetails?.Value != null)
                            {
                                stackTraceErrorMessage = $",stack trace : {exceptionDetails.Value.ToString()} ";
                            }

                            this.GlobalContext.Log.Error($"{response?.Fault?.ToString()}{stackTraceErrorMessage}");
                            errorList.Add(response?.Fault?.ToString());
                        }
                    }
                }
            }
            return errorList;
        }

        protected OrganizationRequestCollection GenerateRequestsCollection(IEnumerable<TApiEntity> apiEntityCollection, RequestType crmRequestType)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            OrganizationRequestCollection requestsCollection = null;
            if (apiEntityCollection != null)
            {
                requestsCollection = new OrganizationRequestCollection();
                foreach (var apiEntityRecord in apiEntityCollection)
                {
                    OrganizationRequest request = this.GenerateRequest(apiEntityRecord, crmRequestType);
                    requestsCollection.Add(request);
                }
            }

            return requestsCollection;
        }

        protected OrganizationRequest GenerateRequest(TApiEntity apiEntityRecord, RequestType crmRequestType)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            string requestName = GetRequestNameByType(crmRequestType);
            OrganizationRequest request = new OrganizationRequest(requestName);
            if (crmRequestType == RequestType.Delete)
            {
                request["Target"] = new EntityReference(apiEntityRecord.LogicalName, apiEntityRecord.Id.Value);
            }
            else
            {
                var recordToOperate = crmRequestType == RequestType.Create ? MappApiEntityToCrmEntity(apiEntityRecord, true) : MappApiEntityToCrmEntity(apiEntityRecord);
                request["Target"] = recordToOperate;
            }

            return request;
        }

        protected ExecuteTransactionResponse ExecuteTransactionRequests(OrganizationRequestCollection collectionRequests, bool returnResponses = true)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var request = new ExecuteTransactionRequest
            {
                Requests = collectionRequests,
                ReturnResponses = returnResponses,
            };

            return (ExecuteTransactionResponse)this.Execute(request);
        }

        protected AssociateResponse ExecuteAssociateRequest(EntityReference primeryEntityRef, EntityReferenceCollection entityRefCollection, string relationshipName)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            AssociateRequest request = new AssociateRequest();
            request.Target = primeryEntityRef;
            request.RelatedEntities = entityRefCollection;
            request.Relationship = new Relationship(relationshipName);
            return (AssociateResponse)this.Execute(request);
        }

        protected List<ExecuteMultipleResponse> ExecuteMultipleRequestsInChunks<TEntity>(List<TEntity> entityList, RequestType crmRequestType, int chunksAmount = 10) where TEntity : Entity
        {
            var listOfchunkedList = entityList.ToChunks(chunksAmount);
            ConcurrentBag<ExecuteMultipleResponse> responses = new ConcurrentBag<ExecuteMultipleResponse>();
            Parallel.ForEach(listOfchunkedList, (chunkedList) =>
            {
                OrganizationRequestCollection requestsCollection = new OrganizationRequestCollection();

                foreach (var entity in chunkedList)
                {
                    string requestName = GetRequestNameByType(crmRequestType);

                    OrganizationRequest request = new OrganizationRequest(requestName);
                    if (crmRequestType == RequestType.Delete)
                    {
                        request["Target"] = entity.ToEntityReference();
                    }
                    else
                    {
                        request["Target"] = entity;
                    }

                    requestsCollection.Add(request);
                }

                responses.Add(this.ExecuteMultipleRequests(requestsCollection));
            });

            return responses.ToList();
        }



        protected ApiEntity GetApiEntityByEntityReference(Entity crmEntity, string attributeLogicalName, ApiEntity apiEntity)
        {
            this.GlobalContext.LogEntry($"Extract attribute- \"{attributeLogicalName}\" From: \"{crmEntity.GetType().Name}\" into: \"{apiEntity.GetType().Name}\" ");
            EntityReference entityReference = crmEntity.GetAttributeValue<EntityReference>(attributeLogicalName);
            if (entityReference == null)
            {
                return null;
            }
            apiEntity.Id = entityReference.Id;
            apiEntity.LogicalName = entityReference.LogicalName;
            if (!string.IsNullOrWhiteSpace(this.OrganizationUrl))
            {
                apiEntity.RecordUrl = $"{this.OrganizationUrl}/main.aspx?etn={apiEntity.LogicalName}&id={apiEntity.Id}&pagetype=entityrecord";
            }
            return apiEntity;
        }

        protected ExecuteMultipleResponse ExecuteMultipleRequestsWithRetry(OrganizationRequestCollection requestsCollection, int retryNumber = 3)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var allResponses = new ExecuteMultipleResponse();
            var responses = this.ExecuteMultipleRequests(requestsCollection);

            while (retryNumber > 0)
            {
                if (!responses.IsFaulted)
                {
                    break;
                }
                for (int i = 0; i < responses.Responses.Count; i++)
                {
                    if (responses.Responses[i].Fault == null)
                    {
                        requestsCollection.RemoveAt(responses.Responses[i].RequestIndex);
                    }
                    else
                    {
                        allResponses.Responses.AddRange(responses.Responses[i]);
                    }
                }
                responses = this.ExecuteMultipleRequests(requestsCollection);

                retryNumber--;
            }

            responses.Responses.AddRange(allResponses.Responses);
            return responses;
        }

        protected EntityMetadata GetEntityMetadata()
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return this.OrganizationService.GetEntityMetadata(entityLogicalName);
        }

        protected RetrieveMetadataChangesResponse GetEntityMetadataChanges(string entityLogicalName, params string[] properties)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var metadataFilterExpression = new MetadataFilterExpression(LogicalOperator.And);
            metadataFilterExpression.Conditions.Add(new MetadataConditionExpression("LogicalName ", MetadataConditionOperator.Equals, entityLogicalName));
            var metadataPropertiesExpression = new MetadataPropertiesExpression();
            if (properties != null && properties.Length > 0)
            {
                metadataPropertiesExpression.AllProperties = false;
                foreach (string property in properties)
                {
                    metadataPropertiesExpression.PropertyNames.Add(property);
                }
            }
            else
            {
                metadataPropertiesExpression.AllProperties = true;
            }

            var entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = metadataFilterExpression,
                Properties = metadataPropertiesExpression
            };

            var retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression
            };

            return (RetrieveMetadataChangesResponse)this.Execute(retrieveMetadataChangesRequest);
        }

        protected IEnumerable<T> ExecuteQuery<T>(QueryBase query) where T : Entity
        {
            this.GlobalContext.LogEntry();
            return (IEnumerable<T>)this.RetrieveMultiple(query);
        }

        private string GetRequestNameByType(RequestType crmRequestType)
        {
            if (!Enum.IsDefined(typeof(RequestType), crmRequestType))
            {
                throw new Exception(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidRequest));
            }
            return crmRequestType.ToString();
        }

        public OptionSetMetadata GetGlobalOptionSetMetaData(string optionSetName)
        {
            this.GlobalContext.LogEntry();

            RetrieveOptionSetRequest retrieveOptionSetRequest = new RetrieveOptionSetRequest
            {
                Name = optionSetName
            };
            RetrieveOptionSetResponse retrieveOptionSetResponse = (RetrieveOptionSetResponse)OrganizationService.Execute(retrieveOptionSetRequest);
            OptionSetMetadata retrievedOptionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;
            return retrievedOptionSetMetadata;
        }

        public virtual List<TApiEntity> GetMultipleByDateLastXDays(string attributeDateName, int daysRange, string[] columns = null)
        {
            this.GlobalContext.LogEntry();
            var columnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true);
            QueryExpression query = new QueryExpression
            {
                EntityName = entityLogicalName,
                ColumnSet = columnSet,
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = {
                        new ConditionExpression(attributeDateName, ConditionOperator.LastXDays, daysRange)
                    }
                },
            };

            query.NoLock = true;

            return this.GetMultiple(query);
        }

        protected string GetOptionSetLableForValue(string attributeLogicalName, int selectedValue)
        {
            this.GlobalContext.LogEntry();

            RetrieveAttributeResponse retrieveAttributeResponse = this.GetAttributeMetadata(attributeLogicalName);

            PicklistAttributeMetadata retrievedPicklistAttributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
            OptionMetadata[] optionMetadata = retrievedPicklistAttributeMetadata.OptionSet.Options.ToArray();

            return optionMetadata?.Where(o => o.Value == selectedValue).FirstOrDefault()?
                .Label.LocalizedLabels[0]?.Label?.ToString();
        }

        protected RetrieveAttributeResponse GetAttributeMetadata(string attributeLogicalName)
        {
            this.GlobalContext.LogEntry();

            var attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName,
                RetrieveAsIfPublished = true
            };

            var attributeResponse = (RetrieveAttributeResponse)this.OrganizationService.Execute(attributeRequest);
            return attributeResponse;
        }

        public string GetParsedMessage(string message, ApiEntity entryPoint, IEntityValueResolver entityValueResolver = null)
        {
            this.GlobalContext.LogEntry();

            if (!string.IsNullOrWhiteSpace(message))
            {
                Parser parser = new Parser(new ParserSettings()
                {
                    RegardingObjectId = entryPoint.Id.ToString(),
                    RegardingObjectEntityLogicalName = entryPoint.LogicalName,
                    MessageToParse = message,
                    EntityValueResolver = entityValueResolver,
                    ValueToParseInEmptyOrInvalidPlaceHolders = " "
                });

                return parser.GetParsedMessage(ExecuteQuery<Entity>);
            }
            else
            {
                return string.Empty;
            }
        }

        private Entity MappOnlyDeltaModifiedPropertiesOnUpdate(TApiEntity entityToUpdate)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            if (entityToUpdate.Id != null)
            {
                return crmEntityMapper.MappOnlyDeltaModifiedPropertiesOnUpdateWithRetreive(entityToUpdate, this.Get);
            }
            else
            {
                return crmEntityMapper.MappOnlyDeltaModifiedPropertiesOnUpdate(entityToUpdate, (Func<string, object, string[], bool, List<TApiEntity>>)GetByAttribute);
            }
        }

        protected Entity MappDynamicToEntity(dynamic record, DynamicCrmMapper dynamicCrmMapper = null, EntityMetadata entityMetadata = null)
        {
            if (dynamicCrmMapper == null)
            {
                dynamicCrmMapper = new DynamicCrmMapper(entityMetadata ?? this.GetEntityMetadata());
            }
            return dynamicCrmMapper.ToEntity(record);
        }
    }
}
