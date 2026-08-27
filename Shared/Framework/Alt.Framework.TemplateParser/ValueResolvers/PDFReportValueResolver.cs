using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Alt.Framework.TemplateParser.ValueResolvers
{
    public class PDFReportValueResolver : IEntityValueResolver
    {
        public string GetAttributeValue(string attributeName, Entity entity)
        {
            string result = null;
            object att;
            if (entity.Contains(attributeName) && entity[attributeName] != null)
            {
                var entityAttribute = entity[attributeName];
                if (entityAttribute.GetType().Name == "AliasedValue")
                    att = ((AliasedValue)entityAttribute).Value;
                else
                    att = entityAttribute;
                switch (att.GetType().Name.ToLower())
                {
                    case "string":
                        {
                            string str = att.ToString().Replace("\"","\\\"");
                            result = ReverseOnlyHebrew(str);
                            break;
                        }
                    case "int":
                    case "int32":
                    case "long":
                        {
                            result = att.ToString();
                            break;
                        }
                    case "boolean":
                        {
                            result = ((bool)att) == true ? "1" : "0";
                            break;
                        }
                    case "decimal":
                    case "double":
                        {
                            var valueAsDouble = double.Parse(att.ToString());
                            result = valueAsDouble.ToString("0.00");
                            break;
                        }
                    case "entityreference":
                        {
                            result = ((EntityReference)att).Name;
                            break;
                        }
                    case "optionsetvalue":
                        {
                            result = ((OptionSetValue)att).Value.ToString();
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

        public object GetAttributeValueForOrderBy(string attributeName, Entity entity)
        {
            object result = null;
            object att;
            if (entity.Contains(attributeName) && entity[attributeName] != null)
            {
                var entityAttribute = entity[attributeName];
                if (entityAttribute.GetType().Name == "AliasedValue")
                    att = ((AliasedValue)entityAttribute).Value;
                else
                    att = entityAttribute;
                switch (att.GetType().Name.ToLower())
                {
                    case "string":
                        {
                            result = att.ToString();
                            break;
                        }
                    case "int":
                    case "int32":
                    case "long":
                        {
                            result = att;
                            break;
                        }
                    case "boolean":
                        {
                            result = ((bool)att);
                            break;
                        }
                    case "decimal":
                    case "double":
                        {
                            result  = double.Parse(att.ToString());
                            break;
                        }
                    case "entityreference":
                        {
                            result = ((EntityReference)att).Name;
                            break;
                        }
                    case "optionsetvalue":
                        {
                            result = ((OptionSetValue)att).Value;
                            break;
                        }
                    case "money":
                        {
                            result = ((Money)att).Value;
                            break;
                        }
                    case "datetime":
                        {
                            result = this.GetIsraelLocalDateTimeFromUtc((DateTime)att);
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

        public string ReverseOnlyHebrew(string stringToReverse)
        {
            StringBuilder reversedText = null;
            var firstHebChar = System.Text.ASCIIEncoding.Default.GetBytes(new[] { (char)1488 }).FirstOrDefault(); //א
            var lastHebChar = System.Text.ASCIIEncoding.Default.GetBytes(new[] { (char)1514 }).FirstOrDefault(); //ת
            if (!string.IsNullOrWhiteSpace(stringToReverse))
            {
                reversedText = new StringBuilder();
                string[] arrSplit = Regex.Split(stringToReverse, "( )|([א-ת]+)");
                int arrlenth = arrSplit.Length - 1;
                for (int i = arrlenth; i >= 0; i--)
                {
                    if (arrSplit[i] == " ")
                    {
                        reversedText.Append(" ");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(arrSplit[i]))
                        {
                            arrSplit[i] = arrSplit[i].Trim();
                            byte[] codes = System.Text.ASCIIEncoding.Default.GetBytes(arrSplit[i].ToCharArray(), 0, 1);
                            if (codes[0] >= firstHebChar && codes[0] <= lastHebChar) // is Hebrew character
                            {
                                reversedText.Append(Reverse(arrSplit[i]));
                            }
                            else
                            {
                                reversedText.Append(arrSplit[i].Trim());
                            }
                        }
                    }
                }
            }

            return reversedText != null ? reversedText.ToString() : null;
        }

        public string Reverse(string stringToReverse)
        {
            string reversedText = stringToReverse;
            if (!string.IsNullOrWhiteSpace(stringToReverse))
            {
                char[] charArray = stringToReverse.ToCharArray();
                Array.Reverse(charArray);
                reversedText = new string(charArray);
            }
            return reversedText;
        }
    }
}
