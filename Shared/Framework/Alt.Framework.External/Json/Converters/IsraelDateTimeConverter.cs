using Alt.Framework.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Alt.Framework.External.Json.Converters
{
    public class IsraelDateTimeConverter : IsoDateTimeConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var returnedDate = (DateTime?)base.ReadJson(reader, objectType, existingValue, serializer);
            if (returnedDate.HasValue && ((reader.ValueType == typeof(string) && returnedDate.Value.Kind != DateTimeKind.Utc)
                || returnedDate.Value.Kind == DateTimeKind.Unspecified))
            {
                returnedDate = returnedDate.Value.ConvertIsraelTimeToUTC();
            }
            return returnedDate;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null && value.GetType() == typeof(DateTime) && ((DateTime)value).Kind == DateTimeKind.Utc)
            {
                value = ((DateTime)value).ConvertUtcToIsraelTime();
            }
            base.WriteJson(writer, value, serializer);
        }
    }
}
