using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonElement;

namespace Alt.Framework.JsonConverters
{
    public class EntityReferenceJsonConverter : JsonConverter<EntityReference>
    {
        public override EntityReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                var id = root.GetProperty("Id").GetString();
                var logicalName = root.GetProperty("LogicalName").GetString();

                ArrayEnumerator keyAttributes = root.GetProperty("KeyAttributes").EnumerateArray();
                var keyAttributesList = new KeyAttributeCollection();

                foreach (var key in keyAttributes)
                {
                    var keyName = key.GetProperty("Key").GetString();
                    var keyValue = key.GetProperty("Value").GetString();
                    keyAttributesList.Add(new KeyValuePair<string, object>(keyName, keyValue));
                }

                return new EntityReference(logicalName, Guid.Parse(id))
                {
                    KeyAttributes = keyAttributesList
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, EntityReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Id", value.Id.ToString());
            writer.WriteString("LogicalName", value.LogicalName);
            writer.WriteStartArray("KeyAttributes");
            foreach (var key in value.KeyAttributes)
            {
                writer.WriteStartObject();
                writer.WriteString("Key", key.Key);
                writer.WriteString("Value", key.Value.ToString());
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}