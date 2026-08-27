using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Alt.Framework.Extensions
{
    public static class StringExtensions
    {
        public static string ReplacePrefixAndSuffix(this String str, string prefixToRemove, string newPrefix, int suffixCharactersNumberToRemove, string newSuffix)
        {
            return newPrefix + str.Substring(prefixToRemove.Length, str.Length - suffixCharactersNumberToRemove - prefixToRemove.Length) + newSuffix;
        }

        public static string ReplacePrefixAndSuffix(this String str, string prefixToRemove, string newPrefix, string suffixToRemove, string newSuffix)
        {
            return newPrefix + str.Substring(prefixToRemove.Length, str.Length - suffixToRemove.Length - prefixToRemove.Length) + newSuffix;
        }

        public static string ReplacePrefix(this String str, string prefixToRemove, string newPrefix)
        {
            return newPrefix + str.Substring(prefixToRemove.Length);
        }

        public static List<string> GetPlaceHoldersValues(this string textToExtractFrom, string placeHolderprefix, string placeHolderSuffix)
        {
            var reg = new Regex(@"(?<open>\" + placeHolderprefix + @").*?(?<final-open>\" + placeHolderSuffix + ")");
            var matches = reg.Matches(textToExtractFrom).Cast<Match>()
                .Select(m => m.Groups["final"].Value).ToList();
            return matches;
        }

        public static string GetPadedLeftZeroString(this string textToPad, int governmentIdLength = 20)
        {
            return !string.IsNullOrWhiteSpace(textToPad) ? textToPad?.PadLeft(governmentIdLength, '0') : null;
        }

        public static string CleanPhone(this string str)
        {
            return Regex.Replace(str, @"[^\d]", String.Empty);
        }

        public static string ExtractJson(this string str)
        {
            for (var i = str.IndexOf('{'); i > -1; i = str.IndexOf('{', i + 1))
            {
                for (var j = str.LastIndexOf('}'); j > -1;)
                {
                    return str.Substring(i, j - i + 1);

                }
            }
            return string.Empty;
        }

        public static string RemoveInvalidCharactersFromFileOrDirName(this string str)
        {
            str = str.Trim();
            str = str.TrimStart('_', '.');
            str = str.Replace("~", "").
                Replace("#", "").
                Replace("%", "").
                Replace("&", "").
                Replace("*", "").
                Replace("{", "").
                Replace("}", "").
                Replace(@"\", "").
                Replace(":", "").
                Replace("<", "").
                Replace(">", "").
                Replace("?", "").
                Replace(@"/", "").
                Replace("+", "").
                Replace("|", "").
                Replace(@"""", "").
                Replace("'", "").
                Replace("׳", "").
                Replace(" ", "_");
            return str;
        }

        public static string SubstringByLength(this string text, int allowedLength)
        {
            if (!string.IsNullOrWhiteSpace(text) && text.Length > allowedLength)
            {
                text = text.Substring(0, allowedLength);
            }
            return text;
        }

        public static T TryParseValue<T>(this string stringValue)
        {
            var targetType = typeof(T);
            if (targetType != stringValue.GetType())
            {
                Type[] argTypes = { typeof(string), targetType.MakeByRefType() };
                object[] parameters = new object[] { stringValue, null };
                var tryParseMethodInfo = targetType.GetMethod("TryParse", argTypes);
                if (tryParseMethodInfo != null)
                {
                    bool successfulParse = (bool)tryParseMethodInfo.Invoke(null, parameters);
                    if (successfulParse)
                    {
                        return (T)parameters[1];
                    }
                    else
                    {
                        throw new InvalidCastException($"Can not parse ({stringValue}) to {targetType}");
                    }
                }
                else
                {
                    try
                    {
                        JsonSerializerOptions options = new JsonSerializerOptions()
                        {
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };
                        return JsonSerializer.Deserialize<T>(stringValue, options);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException($"Can not parse ({stringValue}) to {targetType}");
                    }
                }
            }
            else
            {
                return (T)(stringValue as object);
            }
        }

        public static string GetLast(this string source, int tailLength)
        {
            string result = source;
            if (tailLength < source.Length)
            {
                result = source.Substring(source.Length - tailLength);
            }
            return result;
        }

        public static Dictionary<Tkey, TValue> ToDictionary<Tkey, TValue>(this string value, JsonSerializerOptions jsonSerializerOptions = null)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                UnknownTypeHandling = System.Text.Json.Serialization.JsonUnknownTypeHandling.JsonNode
            };
            return JsonSerializer.Deserialize<Dictionary<Tkey, TValue>>(value, jsonSerializerOptions ?? options);
        }

        public static string UnwrapQuotes(this string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);

            return value;
        }

        public static string Escape(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
    }
}
