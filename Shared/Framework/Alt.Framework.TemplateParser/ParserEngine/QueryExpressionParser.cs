using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Alt.Framework.TemplateParser.ParserEngine
{
    public class QueryExpressionParser
    {
        private CustomLinkEntity root;
        public QueryExpressionParser(CustomLinkEntity customLinkEntity)
        {
            this.root = customLinkEntity;
        }

        public QueryExpression ConvertToQueryExpression(IOrganizationService organizationService)
        {
            string entityIdLogicalName = organizationService != null
                             && this.IsActivity(organizationService, root.EntityName) ?
                                     "activityid" : $"{root.EntityName.ToLower()}id";

            QueryExpression query = new QueryExpression();
            query.EntityName = root.EntityName;
            query.ColumnSet = new ColumnSet(root.Attributes.ToArray());
            query.NoLock = true;
            query.Criteria.Filters.Add(
               new FilterExpression()
               {
                   FilterOperator = LogicalOperator.And,
                   Conditions =
                   {
                           new ConditionExpression(entityIdLogicalName, ConditionOperator.Equal, new Guid(root.Id)),
                   }
               }
           );

            foreach (var customLinkEntity in root.LinkEntities)
            {
                var t = ConvertCustomLinkEntityToLinkEntity(customLinkEntity, null);
                query.LinkEntities.Add(t);
            }

            return query;
        }
        public QueryExpression ConvertTableToQueryExpression(CustomEntity customLinkEntity, Guid attributeFilterValue)
        {
            QueryExpression query = null;
            if (customLinkEntity != null && customLinkEntity.IsLinkEntityQuery)
            {
                query = new QueryExpression();
                query.EntityName = root.EntityName;
                query.ColumnSet = new ColumnSet(root.Attributes.ToArray());
                query.NoLock = true;
                query.Criteria.Filters.Add(
                   new FilterExpression()
                   {
                       FilterOperator = LogicalOperator.And,
                       Conditions =
                       {
                           new ConditionExpression($"{customLinkEntity.TableAttributeFilter}", ConditionOperator.Equal, attributeFilterValue),
                       }
                   }
               );
            }
            if(root.LinkEntities != null && root.LinkEntities.Count > 0)
            {
                foreach (var innerCustomLinkEntity in root.LinkEntities)
                {
                    var t = ConvertCustomLinkEntityToLinkEntity(innerCustomLinkEntity, null);
                    query.LinkEntities.Add(t);
                }
            }
           
            return query;
        }

        public QueryExpression ConvertToManyToManyQueryExpression(CustomEntity customLinkEntity, EntityReference entityReferenceFrom)
        {
            QueryExpression query = null;
            string entityReferenceFromFromAttribute = $"{entityReferenceFrom.LogicalName}id";
            if (customLinkEntity != null && customLinkEntity.IsLinkEntityQuery)
            {
                query = new QueryExpression();
                query.EntityName = root.EntityName;
                query.ColumnSet = new ColumnSet(entityReferenceFromFromAttribute);
                query.NoLock = true;
                query.Criteria.Filters.Add(
                   new FilterExpression()
                   {
                       FilterOperator = LogicalOperator.And,
                       Conditions =
                       {
                           new ConditionExpression($"{entityReferenceFrom.LogicalName}id", ConditionOperator.Equal, entityReferenceFrom.Id),
                       }
                   }
               );
            }
            if (root.LinkEntities != null && root.LinkEntities.Count > 0)
            {
                foreach (var innerCustomLinkEntity in root.LinkEntities)
                {
                    var t = ConvertCustomLinkEntityToLinkEntity(innerCustomLinkEntity, null);
                    query.LinkEntities.Add(t);
                }
            }

            return query;
        }

        private LinkEntity ConvertCustomLinkEntityToLinkEntity(CustomLinkEntity customQuery, LinkEntity query)
        {
            if (customQuery == null)
            {
                return query;
            }

            var linkEntity = new LinkEntity(customQuery.LinkFromEntityName, customQuery.LinkToEntityName, customQuery.LinkFromAttributeName, customQuery.LinkToAttributeName,
                   JoinOperator.LeftOuter);
            linkEntity.EntityAlias = customQuery.Alias;
            linkEntity.Columns = new ColumnSet(customQuery.Attributes.ToArray());

            var linkEntitiesCount = customQuery?.LinkEntities?.Count;
            if (query != null)
            {
                query.LinkEntities.Add(linkEntity);
            }
            foreach (CustomLinkEntity customChildQuery in customQuery.LinkEntities)
            {
                if (query == null)
                {
                    query = linkEntity;
                    ConvertCustomLinkEntityToLinkEntity(customChildQuery, linkEntity);
                }
                else
                {
                    ConvertCustomLinkEntityToLinkEntity(customChildQuery, linkEntity);
                }
            }

            if (linkEntitiesCount != null && linkEntitiesCount == 0)
            {
                if (query == null)
                {
                    query = linkEntity;
                }
                //else
                //{
                //    query.LinkEntities.Add(linkEntity);
                //}
            }

            return query;
        }

        private bool IsActivity(IOrganizationService organizationService, string entityLogicalName)
        {
            EntityMetadata entityMetadata = organizationService.GetEntityMetadata(entityLogicalName);
            return entityMetadata.IsActivity.Value;
        }

    }
}
