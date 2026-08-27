using System.Collections.Generic;
using System.Dynamic;

namespace Alt.Framework.Utils
{
    public class ExpandoObjectUtils
    {
        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }
            else
            {
                expandoDict.Add(propertyName, propertyValue);
            }
        }
    }
}
