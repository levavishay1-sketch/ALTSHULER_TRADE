using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.Mapper.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alt.Framework.Mapper
{
    public class CrmEntityMapper<TApiEntity> where TApiEntity : ApiEntityBase, ICrmEntityMapperable
    {
        private enum DestinationMappingFlow
        {
            ToCrm,
            ToApiEntity
        }

        public Entity MappApiEntityToCrmEntity(TApiEntity apiEntity, bool ignoreNull = false)
        {
            Entity entity = apiEntity.Id != null ? new Entity(apiEntity.LogicalName, apiEntity.Id.Value) : new Entity(apiEntity.LogicalName);
            if (entity.Id == Guid.Empty)
            {
                var firstKey = apiEntity.GetFirstOrDefaultEntityKeyValue();
                if (!firstKey.Equals(default(KeyValuePair<string, object>)))
                {
                    entity.KeyAttributes = new KeyAttributeCollection() { firstKey };
                }
            }
            var modifiedPropertiesWithMetaData = this.GetValidMappToCrmApiProperties(apiEntity);
            var modifiedPropertiesKeys = new List<string>(modifiedPropertiesWithMetaData.Keys); //apiEntity.GetModifiedPropertiesKeys();
            foreach (var key in modifiedPropertiesKeys)
            {
                var apiPropertyValue = apiEntity.GetValueByKey(key);
                ApiProperty apiProperty = modifiedPropertiesWithMetaData[key];
                if (key != "Id" && !string.IsNullOrWhiteSpace(modifiedPropertiesWithMetaData[key]?.CrmProperty?.CrmMetaData))
                {
                    if (!ignoreNull || ignoreNull && apiProperty.Value != null)
                    {
                        entity[modifiedPropertiesWithMetaData[key].CrmProperty.CrmMetaData] = this.ConvertSourceValueTypeToDestinationValueType(apiProperty.Value, apiProperty.CrmProperty.CrmPropertyType, null, DestinationMappingFlow.ToCrm);
                    }
                }
            }
            return entity;
        }

        public Entity MappOnlyDeltaModifiedPropertiesOnUpdateWithRetreive(TApiEntity entityToUpdate, Func<Guid, string[], TApiEntity> getDalCallBack)
        {
            var entityToUpdateModifiedPropertiesWithMetaData = this.GetValidMappToCrmApiProperties(entityToUpdate);
            string[] crmColumns = entityToUpdateModifiedPropertiesWithMetaData?.Where(t => !string.IsNullOrWhiteSpace(entityToUpdateModifiedPropertiesWithMetaData[t.Key].CrmProperty.CrmMetaData))
                ?.Select(t => entityToUpdateModifiedPropertiesWithMetaData[t.Key].CrmProperty.CrmMetaData)?.ToArray();

            TApiEntity apiCrmEntity = crmColumns != null ? getDalCallBack(entityToUpdate.Id.Value, crmColumns) : null;

            Entity actualApiEntityToUpdate = apiCrmEntity != null ? new Entity(entityToUpdate.LogicalName, entityToUpdate.Id.Value) : null;
            foreach (var key in entityToUpdate.GetModifiedPropertiesKeys())
            {
                if (entityToUpdate.GetValueByKey(key) != null && !entityToUpdate.GetValueByKey(key).Equals(apiCrmEntity.GetValueByKey(key)))
                {
                    actualApiEntityToUpdate[entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmMetaData] = this.ConvertSourceValueTypeToDestinationValueType(entityToUpdate.GetValueByKey(key), entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmPropertyType, null, DestinationMappingFlow.ToCrm);
                }
            }

            return actualApiEntityToUpdate?.Attributes != null && actualApiEntityToUpdate.Attributes.Count > 0 ? actualApiEntityToUpdate : null;
        }

        public Entity MappOnlyDeltaModifiedPropertiesOnUpdate(TApiEntity entityToUpdate, TApiEntity currentApiEntityToUpdate)
        {
            var entityToUpdateModifiedPropertiesWithMetaData = this.GetValidMappToCrmApiProperties(entityToUpdate);
            var currentApiEntityToUpdateModifiedPropertiesWithMetaData = this.GetValidMappToCrmApiProperties(currentApiEntityToUpdate);

            Entity actualApiEntityToUpdate = currentApiEntityToUpdate != null ? new Entity(entityToUpdate.LogicalName, currentApiEntityToUpdate.Id.Value) : null;
            foreach (var key in entityToUpdate.GetModifiedPropertiesKeys())
            {
                var currentApiEntityPropertyValue = currentApiEntityToUpdateModifiedPropertiesWithMetaData.ContainsKey(key) ? currentApiEntityToUpdateModifiedPropertiesWithMetaData[key] : null;
                if (entityToUpdateModifiedPropertiesWithMetaData.ContainsKey(key)
                    && !entityToUpdateModifiedPropertiesWithMetaData[key].Equals(currentApiEntityPropertyValue))
                {
                    var attribute = entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmMetaData;
                    if (!string.IsNullOrWhiteSpace(attribute))
                    {
                        actualApiEntityToUpdate[entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmMetaData] =
                       this.ConvertSourceValueTypeToDestinationValueType(entityToUpdate.GetValueByKey(key), entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmPropertyType, null, DestinationMappingFlow.ToCrm);
                    }
                }
            }

            return actualApiEntityToUpdate?.Attributes != null && actualApiEntityToUpdate.Attributes.Count > 0 ? actualApiEntityToUpdate : null;
        }

        public Entity MappOnlyDeltaModifiedPropertiesOnUpdate(TApiEntity entityToUpdate, Func<string, object, string[], bool, List<TApiEntity>> getDalCallBack)
        {
            var entityToUpdateModifiedPropertiesWithMetaData = this.GetValidMappToCrmApiProperties(entityToUpdate);
            List<string> crmColumns = entityToUpdateModifiedPropertiesWithMetaData?.Where(t => !string.IsNullOrWhiteSpace(entityToUpdateModifiedPropertiesWithMetaData[t.Key].CrmProperty.CrmMetaData))
                ?.Select(t => entityToUpdateModifiedPropertiesWithMetaData[t.Key].CrmProperty.CrmMetaData)?.ToList();
            var entityKey = entityToUpdate.GetFirstOrDefaultEntityKeyValue();
            var idAttributeName = entityToUpdate is ApiActivityPointerBase ? "activityid" : $"{entityToUpdate.LogicalName}id";

            crmColumns.Add(idAttributeName);
            TApiEntity apiCrmEntity = crmColumns != null ? getDalCallBack(entityKey.Key, entityKey.Value, crmColumns.ToArray(), true).FirstOrDefault() : null;

            Entity actualApiEntityToUpdate = apiCrmEntity != null ? new Entity(apiCrmEntity.LogicalName, apiCrmEntity.Id.Value) : null;
            foreach (var key in entityToUpdate.GetModifiedPropertiesKeys())
            {
                if (entityToUpdateModifiedPropertiesWithMetaData.ContainsKey(key) && entityToUpdate.GetValueByKey(key) != null && !entityToUpdate.GetValueByKey(key).Equals(apiCrmEntity.GetValueByKey(key)))
                {
                    actualApiEntityToUpdate[entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmMetaData] = this.ConvertSourceValueTypeToDestinationValueType(entityToUpdate.GetValueByKey(key), entityToUpdateModifiedPropertiesWithMetaData[key].CrmProperty.CrmPropertyType, null, DestinationMappingFlow.ToCrm);
                }
            }

            return actualApiEntityToUpdate?.Attributes != null && actualApiEntityToUpdate.Attributes.Count > 0 ? actualApiEntityToUpdate : null;
        }


        private Dictionary<string, ApiProperty> GetValidMappToCrmApiProperties(TApiEntity apiEntity)
        {
            Dictionary<string, ApiProperty> valueToReturn = new Dictionary<string, ApiProperty>();

            var props = apiEntity.GetType().GetProperties().Where(p => apiEntity.Contains(p.Name));
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is CrmEntityMapperAttribute attribute)
                    {
                        if (attribute.MappToCrm)
                        {
                            var value = apiEntity.GetValueByKey(attribute.ApiPropertyName);
                            valueToReturn.Add(attribute.ApiPropertyName, new ApiProperty(attribute.ApiPropertyName, attribute.CrmPropertyName, attribute.TargetCrmPropertyType, value));
                        }
                    }
                }
            }

            return valueToReturn;
        }

        private object ConvertApiToCrmActivityParty(object value)
        {
            if (value != null)
            {
                var entityCollection = new List<Entity>();
                if (value is IEnumerable<ApiEntityBase> apiCollection)
                {
                    foreach (var apiEntity in apiCollection)
                    {
                        entityCollection.Add(this.BuildEntityFromApiActivityParty(apiEntity));
                    }
                }
                else
                {
                    // entityCollection.Add((Entity)this.ConvertSourceValueTypeToDestinationValueType(apiEntityValue, CrmPropertyType.Entity, null, DestinationMappingFlow.ToCrm));
                    entityCollection.Add(this.BuildEntityFromApiActivityParty(value as ApiEntityBase));
                }

                var activityPartyCollection = this.ToActivityPartyCollection(entityCollection);
                return activityPartyCollection != null && activityPartyCollection.Count() > 0 ? activityPartyCollection.ToArray<Entity>() : null;
            }
            return value;
        }

        private Entity BuildEntityFromApiActivityParty(ApiEntityBase apiEntity)
        {
            Entity mappedEntity = null;
            if (apiEntity.LogicalName != null)
            {
                if (apiEntity.Id == Guid.Empty)
                {
                    var firstKey = apiEntity.GetFirstOrDefaultEntityKeyValue();
                    if (!firstKey.Equals(default(KeyValuePair<string, object>)))
                    {
                        mappedEntity = new Entity() { LogicalName = apiEntity.LogicalName };
                        mappedEntity.KeyAttributes = new KeyAttributeCollection() { firstKey };
                    }
                }
                else
                {
                    mappedEntity = new Entity() { LogicalName = apiEntity.LogicalName, Id = apiEntity.Id.Value };
                }
            }
            else
            {
                mappedEntity = new Entity();
                mappedEntity["addressused"] = apiEntity.GetValueByKey("AddressUsed");
            }

            return mappedEntity;
        }

        // to api entity mapper
        public TApiEntity MappCrmEntityToApiEntity(Entity crmEntity)
        {
            TApiEntity apiEntity = (TApiEntity)Activator.CreateInstance(typeof(TApiEntity));
            PropertyInfo[] props = typeof(TApiEntity).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                if (prop.Name == "LogicalName" && string.IsNullOrWhiteSpace(prop.GetValue(apiEntity)?.ToString())) {
                    prop.SetValue(apiEntity, crmEntity.LogicalName);
                }
                foreach (object attr in attrs)
                {
                    if (attr is CrmEntityMapperAttribute attribute)
                    {
                        if (attribute.ApiPropertyName == "Id")
                        {
                            attribute.CrmPropertyName = $"{crmEntity.LogicalName.ToLower()}id";
                            prop.SetValue(apiEntity, crmEntity.Id);
                        }                        
                        if (attribute.CrmPropertyName != null && crmEntity.Contains(attribute.CrmPropertyName) && attribute.MappFromCrm)
                        {

                            var value = this.ConvertSourceValueTypeToDestinationValueType(crmEntity[attribute.CrmPropertyName], attribute.TargetCrmPropertyType, prop, DestinationMappingFlow.ToApiEntity);
                            prop.SetValue(apiEntity, value);
                        }
                    }
                }
            }

            return apiEntity;
        }

        private object ConvertSourceValueTypeToDestinationValueType(object value, CrmPropertyType type, PropertyInfo prop, DestinationMappingFlow destinationMappingFlow)
        {
            if (value == null)
            {
                return null;
            }

            switch (type)
            {
                case CrmPropertyType.String:
                    {
                        object stringValueToReturn = value;
                        if (destinationMappingFlow == DestinationMappingFlow.ToApiEntity)
                        {
                            if (prop.PropertyType == typeof(Guid))
                            {
                                stringValueToReturn = new Guid(value.ToString());
                            }
                        }
                        else
                        {
                            stringValueToReturn = value?.ToString();
                        }
                        return stringValueToReturn;
                    }
                case CrmPropertyType.Int:
                case CrmPropertyType.Bool:
                case CrmPropertyType.Guid:
                case CrmPropertyType.DateTime:
                case CrmPropertyType.Decimal:
                    {
                        return value;
                    }
                case CrmPropertyType.EntityReference:
                    {
                        return this.MappEntityReferenceCrmPropertyTypeHandler(value, prop, destinationMappingFlow);
                    }
                case CrmPropertyType.OptionSet:
                    {
                        if (destinationMappingFlow == DestinationMappingFlow.ToApiEntity)
                        {
                            return ((OptionSetValue)value).Value;
                        }
                        else
                        {
                            return new OptionSetValue((int)value);
                        }
                    }
                case CrmPropertyType.ActivityParty:
                    {
                        if (destinationMappingFlow == DestinationMappingFlow.ToApiEntity)
                        {
                            return this.MapActivityPartyProperty(value, prop);
                        }
                        else
                        {
                            return this.ConvertApiToCrmActivityParty(value);
                        }
                    }
                case CrmPropertyType.Money:
                    {
                        if (destinationMappingFlow == DestinationMappingFlow.ToApiEntity)
                        {
                            return ((Money)value).Value;
                        }
                        else
                        {
                            return new Money((decimal)value);
                        }
                    }
                case CrmPropertyType.Float:
                    {
                        if (destinationMappingFlow == DestinationMappingFlow.ToApiEntity)
                        {
                            if (prop.PropertyType == typeof(float?) || prop.PropertyType == typeof(float))
                            {
                                return ((float)((Double)value));
                            }
                            return ((Double)value);
                        }
                        else
                        {

                            return Convert.ToDouble(value); ;
                        }
                    }
                case CrmPropertyType.OptionSetCollection:
                    {
                        List<int> options;
                        OptionSetValueCollection optionSetCollection;
                        if (destinationMappingFlow == DestinationMappingFlow.ToApiEntity)
                        {
                            options = new List<int>();
                            optionSetCollection = (OptionSetValueCollection)value;
                            foreach (var optionSet in optionSetCollection)
                            {
                                options.Add(((OptionSetValue)optionSet).Value);
                            }
                            return options;
                        }
                        else
                        {
                            optionSetCollection = new OptionSetValueCollection();
                            options = (List<int>)value;
                            foreach (var option in options)
                            {
                                optionSetCollection.Add(new OptionSetValue(option));
                            }
                            return optionSetCollection;
                        }
                    }
                default:
                    break;
            }
            return value;
        }

        private object MappEntityReferenceCrmPropertyTypeHandler(object value, PropertyInfo prop, DestinationMappingFlow destinationMappingFlow)
        {
            switch (destinationMappingFlow)
            {
                case DestinationMappingFlow.ToApiEntity:
                    {
                        return this.ConvertEntityReferenceToApiEntityInstance(value, prop);
                    }
                case DestinationMappingFlow.ToCrm:
                    {
                        return ConvertApiEntityInstanceToEntityReference(value);
                    }
                default:
                    {
                        throw new Exception("invalid destination mapping flow");
                    }
            }
        }

        private ApiEntityBase ConvertEntityReferenceToApiEntityInstance(object value, PropertyInfo prop)
        {
            ApiEntityBase apiEntity = null;
            EntityReference entityReference = value as EntityReference;

            var constructors = prop.PropertyType.GetConstructors().FirstOrDefault(c => c.GetParameters().Count() == 1 && c.GetParameters().FirstOrDefault().ParameterType == typeof(string));

            if (constructors != null)
            {
                apiEntity = (ApiEntityBase)Activator.CreateInstance(prop.PropertyType, entityReference.LogicalName);
            }
            else
            {
                apiEntity = (ApiEntityBase)Activator.CreateInstance(prop.PropertyType);
            }
            if (entityReference.Id != null)
            {
                apiEntity.Id = entityReference.Id;
            }
            // need to add key attributes code 

            if (!string.IsNullOrWhiteSpace(entityReference.Name))
            {
                var customeAttributes = apiEntity.GetType().GetProperties().Where(a => a.GetCustomAttribute(typeof(CrmEntityMapperAttribute)) != null);
                var propertyWithprimaryAttribute = customeAttributes?.FirstOrDefault(c => (c.GetCustomAttribute(typeof(CrmEntityMapperAttribute)) as CrmEntityMapperAttribute).IsCrmPrimaryAttribute);
                if (propertyWithprimaryAttribute != null && propertyWithprimaryAttribute.CanWrite)
                {
                    propertyWithprimaryAttribute.SetValue(apiEntity, entityReference.Name);
                }
            }

            return apiEntity;
        }

        private EntityReference ConvertApiEntityInstanceToEntityReference(object value)
        {
            ApiEntityBase apiEntity = value as ApiEntityBase;
            EntityReference entityReference = null;
            if (apiEntity != null)
            {
                entityReference = new EntityReference(apiEntity.LogicalName);
                if (apiEntity.Id != null)
                {
                    entityReference.Id = apiEntity.Id.Value;
                }
                else
                {
                    var entityKey = apiEntity.GetFirstOrDefaultEntityKeyValue();
                    if (!entityKey.Equals(default(KeyValuePair<string, object>)))
                    {
                        entityReference.KeyAttributes = new KeyAttributeCollection() { entityKey };
                    }
                }
            }
            return entityReference;
        }

        private object MapActivityPartyProperty(object value, PropertyInfo prop)
        {
            if (value != null && value is EntityCollection crmEntityCollection)
            {
                var types = prop.PropertyType.GetTypeInfo().GetGenericArguments();
                if (types.Count() > 0)
                {
                    List<ApiEntityBase> apiCollection = new List<ApiEntityBase>();
                    foreach (var party in crmEntityCollection.Entities)
                    {
                        if (party.LogicalName == "activityparty")
                        {
                            var activityParty = party.GetAttributeValue<EntityReference>("partyid");
                            if (activityParty != null)
                            {

                                var apiEntity = (ApiEntityBase)Activator.CreateInstance(types[0], activityParty.LogicalName);
                                apiEntity.Id = activityParty.Id;
                                apiCollection.Add(apiEntity);
                            }
                        }
                    }
                    return apiCollection;
                }
                else
                {
                    var activityPartyEntityReference = crmEntityCollection.Entities.FirstOrDefault(t => t.LogicalName == "activityparty")?.GetAttributeValue<EntityReference>("partyid");
                    if (activityPartyEntityReference != null)
                    {
                        return (ApiEntityBase)this.ConvertSourceValueTypeToDestinationValueType(activityPartyEntityReference, CrmPropertyType.EntityReference, prop, DestinationMappingFlow.ToApiEntity);
                    }
                    return null;
                }
            }
            return value;
        }

        public IEnumerable<Entity> ToActivityPartyCollection(IEnumerable<Entity> entities)
        {
            List<Entity> collection = new List<Entity>();
            foreach (var toEntity in entities)
            {
                Entity activityParty = new Entity("activityparty");
                if (toEntity.LogicalName == null && toEntity["addressused"] != null)
                {
                    activityParty.Attributes.Add("addressused", toEntity["addressused"] as string);
                }
                else
                {
                    if (toEntity.Id == Guid.Empty)
                    {
                        var entityKey = toEntity.KeyAttributes?.FirstOrDefault();
                        if (!entityKey.Equals(default(KeyValuePair<string, object>)))
                        {
                            EntityReference entityReference = new EntityReference(toEntity.LogicalName);
                            entityReference.KeyAttributes = new KeyAttributeCollection() { entityKey.Value };
                            activityParty.Attributes.Add("partyid", entityReference);
                        }
                        else
                        {
                            throw new Exception("invalid entity for actitvy party mapping");
                        }
                    }
                    else { activityParty.Attributes.Add("partyid", new EntityReference(toEntity.LogicalName, toEntity.Id)); }

                }
                collection.Add(activityParty);
            }
            return collection.Count > 0 ? collection : null;
        }
    }
}
