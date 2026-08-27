using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Crm.Actions.Logger
{
    public class Logger : IPlugin
    {
        private int levelToWriteLog;
        private int maxMessageLength;
        public Logger(string unsecure, string secure)
        {
            if (string.IsNullOrEmpty(unsecure) || !int.TryParse(unsecure, out maxMessageLength))
            {
                throw new InvalidPluginExecutionException("MaxMessageLength unsecure param is null or empty ");
            }

            if (string.IsNullOrEmpty(secure) || !int.TryParse(secure, out levelToWriteLog))
            {
                throw new InvalidPluginExecutionException("Level to write exeption does not exist in secure param");
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext localContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);

            tracingService.Trace($"secure level:{levelToWriteLog}, action level:{localContext.InputParameters["MessageLevelCode"]?.ToString()}");
            tracingService.Trace($"unsecure MaxMessageLength : {maxMessageLength}");
            Entity systemLog = new Entity("alt_systemlog");
            try
            {
                int level = (int)localContext.InputParameters["MessageLevelCode"];
                if (level >= levelToWriteLog)
                {
                    systemLog["alt_entrypointtypecode"] = localContext.InputParameters["EntryPointTypeCode"] != null ? new OptionSetValue((int)localContext.InputParameters["EntryPointTypeCode"]) : null;
                    systemLog["overriddencreatedon"] = localContext.InputParameters["OverrideCreatedOn"] != null ? (DateTime?)localContext.InputParameters["OverrideCreatedOn"] : null;
                    systemLog["alt_name"] = localContext.InputParameters["Name"];
                    systemLog["alt_executingsystemuserid"] = !string.IsNullOrWhiteSpace(localContext.InputParameters["ExecutingSystemUserId"].ToString()) ? new EntityReference("systemuser", new Guid(localContext.InputParameters["ExecutingSystemUserId"].ToString())) : null;
                    systemLog["alt_requestid"] = !string.IsNullOrWhiteSpace(localContext.InputParameters["RequestId"].ToString()) ? localContext.InputParameters["RequestId"] : Guid.Empty.ToString();
                    systemLog["alt_correlationid"] = !string.IsNullOrWhiteSpace(localContext.InputParameters["CorrelationId"].ToString()) ? localContext.InputParameters["CorrelationId"] : Guid.Empty.ToString();
                    systemLog["alt_depth"] = (int)localContext.InputParameters["Depth"];
                    systemLog["alt_performanceexecutionduration"] = (int)localContext.InputParameters["PerformanceExecutionDuration"];
                    systemLog["alt_messagelevelcode"] = new OptionSetValue(level);
                    systemLog["alt_operationduration"] = (int)localContext.InputParameters["OperationDuration"];
                    systemLog["alt_targetlogicalname"] = (string)localContext.InputParameters["TargetLogicalName"];
                    systemLog["alt_targetid"] = (string)localContext.InputParameters["TargetId"];

                    string messageBlock = localContext.InputParameters["MessageBlock"]?.ToString();
                    systemLog["alt_messageblock"] = !string.IsNullOrWhiteSpace(messageBlock) ? (messageBlock.Length > maxMessageLength ? messageBlock.Substring(0, maxMessageLength) : messageBlock) : null;

                    service.Create(systemLog);
                }
            }
            catch (Exception ex)
            {
                string traceExceptionMessage = "\n\nSystem Log fields:\n";

                foreach (var attribute in systemLog.Attributes)
                {
                    if (attribute.Key != "alt_messageblock")
                    {
                        string attributeName = attribute.Key.Replace("alt_", "");
                        var attributeValue = attribute.Value;
                        if (attributeValue is OptionSetValue)
                        {
                            attributeValue = ((OptionSetValue)attributeValue).Value;
                        }
                        else if (attributeValue is EntityReference)
                        {
                            attributeValue = ((EntityReference)attributeValue).Id;
                        }
                        traceExceptionMessage += $"{attributeName}: {attributeValue},   ";
                    }
                }
                traceExceptionMessage += $"\nMESSAGE BLOCK:: \n{systemLog["alt_messageblock"].ToString().Replace("{", "{{").Replace("}", "}}")}\n\n";

                tracingService.Trace(traceExceptionMessage);
                throw ex;
            }
        }
    }
}
