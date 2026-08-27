using System;
using System.IO;
using System.Xml.Serialization;

namespace Alt.Framework.Utils
{
    public static class XmlUtils<TXml>
    {
        public static TXml DeserializeXml<TReader>(string textInputForReader, Func<string, TReader> readerInitializer) where TReader : TextReader
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TXml));

            using (TextReader reader = readerInitializer(textInputForReader))
            {
                return (TXml)serializer.Deserialize(reader);
            }
        }
    }
}
