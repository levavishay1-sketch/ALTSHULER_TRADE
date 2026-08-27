using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alt.Framework.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToJson(this Dictionary<string, string> dictionary)
        {
            var entries = dictionary.Select(d =>
            string.Format("\"{0}\":\"{1}\"", d.Key, d.Value));
            return "{" + string.Join(",", entries) + "}";
        }

        public static string ToJson(this Dictionary<string, object> dictionary)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            foreach (var porperty in dictionary)
            {
                var collectionProperty = porperty.Value as List<Dictionary<string, string>>;
                if (collectionProperty != null)
                {
                    stringBuilder.Append($"\"{porperty.Key}\":");
                    stringBuilder.Append("[");
                    foreach (var innerCollection in collectionProperty)
                    {
                        stringBuilder.Append("{");
                        var entries = innerCollection.Select(d => string.Format("\"{0}\":\"{1}\"", d.Key, d.Value));
                        stringBuilder.Append(string.Join(",", entries));
                        stringBuilder.Append("},");
                    }
                    stringBuilder.Append("],");
                }
                else
                {
                    stringBuilder.Append(string.Format("\"{0}\":\"{1}\",", porperty.Key, porperty.Value));
                }
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString().Replace(",]", "]").Replace("],}", "]}").Replace(",}", "}");
        }

        public static bool TryGetSettingsItemValue<T>(this Dictionary<string, object> dictionary, string key, out T value)
        {
            bool isSucces = false;
            value = (T)(null as object);

            var settingsValue = dictionary[key];
            if (settingsValue != null)
            {
                string strValue = settingsValue.ToString();
                value = strValue.TryParseValue<T>();
            }
            isSucces = true;
            return isSucces;
        }

    }
}
