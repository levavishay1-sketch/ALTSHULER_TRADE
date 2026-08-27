using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework.EntryPoints.External;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Framework.External.WebJobs
{
    public class ExternalEntryPointManager
    {
        private static readonly ConnectionQueue connectionQueue = new ConnectionQueue();

        public static ThirdPartyBase Connect(Type type, RemoteExecutionContext context)
        {
            string customTitle = $"{context.PrimaryEntityName}-{context.MessageName}";
            return Connect(type, context.RequestId, customTitle, context.PrimaryEntityName, context.PrimaryEntityId);         
        }

        public static ThirdPartyBase Connect(Type type, ServiceBusCustomMessage serviceBusMessage)
        {
            string customTitle = $"{serviceBusMessage.PrimaryEntityName}-{serviceBusMessage.ActionType}";
            return Connect(type, serviceBusMessage.RequestId, customTitle, serviceBusMessage.PrimaryEntityName, serviceBusMessage.PrimaryEntityId);
        }

        public static ThirdPartyBase Connect(Type type, Guid? requestId, string customTitle, string primaryEntityName, Guid? primaryEntityId)
        {
            var crmService = connectionQueue.GetConnection();
            if (crmService != null)
            {
                return new ThirdPartyBase(crmService, type, requestId, customTitle, primaryEntityName, primaryEntityId);
            }
            else
            {
                throw new Exception("Can Not Initialize or Get CRM Connection");
            }
        }
    }
}
