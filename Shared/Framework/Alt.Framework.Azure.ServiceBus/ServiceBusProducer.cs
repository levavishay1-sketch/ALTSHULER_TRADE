using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alt.Framework.Azure.ServiceBus
{
    public class ServiceBusProducer
    {
        string queueName;
        public ServiceBusProducer(string queueName)
        {
            this.queueName = queueName;
        }

        public async Task SendMessageAsync(string message)
        {
            ServiceBusSender sender = ServiceBusClientCache.Instance.GetServiceBusSenderInstance(this.queueName);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            serviceBusMessage.ApplicationProperties.Add(nameof(ServiceBusMessageType), (int)ServiceBusMessageType.ServiceBusCustomMessage);

            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(ServiceBusCustomMessage serviceBusMessage)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {                
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var message = JsonSerializer.Serialize(serviceBusMessage, options);
            await SendMessageAsync(message).ConfigureAwait(false);
        }

        public ActionResult SendMessageSync(ServiceBusCustomMessage serviceBusMessage)
        {
            ActionResult actionResult = new ActionResult();
            var result = Task.Run(() => this.SendMessageAsync(serviceBusMessage).GetAwaiter().GetResult());
            result.Wait();
            if (!result.IsCompleted || result.IsFaulted || result.IsCanceled)
            {
                actionResult.SetToFailedActionResult($"Redirect Request Failed");
            }

            return actionResult;
        }

        public async Task SendMessagesAsync(List<string> messages)
        {
            ServiceBusSender sender = ServiceBusClientCache.Instance.GetServiceBusSenderInstance(this.queueName);

            ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            foreach (var message in messages)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(message)))
                {
                    throw new Exception($"Send Message to Service Bus Failed.");
                }
            }
            await sender.SendMessagesAsync(messageBatch);
        }

        public async Task SendMessagesAsync(List<ServiceBusCustomMessage> serviceBusMessages)
        {
            List<string> messages = serviceBusMessages.Select(s => JsonSerializer.Serialize(s)).ToList();
            await this.SendMessagesAsync(messages);
        }

    }
}
