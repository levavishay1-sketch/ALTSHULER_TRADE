using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.Framework.Mapper
{
    public class DynamicCrmMapper
    {
        private readonly EntityMetadata entityMetadata;
        CrmEntityBuilderConfiguration crmEntityBuilderConfiguration;
        public DynamicCrmMapper(EntityMetadata entityMetadata, CrmEntityBuilderConfiguration crmEntityBuilderConfiguration = null)
        {
            this.entityMetadata = entityMetadata;
            this.crmEntityBuilderConfiguration = crmEntityBuilderConfiguration;
        }

        public Entity ToEntity(dynamic record)
        {
            var keyValuePairs = (IDictionary<string, object>)record;
            Entity entity = new Entity(entityMetadata.LogicalName);

            foreach (var attribute in entityMetadata.Attributes)
            {
                if (string.IsNullOrEmpty(attribute.AttributeOf) && attribute.LogicalName != null && keyValuePairs.ContainsKey(attribute.LogicalName))
                {
                    var value = keyValuePairs[attribute.LogicalName];
                    entity[attribute.LogicalName] = value != null ? this.ConvertValue(value, attribute) : value;
                }
            }
            if (crmEntityBuilderConfiguration?.RecordMatchtchingCriteria == RecordMatchingCriteriaCode.AlternateKey)
            {
                this.AddKeyAttributes(entity);
            }
            return entity;
        }

        private void AddKeyAttributes(Entity entity)
        {
            if (this.crmEntityBuilderConfiguration.RecordAlternateKeyAttributes != null)
            {
                foreach (var keyAttribute in this.crmEntityBuilderConfiguration.RecordAlternateKeyAttributes)
                {
                    var value = entity.Attributes[keyAttribute];
                    entity.KeyAttributes.Add(keyAttribute, value);
                    entity.Attributes.Remove(keyAttribute);
                }
            }        
        }

        private object ConvertValue(object value, AttributeMetadata attributeMetadata, int? attributetypeCode = null)
        {
            string valueStr = value.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(valueStr))
            {
                AttributeTypeCode? attributeTypeCode = attributetypeCode != null ?
                    (AttributeTypeCode)attributetypeCode.Value
                    : attributeMetadata.AttributeType;
                switch (attributeTypeCode)
                {
                    case AttributeTypeCode.Boolean:
                        {
                            value = valueStr == "0" || valueStr == "1" ?
                                valueStr == "1"
                                : bool.Parse(valueStr);
                            break;
                        }
                    case AttributeTypeCode.DateTime:
                        {
                            if (DateTime.TryParse(valueStr, out DateTime dateTime))
                            {
                                value = dateTime;
                            }
                            else
                            {
                                value = null;
                            }
                            break;
                        }
                    case AttributeTypeCode.Decimal:
                        {
                            value = decimal.Parse(valueStr);
                            break;
                        }
                    case AttributeTypeCode.Double:
                        {
                            value = double.Parse(valueStr);
                            break;
                        }
                    case AttributeTypeCode.Integer:
                        {
                            value = int.Parse(valueStr);
                            break;
                        }
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                        {
                            value = CreateLookupValue(valueStr, attributeMetadata.LogicalName);                            
                            break;
                        }
                    case AttributeTypeCode.Uniqueidentifier:
                        {
                            value = new Guid(valueStr);
                            break;
                        }
                    case AttributeTypeCode.Money:
                        {
                            value = new Money(decimal.Parse(valueStr));
                            break;
                        }
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.Status:
                    case AttributeTypeCode.State:
                        {
                            value = new OptionSetValue(int.Parse(valueStr));
                            break;
                        }
                    default:
                        break;
                }
            }
            else
            {
                value = null;
            }
            return value;
        }

        private EntityReference CreateLookupValue(string value, string logicalName)
        {
            EntityReference entityReference;
            string referencedEntity = entityMetadata.ManyToOneRelationships.Where(r => r.ReferencedAttribute == logicalName).FirstOrDefault()?.ReferencedEntity;
            var textLookup = this.crmEntityBuilderConfiguration?.TextLookups?
                .Where(t => t.AttributeName == logicalName).FirstOrDefault();
            if (textLookup != null)
            {
                entityReference = new EntityReference(textLookup.TargetEntity ?? referencedEntity, textLookup.TargetField, this.ConvertValue(value, null, textLookup.TargetFieldTypeCode));
            }
            else
            {
               entityReference = new EntityReference(referencedEntity, new Guid(value));
            }                   
            return entityReference;
        }
    }
}
