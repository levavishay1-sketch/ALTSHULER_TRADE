using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm
{
    public abstract class CrmBaseDAL<TEntity> : GlobalContext, IRepository<TEntity> where TEntity : Entity
    {
        protected GlobalContext GlobalContext { get; private set; }

        protected string entityLogicalName;

        public CrmBaseDAL(GlobalContext globalContext, string entityLogicalName = null) : base(globalContext)
        {

            this.entityLogicalName = !string.IsNullOrWhiteSpace(entityLogicalName) ? entityLogicalName : typeof(TEntity).Name.ToLower();
            this.GlobalContext = globalContext;
        }

        public Guid Create(TEntity entity)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName ?? entity.LogicalName);
            return base.OrganizationService.Create(entity);
        }

        public ExecuteTransactionResponse Create(IEnumerable<TEntity> entities)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var requestes = new OrganizationRequestCollection();
            foreach (var entity in entities)
            {
                CreateRequest createRequest = new CreateRequest { Target = entity };
                requestes.Add(createRequest);
            }

            return this.ExecuteTransactionRequests(requestes);
        }

        public void Update(TEntity entity)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName ?? entity.LogicalName);
            OrganizationService.Update(entity);
        }

        public ExecuteTransactionResponse Update(IEnumerable<TEntity> entities)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var requestes = new OrganizationRequestCollection();
            foreach (var entity in entities)
            {
                UpdateRequest updateRequest = new UpdateRequest { Target = entity };
                requestes.Add(updateRequest);
            }

            return this.ExecuteTransactionRequests(requestes);
        }

        public UpsertResponse Upsert(TEntity entity)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName ?? entity.LogicalName);
            var request = new UpsertRequest()
            {
                Target = entity
            };
            return (UpsertResponse)Execute(request);
        }

        public List<UpsertResponse> Upsert(IEnumerable<TEntity> entites)
        {
            this.GlobalContext.LogEntry();
            List<UpsertResponse> responses = new List<UpsertResponse>();
            foreach (var entity in entites)
            {
                var request = new UpsertRequest()
                {
                    Target = entity
                };
                var response = (UpsertResponse)Execute(request);
                responses.Add(response);
            }
            return responses;
        }

        public void Delete(TEntity entity)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName ?? entity.LogicalName);
            OrganizationService.Delete(entity.LogicalName, entity.Id);
        }

        public ExecuteTransactionResponse Delete(IEnumerable<TEntity> entities)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var requestes = new OrganizationRequestCollection();
            foreach (var entity in entities)
            {
                EntityReference entityToDelete = entity.ToEntityReference();
                DeleteRequest deleteRequest = new DeleteRequest { Target = entityToDelete };
                requestes.Add(deleteRequest);
            }

            return this.ExecuteTransactionRequests(requestes);
        }

        public TEntity Get(Guid id, string[] columns)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var columnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true);
            return OrganizationService.Retrieve(this.entityLogicalName, id, columnSet).ToEntity<TEntity>();
        }

        public string GetPrimeryAttributeValue(EntityReference entityRef, string primeryAttributeName)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            string primeryAttributeValue = string.Empty;

            if (entityRef != null)
            {
                primeryAttributeValue = entityRef.Name;

                if (string.IsNullOrWhiteSpace(primeryAttributeValue))
                {
                    Entity retrievedEntity = this.Get(entityRef.Id, new[] { primeryAttributeName });
                    primeryAttributeValue = retrievedEntity.GetAttributeValue<string>(primeryAttributeName);
                }
            }

            return primeryAttributeValue;
        }

        public TEntity GetFirstOrDefaultByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            return this.GetByAttribute<T1>(attributeName, attributeValue, columns, noLock)?.FirstOrDefault();
        }

        public List<TEntity> GetByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
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

        public TEntity GetFirstActivetOrDefaultByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            return this.GetActiveByAttribute<T1>(attributeName, attributeValue, columns, noLock)?.FirstOrDefault();
        }

        public List<TEntity> GetActiveByAttribute<T1>(string attributeName, T1 attributeValue, string[] columns, bool noLock = true)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
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

        protected List<TEntity> GetMultipleWithPaging(QueryExpression query)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            EntityCollection finalEntityCollection = new EntityCollection();

            query.NoLock = true;
            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 5000;
            query.PageInfo.PageNumber = 1;
            query.PageInfo.PagingCookie = null;

            while (true)
            {
                EntityCollection entityCollection = OrganizationService.RetrieveMultiple(query);
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
            return finalEntityCollection?.Entities?.Select(item => item.ToEntity<TEntity>())?.ToList<TEntity>();
        }

        protected TEntity GetFirstOrDefault(QueryBase query)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            return this.GetMultiple(query)?.FirstOrDefault();
        }

        protected List<TEntity> GetMultiple(QueryBase query)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            return OrganizationService.RetrieveMultiple(query)?.Entities?.Select(item => item.ToEntity<TEntity>())?.ToList<TEntity>();
        }

        protected ExecuteMultipleResponse ExecuteMultipleRequests(OrganizationRequestCollection collectionRequests, bool continueOnError = true, bool returnResponses = true)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var request = new ExecuteMultipleRequest();

            request.Requests = collectionRequests;
            request.Settings = new ExecuteMultipleSettings
            {
                ContinueOnError = continueOnError,
                ReturnResponses = returnResponses
            };

            ExecuteMultipleResponse MultipleResponse = null;
            MultipleResponse = (ExecuteMultipleResponse)OrganizationService.Execute(request);
            if (MultipleResponse.IsFaulted)// check for errors
            {
                foreach (var response in MultipleResponse.Responses)
                {
                    if (response?.Fault != null)
                    {
                        var exceptionDetails = response?.Fault?.ErrorDetails?.FirstOrDefault(t => t.Key == "ApiOriginalExceptionKey");
                        if (exceptionDetails != null && exceptionDetails.HasValue && exceptionDetails?.Value != null)
                        {
                            this.GlobalContext.Log.Error($"Error : {response?.Fault?.ToString()}, stack trace : {exceptionDetails.Value.ToString()}");
                        }
                        else
                        {
                            this.GlobalContext.Log.Error(response?.Fault?.ToString());
                        }
                    }
                }
            }
            return MultipleResponse;
        }

        protected ExecuteTransactionResponse ExecuteTransactionRequests(OrganizationRequestCollection collectionRequests, bool returnResponses = true)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var request = new ExecuteTransactionRequest
            {
                Requests = collectionRequests,
                ReturnResponses = returnResponses
            };
            return (ExecuteTransactionResponse)OrganizationService.Execute(request);
        }

        protected OrganizationResponse Execute(OrganizationRequest request)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            return OrganizationService.Execute(request);
        }

        protected AssociateResponse ExecuteAssociateRequest(EntityReference primeryEntityRef, EntityReferenceCollection entityRefCollection, string relationshipName)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            AssociateRequest request = new AssociateRequest();
            request.Target = primeryEntityRef;
            request.RelatedEntities = entityRefCollection;
            request.Relationship = new Relationship(relationshipName);

            return (AssociateResponse)OrganizationService.Execute(request);
        }

        protected EntityCollection Fetch(QueryBase query)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            RetrieveMultipleResponse retrieveMultipleResponse = (RetrieveMultipleResponse)this.Execute(new RetrieveMultipleRequest { Query = query });

            return retrieveMultipleResponse?.EntityCollection;
        }

        protected OrganizationRequestCollection GenerateRequestsCollection(IEnumerable<TEntity> entitiesCollection, RequestType crmRequestType)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            OrganizationRequestCollection requestsCollection = null;
            if (entitiesCollection != null)
            {
                requestsCollection = new OrganizationRequestCollection();
                foreach (var entityRecord in entitiesCollection)
                {
                    OrganizationRequest request = this.GenerateRequestByRequestType(entityRecord, crmRequestType);
                    requestsCollection.Add(request);
                }
            }

            return requestsCollection;
        }

        protected OrganizationRequest GenerateRequestByRequestType(TEntity entityRecord, RequestType crmRequestType)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            string requestName = GetRequestNameByType(crmRequestType);
            OrganizationRequest request = new OrganizationRequest(requestName);
            request["Target"] = crmRequestType == RequestType.Delete ? request["Target"] = new EntityReference(entityRecord.LogicalName, entityRecord.Id)
                : request["Target"] = entityRecord;

            return request;
        }

        private string GetRequestNameByType(RequestType crmRequestType)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            if (!Enum.IsDefined(typeof(RequestType), crmRequestType))
            {
                throw new Exception(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidRequest));
            }

            return crmRequestType.ToString();
        }
        /// <summary>
        /// Grant a security principals (users or teams) access to the specified record.
        /// </summary>
        /// <param name="target">Specified record</param>
        /// <param name="principals">Security principals list</param>
        /// <param name="accessRights"> Access rights</param>
        /// <returns></returns>
        public ExecuteTransactionResponse GrantAccess(EntityReference target, List<EntityReference> principals, AccessRights accessRights)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            ExecuteTransactionResponse response = null;
            if (target != null && principals != null)
            {

                var requestes = new OrganizationRequestCollection();
                foreach (var principal in principals)
                {
                    var grantAccessRequest = new GrantAccessRequest
                    {
                        PrincipalAccess = new PrincipalAccess
                        {
                            AccessMask = accessRights,
                            Principal = principal
                        },
                        Target = target
                    };
                    requestes.Add(grantAccessRequest);
                }
                if (requestes.Count > 0)
                {
                    response = (ExecuteTransactionResponse)this.ExecuteTransactionRequests(requestes);
                }
            }
            return response;
        }

        /// <summary>
        /// Unshare all security principals (users or teams) that have access to, and access rights for, the specified record.
        /// </summary>
        /// <param name="targetReference">Specified record entity reference</param>
        public void RevokeAccess(EntityReference targetReference)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);
            var accessRequest = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = targetReference
            };

            var accessResponse = (RetrieveSharedPrincipalsAndAccessResponse)this.Execute(accessRequest);

            if (accessResponse != null && accessResponse.PrincipalAccesses.Length > 0)
            {
                Entity targetEntity = this.Get(targetReference.Id, new[] { "ownerid" });
                EntityReference ownerReference = targetEntity.GetAttributeValue<EntityReference>("ownerid");
                List<EntityReference> unsharedPrincipals = accessResponse.PrincipalAccesses.Select(e => e.Principal).Where(p => p.Id != ownerReference?.Id)?.ToList();
                RevokeAccess(targetReference, unsharedPrincipals);
            }
        }

        /// <summary>
        ///  Unshare list of principals (users or teams) that have access to, and access rights for, the specified record.
        /// </summary>
        /// <param name="targetReference">The specified record</param>
        /// <param name="principals">List of principals (users or teams) </param>
        /// <returns></returns>
        public ExecuteTransactionResponse RevokeAccess(EntityReference targetReference, List<EntityReference> principals)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            ExecuteTransactionResponse response = null;
            if (targetReference != null && principals != null)
            {
                var requestes = new OrganizationRequestCollection();
                foreach (var principal in principals)
                {
                    RevokeAccessRequest revokeRequest = new RevokeAccessRequest()
                    {
                        Target = targetReference,
                        Revokee = principal
                    };
                    requestes.Add(revokeRequest);
                }
                if (requestes.Count > 0)
                {
                    response = (ExecuteTransactionResponse)this.ExecuteTransactionRequests(requestes);
                }
            }
            return response;
        }

        /// <summary>
        ///  Grant a security principals (users or teams) access to the specified record.
        /// </summary>
        /// <param name="targetReference">The specified record</param>
        /// <param name="principals">List of principals (users or teams)</param>
        /// <param name="accessRights">access rights</param>
        /// <returns></returns>
        public ExecuteTransactionResponse ModifyAccess(EntityReference targetReference, List<EntityReference> principals, AccessRights accessRights)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            ExecuteTransactionResponse response = null;
            if (targetReference != null && principals != null)
            {
                var requestes = new OrganizationRequestCollection();
                foreach (var principal in principals)
                {
                    var grantAccessRequest = new ModifyAccessRequest
                    {
                        PrincipalAccess = new PrincipalAccess
                        {
                            AccessMask = accessRights,
                            Principal = principal
                        },
                        Target = targetReference
                    };
                    requestes.Add(grantAccessRequest);
                }
                if (requestes.Count > 0)
                {
                    response = (ExecuteTransactionResponse)this.ExecuteTransactionRequests(requestes);
                }
            }
            return response;
        }

        protected List<TEntity> GetWithPagingByFilterExpression(FilterExpression filter, params string[] columns)
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            QueryExpression query = new QueryExpression
            {
                EntityName = this.entityLogicalName,
                ColumnSet = new ColumnSet(columns)
            };

            query.Criteria.AddFilter(filter);

            return this.GetMultipleWithPaging(query)?.Select(item => item.ToEntity<TEntity>())?.ToList();
        }

        public IEnumerable<T> ExecuteQuery<T>(QueryBase query) where T : Entity
        {
            this.GlobalContext.LogEntry();
            return (IEnumerable<T>)this.GetMultiple(query);
        }

        public OptionSetMetadata GetGlobalOptionSetMetaData(string optionSetName)
        {
            this.GlobalContext.LogEntry();

            RetrieveOptionSetRequest retrieveOptionSetRequest = new RetrieveOptionSetRequest
            {
                Name = optionSetName,


            };
            RetrieveOptionSetResponse retrieveOptionSetResponse = (RetrieveOptionSetResponse)OrganizationService.Execute(retrieveOptionSetRequest);
            OptionSetMetadata retrievedOptionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;
            return retrievedOptionSetMetadata;
        }

        public string GetParsedMessage(string message, EntityReference entryPoint, IEntityValueResolver entityValueResolver = null)
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
        public EntityMetadata GetEntityMetadata()
        {
            this.GlobalContext.LogEntry(this.entityLogicalName);

            RetrieveEntityRequest request = new RetrieveEntityRequest
            {
                LogicalName = this.entityLogicalName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes
            };

            RetrieveEntityResponse response =
                (RetrieveEntityResponse)this.Execute(request);

            return response?.EntityMetadata;
        }

        public string GetPrimeryAttributeValue(EntityReference entityRef)
        {
            this.GlobalContext.LogEntry();
            string primaryAttributeValue = string.Empty;

            if (entityRef != null)
            {
                primaryAttributeValue = entityRef.Name;

                if (string.IsNullOrWhiteSpace(primaryAttributeValue))
                {
                    EntityMetadata entityMetadata = GetEntityMetadata(entityRef.LogicalName);
                    string primaryNameAttribute = entityMetadata.PrimaryNameAttribute;

                    Entity retrievedEntity = this.Get(entityRef.Id, new[] { primaryNameAttribute });
                    primaryAttributeValue = retrievedEntity.GetAttributeValue<string>(primaryNameAttribute);
                }
            }

            return primaryAttributeValue;
        }
    }
}
