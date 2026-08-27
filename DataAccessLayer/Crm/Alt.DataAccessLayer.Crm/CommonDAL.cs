using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework;
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
    public class CommonDAL : CrmBaseDAL<Entity>
    {
        public CommonDAL(GlobalContext globalContext, string entityName) : base(globalContext, entityName)
        {
        }

        public Guid SendAppNotification(EntityReference recipient, AppNotificationSettings appNotificationSettings)
        {
            var request = new OrganizationRequest()
            {
                RequestName = "SendAppNotification",
                Parameters = new ParameterCollection
                {
                    ["Title"] = appNotificationSettings.Title,
                    ["Recipient"] = recipient,
                    ["Body"] = appNotificationSettings.Body,
                    ["IconType"] = appNotificationSettings.IconType != null ?
                                    new OptionSetValue((int)appNotificationSettings.IconType.Value)
                                    : new OptionSetValue((int)AppNotificationIconTypeCode.Info),
                    ["ToastType"] = appNotificationSettings.ToastType != null ?
                                    new OptionSetValue((int)appNotificationSettings.ToastType.Value)
                                    : new OptionSetValue((int)AppNotificationToastTypeCode.Timed),
                    ["Actions"] = appNotificationSettings.Actions
                }
            };

            OrganizationResponse response = base.Execute(request);
            return (Guid)response.Results["NotificationId"];
        }

        protected Entity GetById(string entityLogicalName, Guid id, string[] columns)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            var columnSet = columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true);
            RetrieveResponse retrieveResponse = (RetrieveResponse)this.Execute(new RetrieveRequest { Target = new EntityReference(entityLogicalName, id), ColumnSet = columnSet });
            return retrieveResponse.Entity;
        }

        public void CallCrmCustomAPI(Guid id, ApiConfigurationCode apiConfigurationCode)
        {
            OrganizationRequest request = new OrganizationRequest("alt_CrmApi")
            {
                ["Content"] = $"{{\"Id\": \"{id}\",\"ApiConfigurationCode\":{(int)apiConfigurationCode}}}"
            };
            base.Execute(request);
        }

        public List<Entity> Fetch(string fetchQuery)
        {
            this.GlobalContext.LogEntry();
            EntityCollection responseCollection = base.Fetch(new FetchExpression(fetchQuery));
            return (responseCollection?.Entities.ToList());
        }

        public RetrieveDuplicatesResponse ExecuteDuplicateDetectionRequest(Entity entityToSearch, string matchingEntityName = null)
        {
            GlobalContext.LogEntry();
            RetrieveDuplicatesRequest request = new RetrieveDuplicatesRequest
            {
                BusinessEntity = entityToSearch,
                MatchingEntityName = matchingEntityName ?? entityToSearch.LogicalName,
                PagingInfo = new PagingInfo() { PageNumber = 1, Count = 100 }
            };

            RetrieveDuplicatesResponse response = (RetrieveDuplicatesResponse)Execute(request);
            return response;
        }
        public Entity RetrieveLookupValues(Entity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var lookupAttributes = target.Attributes
                .Where(a => a.Value is EntityReference)
                .ToList();

            if (!lookupAttributes.Any())
                return null;

            Entity result = new Entity(target.LogicalName);

            foreach (var attribute in lookupAttributes)
            {
                EntityReference targetReference = (EntityReference)attribute.Value;

                if (targetReference == null ||
                    targetReference.Id == Guid.Empty ||
                    string.IsNullOrWhiteSpace(targetReference.LogicalName))
                {
                    continue;
                }

                EntityMetadata metadata =
                    GlobalContext.GetEntityMetadata(targetReference.LogicalName);

                string primaryNameAttribute =
                    metadata?.PrimaryNameAttribute;

                if (string.IsNullOrWhiteSpace(primaryNameAttribute))
                {
                    continue;
                }

                Entity lookupEntity =
                    GlobalContext.OrganizationService.Retrieve(
                        targetReference.LogicalName,
                        targetReference.Id,
                        new ColumnSet(primaryNameAttribute));

                result[attribute.Key] = new EntityReference(
                    targetReference.LogicalName,
                    targetReference.Id)
                {
                    Name = lookupEntity.GetAttributeValue<string>(primaryNameAttribute)
                };
            }

            return result;
        }


    }
}
