using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alt.Framework.External.Json.Converters
{
    public class NullableIntToStringConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetInt32();
                case JsonTokenType.String:
                    string stringValue = reader.GetString();
                    if (int.TryParse(stringValue, out int parsedValue))
                    {
                        return parsedValue;
                    }
                    return null;
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new JsonException($"Unexpected JSON token '{reader.TokenType}' when parsing int?");
            }
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            switch (value.HasValue)
            {
                case true:
                    writer.WriteNumberValue(value.Value);
                    break;
                case false:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}
