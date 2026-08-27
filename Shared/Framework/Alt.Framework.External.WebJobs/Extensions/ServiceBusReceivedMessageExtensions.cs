using Azure.Messaging.ServiceBus;
using Microsoft.Xrm.Sdk;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Alt.Framework.External.WebJobs.Extensions
{
    public static class ServiceBusReceivedMessageExtensions
    {
        public static RemoteExecutionContext GetBody(this ServiceBusReceivedMessage message)
        {
            RemoteExecutionContext context;
            var bodyAsByteArray = message.Body.ToArray();

            DataContractSerializer serializer = new DataContractSerializer(typeof(RemoteExecutionContext));
            using (var memoryStream = new MemoryStream(bodyAsByteArray.Length))
            {
                memoryStream.Write(bodyAsByteArray, 0, bodyAsByteArray.Length);
                memoryStream.Flush();
                memoryStream.Position = 0;
                context = (RemoteExecutionContext)serializer
                    .ReadObject(XmlDictionaryReader.CreateBinaryReader(memoryStream, XmlDictionaryReaderQuotas.Max));
            }
            return context;
        }
    }
}
