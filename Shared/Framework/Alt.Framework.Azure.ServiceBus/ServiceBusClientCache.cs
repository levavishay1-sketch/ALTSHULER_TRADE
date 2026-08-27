using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Concurrent;
using System.Configuration;

namespace Alt.Framework.Azure.ServiceBus
{
    public sealed class ServiceBusClientCache
    {
        private readonly static string serviceBusHostName = ConfigurationManager.AppSettings["ServiceBusHostName"];

        private ServiceBusClient serviceBusClient;
        private ConcurrentDictionary<string, ServiceBusSender> serviceBusSenders = new ConcurrentDictionary<string, ServiceBusSender>();

        private readonly static object lockObject = new object();
        public static ServiceBusClientCache Instance { get { return lazy.Value; } }

        private static readonly Lazy<ServiceBusClientCache> lazy = new Lazy<ServiceBusClientCache>(() => new ServiceBusClientCache());

        private ServiceBusClient GetClient(ServiceBusClientOptions clientOptions = null)
        {
            clientOptions = clientOptions ?? new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            if (serviceBusClient == null || serviceBusClient.IsClosed)
            {
                serviceBusClient = new ServiceBusClient(serviceBusHostName, new DefaultAzureCredential(), clientOptions);
            }

            return serviceBusClient;
        }

        public ServiceBusClient GetClientInstance(ServiceBusClientOptions clientOptions = null)
        {
            if (serviceBusClient == null || serviceBusClient.IsClosed)
            {
                lock (lockObject)
                {
                    this.GetClient(clientOptions);
                }
            }

            return serviceBusClient;
        }


        public ServiceBusSender GetServiceBusSenderInstance(string queueName, ServiceBusClientOptions clientOptions = null)
        {
            if (!serviceBusSenders.ContainsKey(queueName) || serviceBusSenders[queueName].IsClosed)
            {
                lock (lockObject)
                {
                    if (!serviceBusSenders.ContainsKey(queueName) || serviceBusSenders[queueName].IsClosed)
                    {
                        var serviceBusClient = this.GetClient(clientOptions);
                        serviceBusSenders[queueName] = serviceBusClient.CreateSender(queueName);
                    }
                }
            }

            return serviceBusSenders[queueName];
        }
    }
}
