using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Alt.Framework.External.Extensions
{
    public static class JsonStringExtensions
    {
        public static string ReplaceContentPropertiesValue(this string content, List<string> tokenNamesToIgnore, string valueForReplace = null)
        {
            string requestBodyContent = content;
            if (content != null)
            {
                var jsonObj = JObject.Parse(requestBodyContent);
                jsonObj.ReplaceJTokenValues(tokenNamesToIgnore, valueForReplace);
                requestBodyContent = jsonObj.ToString();
            }       
            return requestBodyContent;
        }

        public static string RemoveBodyContentProperty(this string bodyContent, string propertiesToRemoveString)
        {
            string requestBodyContent = bodyContent;

            var jsonObj = JObject.Parse(bodyContent);
            var propertiesToRemove = propertiesToRemoveString.Split(',').Select(s => s.Trim()).ToHashSet();
            var propertiesToRemoveList = jsonObj.Descendants().OfType<JProperty>().Where(attr => propertiesToRemove.Contains(attr.Name.ToLower())).Select(attr => attr.Name).ToList();

            foreach (var property in propertiesToRemoveList)
            {
                if (jsonObj.ContainsKey(property))
                {
                    jsonObj.Remove(property);
                }
            }
            requestBodyContent = jsonObj.ToString();

            return requestBodyContent;
        }

        public static dynamic ToDinamic(this string jsonString)
        {
            dynamic obj = JsonConvert.DeserializeObject<List<ExpandoObject>>(jsonString,new ExpandoObjectConverter());
            return obj;
        }
    }
}
