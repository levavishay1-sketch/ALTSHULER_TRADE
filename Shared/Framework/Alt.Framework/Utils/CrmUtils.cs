using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.Utils
{
    public static class CrmUtils
    {
        public static string GetAttributeValueAsString(object value)
        {
            switch (value.GetType().Name.ToLower())
            {
                case "string":
                case "int":
                case "int32":
                case "long":
                case "decimal":
                case "double":
                    {
                        return value.ToString();
                    }
                case "entityreference":
                    {
                        return ((EntityReference)value).Name;
                    }
                case "boolean":
                    {
                        return (bool)value ? "כן" : "לא";
                    }
                case "money":
                    return ((Money)value).Value.ToString(); // (((Money)value).Value.ToString().Split('.')[1].StartsWith("0")) ? ((Money)value).Value.ToString().Split('.')[0] : ((Money)value).Value.ToString();
                case "datetime":
                    return ((DateTime)value).ConvertUtcToIsraelTime().ToString("dd/MM/yyyy");
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
