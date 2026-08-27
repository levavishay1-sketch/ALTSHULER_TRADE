using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Text;

namespace Alt.Framework.Logger
{
    public class DefaultLogEntityValueResolver : IEntityValueResolver
    {
        public string GetAttributeValue(string attributeName, Entity entity)
        {
            string result = null;
            object att;
            var entityAttribute = entity[attributeName];
            if (entityAttribute != null)
            {
                if (entityAttribute.GetType().Name == "AliasedValue")
                    att = ((AliasedValue)entityAttribute).Value;
                else
                    att = entityAttribute;
                switch (att.GetType().Name.ToLower())
                {
                    case "string":
                    case "int":
                    case "int32":
                    case "long":
                    case "decimal":
                    case "double":
                        {
                            result = att.ToString();
                            break;
                        }
                    case "entityreference":
                        {
                            var entityReference = (EntityReference)att;
                            var textBuilder = new StringBuilder().Append("{").Append(Environment.NewLine).Append($"\"Id\" : \"{entityReference.Id}\",").Append(Environment.NewLine)
                                .Append($"\"LogicalName\" : \"{entityReference.LogicalName}\"");
                            if (entityReference.KeyAttributes != null && entityReference.KeyAttributes.Count > 0)
                            {
                                textBuilder.Append(",").Append(Environment.NewLine).Append($"{entityReference.KeyAttributes.Keys.FirstOrDefault()} : {entityReference.KeyAttributes[entityReference.KeyAttributes.Keys.FirstOrDefault()]} ").Append(Environment.NewLine);
                            }
                            textBuilder.Append(Environment.NewLine).Append("}");
                            result = textBuilder.ToString();
                            break;
                        }
                    case "optionsetvalue":
                        {

                            result = (entity[attributeName] as OptionSetValue)?.Value.ToString();
                            break;
                        }
                    case "boolean":
                        {
                            result = entity[attributeName].ToString();
                            break;
                        }
                    case "money":
                        {
                            result = ((Money)att).Value.ToString(); 
                            break;
                        }
                    case "datetime":
                        {
                            result = GetIsraelLocalDateTimeFromUtc((DateTime)att).ToString();
                            break;
                        }
                    default:
                        break;
                }
            }
            return result;

        }

        private DateTime GetIsraelLocalDateTimeFromUtc(DateTime dateTimeUtc)
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, cstZone);
        }
    }
}
