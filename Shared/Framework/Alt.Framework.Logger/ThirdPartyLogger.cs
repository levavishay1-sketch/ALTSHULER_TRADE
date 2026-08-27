using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Alt.Framework.Logger
{
    public class ThirdPartyLogger : LoggerBase
    {
        const int defaultLogBuilderCapacity = 50000;
        const int defaultLogLifeTimeInSeconds = 300;
        ICrmServiceManager crmServiceManager;
        private IOrganizationService organizationService;
        private ITracingService tracingService;
        private int depth;
        private string correlationId;
        private DateTime executionTime;
        private DateTime operationTime;
        private object executionLockbOject = new object();
        private DateTime createdOn;
        private Retry retry = new Retry();

        /// <summary>
        /// Flush log when time reached
        /// </summary>
        private readonly int LogLifeTime = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings?["LogLifeTimeInSeconds"]) ? int.Parse(ConfigurationManager.AppSettings["LogLifeTimeInSeconds"]) : defaultLogLifeTimeInSeconds;

        /// <summary>
        /// Flush log when quantity reached
        /// </summary>
        private readonly int logBuilderCapacity = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings?["LogCapacity"]) ? int.Parse(ConfigurationManager.AppSettings["LogCapacity"]) : defaultLogBuilderCapacity;

        public ThirdPartyLogger(ICrmServiceManager crmServiceManager, ITracingService tracingService, EntryPointTypeCode entryPointType, DateTime operationTime, DateTime executionTime, string className, string requestId = "", string correlationId = "", int depth = 0, MessageLevel levelToLog = MessageLevel.Information, string primaryEntityName = null, string primaryEntityId = null)
            : base(entryPointType, className, requestId, levelToLog, primaryEntityName, primaryEntityId)
        {
            this.crmServiceManager = crmServiceManager;
            this.organizationService = this.crmServiceManager.GetService();
            this.tracingService = tracingService;
            this.operationTime = operationTime;
            this.executionTime = executionTime;
            this.correlationId = correlationId;
            this.depth = depth;
            this.createdOn = executionTime;
        }

        public override void Execute()
        {
            lock (executionLockbOject)
            {
                if (logMessageBuilder != null && logMessageBuilder.Length > 0 && this.level >= this.levelToLog)
                {
                    OrganizationRequest request = new OrganizationRequest("alt_Logger");
                    string logMessageStr = logMessageBuilder.ToString();
                    tracingService.Trace(logMessageStr.Replace("{", "{{").Replace("}", "}}"));
                    DateTime operationEndTime = DateTime.UtcNow;
                    TimeSpan performanceExecutionDuration = operationEndTime - this.executionTime;
                    TimeSpan operationDuration = operationEndTime - this.operationTime;

                    try
                    {
                        request["MessageBlock"] = logMessageStr;
                        request["MessageLevelCode"] = (int)level;
                        request["EntryPointTypeCode"] = (int)entryPointType;
                        request["OverrideCreatedOn"] = createdOn;
                        request["Name"] = className;
                        request["ExecutingSystemUserId"] = ((WhoAmIResponse)organizationService.Execute(new WhoAmIRequest())).UserId.ToString();
                        request["RequestId"] = requestId;
                        request["CorrelationId"] = correlationId;
                        request["Depth"] = depth;
                        request["PerformanceExecutionDuration"] = (int)performanceExecutionDuration.TotalMilliseconds;
                        request["OperationDuration"] = (int)operationDuration.TotalMilliseconds;
                        request["TargetLogicalName"] = base.primaryEntityName;
                        request["TargetId"] = base.primaryEntityId;

                        ExecuteMultipleRequest executeMultipleRequest = new ExecuteMultipleRequest()
                        {
                            Requests = new OrganizationRequestCollection(),
                            Settings = new ExecuteMultipleSettings()
                            {
                                ContinueOnError = true,
                                ReturnResponses = true
                            }
                        };
                        executeMultipleRequest.Requests.Add(request);

                        Func<OrganizationRequest, OrganizationResponse> execute = (r) =>
                        {
                            var organizationService = this.crmServiceManager.GetService();
                            return organizationService.Execute(r);
                        };
                        this.retry.Do(executeMultipleRequest, execute);
                    }
                    catch (Exception ex)
                    {
                        this.tracingService.Trace(ex.ToString());
                    }
                    finally
                    {
                        WriteToApplicationInsights(request.Parameters);
                        logMessageBuilder.Clear();
                        this.level = MessageLevel.Information;
                    }
                }
            }
        }

        protected override void WriteMessage(string message, MessageLevel level, string sourceFilePath, string memberName, int sourceLineNumber)
        {
            lock (executionLockbOject)
            {
                try
                {
                    string messageToLog = $"[{DateTime.UtcNow.ToString("O")} UTC]  [{ this.GetMessageLevelText(level)}]  {Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName} ({sourceLineNumber}) :: {message}";

                    if (logMessageBuilder.Length + messageToLog.Length >= logBuilderCapacity || this.IsLogLifeTimeOver())
                    {
                        this.Execute();
                        this.createdOn = DateTime.UtcNow;
                    }

                    if (level > this.level)
                    {
                        this.level = level;
                    }

                    logMessageBuilder.AppendLine(messageToLog);

                }
                catch (Exception ex)
                {
                    this.Critical(ex);
                    this.Execute();
                }
            }

        }

        private string GetMessageLevelText(MessageLevel level)
        {
            string messageLevelStr = "INFO";
            switch (level)
            {
                case MessageLevel.Information:
                    {
                        messageLevelStr = "INFO";
                        break;
                    }
                case MessageLevel.Warning:
                    {
                        messageLevelStr = "WARN";
                        break;
                    }
                case MessageLevel.Error:
                    {
                        messageLevelStr = "ERROR";
                        break;
                    }
                case MessageLevel.Critical:
                    {
                        messageLevelStr = "CRITICAL";
                        break;
                    }
            }
            return messageLevelStr;
        }

        private bool IsLogLifeTimeOver()
        {
            TimeSpan time = DateTime.UtcNow - this.createdOn;
            if (time.TotalSeconds >= LogLifeTime)
            {
                return true;
            }
            return false;
        }

        protected void WriteToApplicationInsights(ParameterCollection parameterCollection)
        {
            if (parameterCollection != null && 
                bool.TryParse(ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_TRACE"], out bool isTrace)
                 && isTrace)
            {
                var parameters = new KeyValuePair<string, object>[parameterCollection.Count];
                parameterCollection.CopyTo(parameters, 0);
                var customDimensions = parameters.ToDictionary(x => x.Key, x => x.Value?.ToString());

                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.ConnectionString = ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"];

                TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
                telemetryClient.TrackTrace(this.className, (SeverityLevel)this.level, customDimensions);
            }
        }
    }
}
