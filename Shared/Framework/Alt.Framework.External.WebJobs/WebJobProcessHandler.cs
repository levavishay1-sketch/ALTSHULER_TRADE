using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.EntryPoints.External;
using Alt.Framework.External.WebJobs.Extensions;
using Azure.Messaging.ServiceBus;
using System;

namespace Alt.Framework.External.WebJobs
{
    public class WebJobProcessHandler
    {
        public string PrimaryEntityLogicalName { get; private set; }

        public ThirdPartyBase ThirdPartyBase { get; private set; }

        public object RemoteContext { get; private set; }

        private readonly ThirdPartyTracingService tracingService = new ThirdPartyTracingService();

        public WebJobProcessHandler(ProcessMessageEventArgs args, Type type)
        {
            this.Initialize(args, type);
            this.TraceReceivedMessage(args.Message.SequenceNumber);
        }

        private void Initialize(ProcessMessageEventArgs args, Type type)
        {
            switch (args.Message.ContentType)
            {
                case "application/msbin1":
                    {
                        this.HandleMSBin1ContentType(args, type);
                        break;
                    }
                default:
                    {
                        this.InitializeBasedOnServiceBusMessageType(args, type);
                        break;
                    }
            }
        }

        private void InitializeBasedOnServiceBusMessageType(ProcessMessageEventArgs args, Type type)
        {
            string key = nameof(ServiceBusMessageType);
            if (args.Message.ApplicationProperties.ContainsKey(key)
                && int.TryParse(args.Message.ApplicationProperties[key].ToString(), out int value)
                && Enum.IsDefined(typeof(ServiceBusMessageType), value))
            {
                ServiceBusMessageType serviceBusMessageType = (ServiceBusMessageType)value;
                switch (serviceBusMessageType)
                {
                    case ServiceBusMessageType.ServiceBusCustomMessage:
                        {
                            this.HandleServiceBusCustomMessage(args, type);
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException($"Not Implemented Logic for Service Bus Message Type ({serviceBusMessageType})");
                        }
                }
            }
            else
            {
                throw new NotImplementedException($"Not Implemented Logic for Service Bus Message Type. SequenceNumber ({args.Message.SequenceNumber})");
            }
        }

        private void HandleServiceBusCustomMessage(ProcessMessageEventArgs args, Type type)
        {
            var serviceBusCustomMessage = System.Text.Json.JsonSerializer.Deserialize<ServiceBusCustomMessage>(args.Message.Body.ToString());

            this.PrimaryEntityLogicalName = serviceBusCustomMessage.PrimaryEntityName;
            this.RemoteContext = serviceBusCustomMessage;
            this.ThirdPartyBase = ExternalEntryPointManager.Connect(type, serviceBusCustomMessage);
            this.ThirdPartyBase.GlobalContext.Log.Info($"{Environment.NewLine}{args.Message.Body}{Environment.NewLine}");
        }

        private void HandleMSBin1ContentType(ProcessMessageEventArgs args, Type type)
        {
            var remoteExecutionContext = args.Message.GetBody();

            this.PrimaryEntityLogicalName = remoteExecutionContext.PrimaryEntityName;
            this.RemoteContext = remoteExecutionContext;
            this.ThirdPartyBase = ExternalEntryPointManager.Connect(type, remoteExecutionContext);
        }

        private void TraceReceivedMessage(long sequenceNumber)
        {
            tracingService.Trace($"Received message: SequenceNumber={sequenceNumber} PrimaryEntityName={this.PrimaryEntityLogicalName}");
        }
    }
}
