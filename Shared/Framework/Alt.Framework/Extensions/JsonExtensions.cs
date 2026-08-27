using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alt.Framework.Extensions
{
    public static class JsonExtensions
    {
        public static void DeepInsert(this JsonObject obj, string path, JsonNode value)
        {
            var current = obj;
            var pathParts = path.Split('.');

            for (int i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                if (i != (pathParts.Length - 1))
                {
                    var innerObject = current[part];
                    if (innerObject == null)
                    {
                        innerObject = new JsonObject();
                        current[part] = innerObject;
                        current = (JsonObject)innerObject;
                    }
                    else
                    {
                        current = (JsonObject)innerObject;
                    }
                }
                else
                {
                    current[part] = value;
                }
            }
        }

        public static Dictionary<string, JsonElement> Flatten(this JsonElement obj, string previousPath = null)
        {
            var result = new Dictionary<string, JsonElement>();

            foreach (var prop in obj.EnumerateObject())
            {
                var currentPropName = previousPath != null ? $"{previousPath}.{prop.Name}" : prop.Name;
                result.Add(currentPropName, prop.Value);

                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    var innerDictionary = prop.Value.Flatten(currentPropName);

                    foreach (var inner in innerDictionary)
                    {
                        result.Add(inner.Key, inner.Value);
                    }
                }
            }

            return result;
        }
    }
}
