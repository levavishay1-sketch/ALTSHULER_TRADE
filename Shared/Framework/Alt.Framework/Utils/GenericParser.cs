using System;

namespace Alt.Framework.Utils
{
    public class GenericParser
    {
        public Nullable<T> TryParseProperty<T>(string propertyValue) where T : struct
        {
            T? castedValue = null;
            if (!string.IsNullOrEmpty(propertyValue))
            {
                var targetType = typeof(T);
                Type[] argTypes = { typeof(string), targetType.MakeByRefType() };
                object[] parameters = new object[] { propertyValue, null };
                var tryParseMethodInfo = targetType.GetMethod("TryParse", argTypes);
                if (tryParseMethodInfo != null)
                {
                    bool successfulParse = (bool)tryParseMethodInfo.Invoke(null, parameters);
                    if (successfulParse)
                    {
                        castedValue = (Nullable<T>)parameters[1];
                    }
                }
            }
            if (castedValue == null)
            {
                if (typeof(T) == typeof(bool))
                {
                    castedValue = (!string.IsNullOrEmpty(propertyValue) && (propertyValue == "1" || propertyValue?.ToLower() == "y")) ?
                                        true as T? : false as T?;
                }
            }
            return castedValue;
        }
    }
}
