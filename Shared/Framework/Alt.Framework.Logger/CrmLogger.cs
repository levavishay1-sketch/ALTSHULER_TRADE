using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Alt.Framework.Logger
{
    public class CrmLogger : LoggerBase
    {
        private ICrmServiceManager crmServiceManager;
        private IOrganizationService organizationService;
        private ITracingService tracingService;
        private int depth;
        private string correlationId;
        private DateTime executionTime;
        private DateTime operationTime;
        protected string executingSystemUserId;
        private static DateTime? latestLogLevelRetrieveDate = null;
        private readonly int latestLogLevelRetrieveLifeTime = 5;
        private object executionLockbOject = new object();
        private static MessageLevel crmLevelToLog = MessageLevel.Information;

        public CrmLogger(ICrmServiceManager crmServiceManager, ITracingService tracingService, EntryPointTypeCode entryPointType, DateTime operationTime, DateTime executionTime, string primaryEntityName, string primaryEntityId, string className, string executingSystemUserId, string requestId = "", string correlationId = "", int depth = 0, MessageLevel levelToLog = MessageLevel.Information)
            : base(entryPointType, className, requestId, levelToLog, primaryEntityName, primaryEntityId)
        {
            this.crmServiceManager = crmServiceManager;
            this.organizationService = this.crmServiceManager.GetService();
            this.tracingService = tracingService;
            this.operationTime = operationTime;
            this.executionTime = executionTime;
            this.correlationId = correlationId;
            this.depth = depth;
            this.executingSystemUserId = executingSystemUserId;
        }

        public override void Execute()
        {
            this.HandleLogLevel();
            if (logMessageBuilder != null && logMessageBuilder.Length > 0 && this.level >= crmLevelToLog)
            {
                string logMessageStr = logMessageBuilder.ToString();

                tracingService.Trace(logMessageStr.Replace("{", "{{").Replace("}", "}}"));
                DateTime operationEndTime = DateTime.UtcNow;
                TimeSpan performanceExecutionDuration = operationEndTime - this.executionTime;
                TimeSpan operationDuration = operationEndTime - this.operationTime;

                try
                {
                    OrganizationRequest request = new OrganizationRequest("alt_Logger");
                    request["MessageBlock"] = logMessageStr;
                    request["MessageLevelCode"] = (int)level;
                    request["EntryPointTypeCode"] = (int)entryPointType;
                    request["OverrideCreatedOn"] = executionTime;
                    request["Name"] = className;
                    request["ExecutingSystemUserId"] = executingSystemUserId;
                    request["RequestId"] = requestId;
                    request["CorrelationId"] = correlationId;
                    request["Depth"] = depth;
                    request["PerformanceExecutionDuration"] = (int)performanceExecutionDuration.TotalMilliseconds;
                    request["OperationDuration"] = (int)operationDuration.TotalMilliseconds;
                    request["TargetLogicalName"] = this.primaryEntityName;
                    request["TargetId"] = this.primaryEntityId;

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

                    var response = this.organizationService.Execute(executeMultipleRequest);
                }
                catch (Exception ex)
                {
                    this.tracingService.Trace(ex.ToString());
                }
            }
        }

        private void HandleLogLevel()
        {
            if (latestLogLevelRetrieveDate == null)
            {
                lock (executionLockbOject)
                {
                    if (latestLogLevelRetrieveDate == null)
                    {
                        this.SetLogLevel();
                        latestLogLevelRetrieveDate = DateTime.UtcNow;
                    }
                }
            }

            TimeSpan time = DateTime.UtcNow - latestLogLevelRetrieveDate.Value;
            if (time.TotalMinutes >= latestLogLevelRetrieveLifeTime)
            {
                this.SetLogLevel();
                lock (executionLockbOject)
                {
                    latestLogLevelRetrieveDate = DateTime.UtcNow;
                }
            }
        }


        private void SetLogLevel()
        {
            try
            {
                string retrievedLevelToLog = this.GetLogLevel();
                if (!string.IsNullOrWhiteSpace(retrievedLevelToLog)
                    && Enum.TryParse(retrievedLevelToLog, true, out MessageLevel messageLevel))
                {
                    crmLevelToLog = messageLevel;
                }
                else
                {
                    this.Critical("Environment variable alt_LogLevel not exist or contains invalid value.");
                }
            }
            catch (Exception ex)
            {
                this.Critical(ex);
            }                
        }

        private string GetLogLevel()
        {
            QueryExpression query = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("defaultvalue", "valueschema", "schemaname", "environmentvariabledefinitionid", "type"),
                NoLock = true,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("schemaname", ConditionOperator.Equal, "alt_LogLevel")
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        JoinOperator = JoinOperator.LeftOuter,
                        LinkFromEntityName = "environmentvariabledefinition",
                        LinkFromAttributeName ="environmentvariabledefinitionid",
                        LinkToEntityName = "environmentvariablevalue",
                        LinkToAttributeName = "environmentvariabledefinitionid",
                        Columns = new ColumnSet("value", "environmentvariablevalueid"),
                        EntityAlias = "variable"
                    }
                }
            };
            var result = this.organizationService.RetrieveMultiple(query)?.Entities?.FirstOrDefault();

            return  result?.GetAttributeValue<AliasedValue>("variable.value")?.Value?.ToString()
                ?? result?.GetAttributeValue<string>("defaultvalue");
        }
    }
}
