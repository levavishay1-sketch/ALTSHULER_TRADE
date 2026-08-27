using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.TemplateParser.Interfaces;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Framework.TemplateParser.ValueResolvers
{
    public class DefaultEntityValueResolver : IEntityValueResolver
    {
        public string GetAttributeValue(string attributeName, Entity entity)
        {
            string result = null;
            if (entity.Contains(attributeName) && entity[attributeName] != null)
            {
                object att;
                var entityAttribute = entity[attributeName];
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
                    case "guid":
                        {
                            result = att.ToString();
                            break;
                        }
                    case "entityreference":
                        {
                            result = ((EntityReference)att).Name;
                            break;
                        }
                    case "optionsetvalue":
                    case "boolean":
                        {
                            result = entity.FormattedValues[attributeName].ToString();
                            break;
                        }
                    case "money":
                        {
                            result = (((Money)att).Value.ToString().Split('.')[1].StartsWith("0")) ? ((Money)att).Value.ToString().Split('.')[0] : ((Money)att).Value.ToString();
                            break;
                        }
                    case "datetime":
                        {
                            result = this.GetIsraelLocalDateTimeFromUtc((DateTime)att).ToString("dd/MM/yyyy");
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
