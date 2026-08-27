using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Alt.Framework.Utils
{
    public static class JsonUtils
    {
        public static string Serialize<T>(T sourceObject)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                DataContractJsonSerializer serlializer = new DataContractJsonSerializer(typeof(T), settings);
                serlializer.WriteObject(memoryStream, sourceObject);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static T Deserialize<T>(string json)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;
                T responseObject = (T)serializer.ReadObject(stream);
                return responseObject;
            }
        }
    }
}
