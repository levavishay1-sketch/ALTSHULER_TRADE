using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.Logger;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Alt.Framework.Extensions
{
    public static class EntityExtensions
    {
        public static T Merge<T>(this T target, T image) where T : Entity, new()
        {
            Entity mergedEntity = new Entity(target.LogicalName, target.Id);
            if (image != null)
            {
                Entity privateImage = image as Entity;
                foreach (var attrib in privateImage.Attributes)
                {
                    mergedEntity.Attributes[attrib.Key] = attrib.Value;
                }

                foreach (var formatedValue in privateImage.FormattedValues)
                {
                    mergedEntity.FormattedValues[formatedValue.Key] = formatedValue.Value;
                }
            }

            if (target != null)
            {
                Entity privateTarget = target as Entity;

                foreach (var attrib in privateTarget.Attributes)
                {
                    mergedEntity.Attributes[attrib.Key] = attrib.Value;
                }

                foreach (var formatedValue in privateTarget.FormattedValues)
                {
                    mergedEntity.FormattedValues[formatedValue.Key] = formatedValue.Value;
                }
            }
            return mergedEntity.ToEntity<T>();
        }

        public static T GetAliasedAttributeValue<T>(this Entity entity, string alias, string attributeName)
        {
            var parameterNameWithDot = string.Concat(alias, ".", attributeName);
            var parameterNameWithUnderScore = string.Concat(alias, "_", attributeName);

            if (entity != null && entity.Contains(parameterNameWithDot) && entity[parameterNameWithDot] != null)
            {
                AliasedValue aliased = (AliasedValue)entity.Attributes[parameterNameWithDot];
                return (T)aliased.Value;
            }
            else if (entity != null && entity.Contains(parameterNameWithUnderScore) && entity[parameterNameWithUnderScore] != null)
            {
                AliasedValue aliased = (AliasedValue)entity.Attributes[parameterNameWithUnderScore];
                return (T)aliased.Value;
            }
            else
            {
                return default(T);
            }
        }

        public static Entity GetAliasedEntity(this Entity entity, string alias)
        {
            Entity aliasedEntity = null;

            if (entity != null)
            {
                aliasedEntity = new Entity();
                foreach (var attrib in entity.Attributes)
                {
                    if (attrib.Key.Contains($"{alias}."))
                    {
                        aliasedEntity.Attributes[attrib.Key.Remove(0, alias.Length + 1)] = ((AliasedValue)entity.Attributes[attrib.Key]).Value;

                        if (string.IsNullOrWhiteSpace(aliasedEntity.LogicalName))
                        {
                            aliasedEntity.LogicalName = ((AliasedValue)entity.Attributes[attrib.Key]).EntityLogicalName;
                        }
                    }
                }

                foreach (var formatedValue in entity.FormattedValues)
                {
                    if (formatedValue.Key.Contains($"{alias}."))
                    {
                        aliasedEntity.FormattedValues[formatedValue.Key.Remove(0, alias.Length + 1)] = entity.FormattedValues[formatedValue.Key];
                    }
                }
            }

            return aliasedEntity;
        }

        public static EntityReference ToEntityReference(this Entity entity, string primeryAttribute)
        {
            EntityReference entityReference = entity.ToEntityReference();
            entityReference.Name = entity.GetAttributeValue<string>(primeryAttribute);

            return entityReference;
        }

        /// <summary>
        /// check if entity contains the passed attributeName key and check if it's value not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="attributeName"></param>
        /// <returns>bool</returns>
        public static bool AttributeHasValue<T>(this Entity entity, string attributeName)
        {
            bool hasValue = false;

            if (typeof(T) == typeof(string))
            {
                hasValue = entity != null && entity.Contains(attributeName) && !string.IsNullOrWhiteSpace(entity.GetAttributeValue<T>(attributeName)?.ToString());
            }
            else
            {
                hasValue = entity != null && entity.Contains(attributeName) && entity.GetAttributeValue<T>(attributeName) != null;
            }

            return hasValue;
        }

        /// <summary>
        /// convert Entity Target to Json string
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="defaultEntityValueResolver"></param>
        /// <returns></returns>
        public static string ToJson(this Entity entity, IEntityValueResolver defaultEntityValueResolver = null)
        {
            try
            {
                string entityJsonString = null;
                if (entity != null && entity.Attributes != null)
                {
                    if (defaultEntityValueResolver == null)
                    {
                        defaultEntityValueResolver = new DefaultLogEntityValueResolver();
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"{entity.LogicalName}:").Append("{").Append(Environment.NewLine);
                    if (entity.Id != Guid.Empty)
                    {
                        stringBuilder.Append($"\"Id\":\"{entity.Id}\",").Append(Environment.NewLine);
                    }
                    foreach (var attribute in entity.Attributes)
                    {
                        var attributeValue = attribute.Value is EntityReference ? defaultEntityValueResolver.GetAttributeValue(attribute.Key, entity) : $"\"{ defaultEntityValueResolver.GetAttributeValue(attribute.Key, entity)}\"";
                        stringBuilder.Append("\"").Append(attribute.Key).Append($"\":{attributeValue},").Append(Environment.NewLine);
                    }
                    stringBuilder.Append("}");
                    entityJsonString = stringBuilder.ToString();
                }
                return entityJsonString;

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string SerializeAttributes(this Entity entity)
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                dynamic expandoObject = new ExpandoObject();
                expandoObject.Id = entity.Id;
                expandoObject.LogicalName = entity.LogicalName;

                var dictionary = expandoObject as IDictionary<string, object>;

                GenerateExpandoObjectBasedOnAttributes(entity.Attributes, dictionary);

                return JsonSerializer.Serialize(expandoObject, options);
            }
            catch (Exception ex)
            {
                return $"{nameof(Exception)}: {ex.Message}";
            }
        }

        private static void GenerateExpandoObjectBasedOnAttributes(AttributeCollection attributes, IDictionary<string, object> dictionary, string key = null)
        {
            Dictionary<Type, Delegate> objectValueResolver = new Dictionary<Type, Delegate>
            {
                {typeof(EntityReference),new Func<object,ExpandoObject >(HandleEntityReferenceValue)},
                {typeof(OptionSetValue),new Func<object,int?>(CovertOptionSetValue)},
                {typeof(Money), new Func<object,decimal?>(ConvertMoney) },
                {typeof(EntityCollection), new Func<object,dynamic[]>(HandleEntityCollectionValue) }
            };
            foreach (KeyValuePair<string, object> attribute in attributes)
            {
                if (attribute.Value != null)
                {
                    Type type = attribute.Value.GetType();
                    if (objectValueResolver.ContainsKey(type))
                    {
                        var editedValue = objectValueResolver[type].DynamicInvoke(attribute.Value);
                        dictionary.Add(attribute.Key, editedValue);
                    }
                    else
                    {
                        dictionary.Add(attribute.Key, attribute.Value);
                    }
                }
                else
                {
                    dictionary.Add(attribute.Key, attribute.Value);
                }
            }
        }

        private static decimal? ConvertMoney(object value)
        {
            return ((Money)value).Value;
        }

        private static int? CovertOptionSetValue(object value)
        {
            return ((OptionSetValue)value).Value;
        }

        private static ExpandoObject HandleEntityReferenceValue(object value)
        {
            dynamic expandoObject = new ExpandoObject();
            EntityReference entityReference = (EntityReference)value;

            expandoObject.Id = entityReference.Id;
            expandoObject.LogicalName = entityReference.LogicalName;
            expandoObject.Name = entityReference.Name;

            return expandoObject;
        }

        private static dynamic[] HandleEntityCollectionValue(object entityCollectionValue)
        {
            EntityCollection entityCollection = (EntityCollection)entityCollectionValue;
            if (entityCollection.Entities.Count > 0)
            {
                var enities = new dynamic[entityCollection.Entities.Count];
                foreach (var item in entityCollection.Entities.Select((value, i) => (value, i)))
                {
                    var entity = item.value;
                    var index = item.i;
                    dynamic expandoEntity = new ExpandoObject();
                    var dictionary = expandoEntity as IDictionary<string, object>;
                    expandoEntity.Id = entity.Id;
                    expandoEntity.LogicalName = entity.LogicalName;
                    GenerateExpandoObjectBasedOnAttributes(entity.Attributes, dictionary);

                    enities[index] = expandoEntity;
                }
                return enities;
            }
            else
            {
                return null;
            }
        }

        public static List<EntityReference> GetActivityPartiesAsEntityReferences(this Entity entity, string activityPartyAttributeName)
        {
            List<EntityReference> entityReferences = new List<EntityReference>();
            EntityCollection activityParties = entity.GetAttributeValue<EntityCollection>(activityPartyAttributeName);
            if (activityParties != null)
            {
                activityParties.Entities.ToList().ForEach(party =>
                {
                    EntityReference partyId = party.GetAttributeValue<EntityReference>("partyid");
                    entityReferences.Add(partyId);
                });
            }
            return entityReferences;
        }

        public static string GetDisplayValue(
    this Entity entity,
    string attributeLogicalName,
    EntityMetadata entityMetadata)
        {
            if (entity == null || !entity.Attributes.Contains(attributeLogicalName))
            {
                return string.Empty;
            }

            object value = entity.Attributes[attributeLogicalName];

            if (value == null)
            {
                return string.Empty;
            }

            switch (value)
            {
                case OptionSetValue optionSet:
                    {
                        EnumAttributeMetadata attributeMetadata =
                            entityMetadata.Attributes
                                .FirstOrDefault(a => a.LogicalName == attributeLogicalName) as EnumAttributeMetadata;

                        OptionMetadata option =
                            attributeMetadata?.OptionSet?.Options
                                .FirstOrDefault(o => o.Value == optionSet.Value);

                        return option?.Label?.UserLocalizedLabel?.Label
                               ?? optionSet.Value.ToString();
                    }

                case EntityReference entityReference:
                    return entityReference.Name ?? string.Empty;

                case Money money:
                    return money.Value.ToString();

                case DateTime dateTime:
                    return dateTime.ToString("dd/MM/yyyy HH:mm:ss");

                case bool boolean:
                    return boolean ? "כן" : "לא";

                case string stringValue:
                    return stringValue.Replace(Environment.NewLine, " ");

                default:
                    return value.ToString();
            }
        }



        public static string GetDisplayName(
    this EntityMetadata entityMetadata,
    string attributeLogicalName)
        {
            AttributeMetadata attributeMetadata =
                entityMetadata.Attributes
                    .FirstOrDefault(a => a.LogicalName == attributeLogicalName);

            return attributeMetadata?.DisplayName?.UserLocalizedLabel?.Label
                   ?? attributeLogicalName;
        }

        public static string GetEntityDisplayName(
    this EntityMetadata entityMetadata)
        {
            return entityMetadata.DisplayName?.UserLocalizedLabel?.Label
                   ?? entityMetadata.LogicalName;
        }
        public static Entity EnrichLookups(this Entity target, Entity lookupEntity)
        {
            if (target == null || lookupEntity == null)
                return target;

            foreach (var attribute in target.Attributes)
            {
                if (!(attribute.Value is EntityReference targetReference))
                    continue;

                if (!lookupEntity.Contains(attribute.Key))
                    continue;

                var sourceReference = lookupEntity.GetAttributeValue<EntityReference>(attribute.Key);

                if (sourceReference == null)
                    continue;

                if (string.IsNullOrWhiteSpace(targetReference.LogicalName))
                {
                    targetReference.LogicalName = sourceReference.LogicalName;
                }

                if (string.IsNullOrWhiteSpace(targetReference.Name))
                {
                    targetReference.Name = sourceReference.Name;
                }
            }

            return target;
        }

    }

   

}
