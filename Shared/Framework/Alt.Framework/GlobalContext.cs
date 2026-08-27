using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Interfaces;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.Cache;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Alt.Framework
{
    public class GlobalContext
    {
        private int depth = 0;
        private Guid userId = Guid.Empty;
        private Guid initiatingUserId;
        private Guid businessUnitId = Guid.Empty;
        private WhoAmIResponse whoAmIResponse = null;
        private CrmServiceContext serviceContext = null;
        private string organizationUrl = null;
        public CacheManager CacheManager { get; set; } = null;

        public IExecutionContext ExecutionContext { get; set; } = null;

        public Func<Guid, string> ExecuteCloudServiceFunc { get; set; }

        public IOrganizationService OrganizationService
        {
            get
            {
                return this.ServiceManager.GetService();
            }
        }
        protected ICrmServiceManager ServiceManager { get; private set; }
        public EntryPointTypeCode EntryPointType { get; private set; }
        public Guid RequestId { get; private set; }
        public Guid CorrelationId { get; private set; }
        public Guid UserId
        {
            get
            {
                if (this.userId.Equals(Guid.Empty))
                {
                    this.userId = GetWhoAmIResponse().UserId;
                }

                return this.userId;
            }
        }
        public Guid InitiatingUserId
        {
            get
            {
                if (this.initiatingUserId.Equals(Guid.Empty))
                {
                    this.initiatingUserId = this.UserId;
                }

                return this.initiatingUserId;
            }

        }
        public Guid BusinessUnitId
        {
            get
            {
                if (this.businessUnitId.Equals(Guid.Empty))
                {
                    this.businessUnitId = GetWhoAmIResponse().BusinessUnitId;
                }

                return this.businessUnitId;
            }

        }
        public int Depth
        {
            get { return this.depth; }

        }
        public CrmServiceContext Context
        {
            get
            {
                if (this.serviceContext == null)
                {
                    this.serviceContext = new CrmServiceContext(this.OrganizationService);
                }

                return this.serviceContext;
            }
        }
        public ILog Log { get; private set; }
        public string OrganizationUrl
        {
            get
            {

                return this.organizationUrl;
            }
        }
        public int? ApiConfigurationCode { get; set; }
        public string Content { get; set; }
        public ConcurrentDictionary<string, object> EnvironmentVariables
        {
            get
            {
                return EnvironmentVariablesCache.Instance.GetEnvironmentVariables(OrganizationService, Log);
            }
        }

        public GlobalContext(GlobalContext globalContext)
        {
            this.ServiceManager = globalContext.ServiceManager;
            this.Log = globalContext.Log;
            this.EntryPointType = globalContext.EntryPointType;
            this.userId = globalContext.userId;
            this.initiatingUserId = globalContext.initiatingUserId;
            this.businessUnitId = globalContext.businessUnitId;
            this.depth = globalContext.depth;
            this.RequestId = globalContext.RequestId;
            this.CorrelationId = globalContext.CorrelationId;
            this.organizationUrl = globalContext.organizationUrl;
            this.ApiConfigurationCode = globalContext.ApiConfigurationCode;
            this.Content = globalContext.Content;
            this.CacheManager = new CacheManager(this);
            this.ExecutionContext = globalContext.ExecutionContext;
        }

        public GlobalContext(ICrmServiceManager serviceManage, ILog log, EntryPointTypeCode entryPointType, Guid requestId, Guid correlationId, string organizationUrl)
        {
            this.ServiceManager = serviceManage;
            this.Log = log;
            this.EntryPointType = entryPointType;
            this.RequestId = requestId;
            this.organizationUrl = organizationUrl;
            this.CorrelationId = correlationId;
            this.CacheManager = new CacheManager(this);
        }

        public GlobalContext(ICrmServiceManager serviceManage, ILog log, EntryPointTypeCode entryPointType, Guid requestId, Guid correlationId, string organizationUrl, Guid userId)
            : this(serviceManage, log, entryPointType, requestId, correlationId, organizationUrl)
        {
            this.userId = userId;
        }

        public GlobalContext(ICrmServiceManager serviceManage, ILog log, EntryPointTypeCode entryPointType, Guid requestId, Guid correlationId, string organizationUrl, Guid userId, Guid businessUnitId) : this(serviceManage, log, entryPointType, requestId, correlationId, organizationUrl, userId)
        {
            this.businessUnitId = businessUnitId;
        }

        public GlobalContext(ICrmServiceManager serviceManage, ILog log, EntryPointTypeCode entryPointType, Guid requestId, Guid correlationId, string organizationUrl, Guid userId, Guid initiatingUserId, Guid businessUnitId, int depth) : this(serviceManage, log, entryPointType, requestId, correlationId, organizationUrl, userId)
        {
            this.initiatingUserId = initiatingUserId;
            this.businessUnitId = businessUnitId;
            this.depth = depth;
        }

        public GlobalContext(ICrmServiceManager serviceManager, ILog log, EntryPointTypeCode entryPointType, IPluginExecutionContext pluginExecutionContext) :
            this(serviceManager, log, entryPointType, pluginExecutionContext.RequestId ?? Guid.Empty, pluginExecutionContext.CorrelationId, null, pluginExecutionContext.UserId, pluginExecutionContext.InitiatingUserId, pluginExecutionContext.BusinessUnitId, pluginExecutionContext.Depth)
        {
            this.SetCustomApiInputParameters(pluginExecutionContext);
            this.ExecutionContext = pluginExecutionContext;
        }

        private void SetCustomApiInputParameters(IPluginExecutionContext pluginExecutionContext)
        {
            if (pluginExecutionContext.InputParameters != null)
            {
                if (pluginExecutionContext.InputParameters.ContainsKey("Content"))
                {
                    this.Content = (string)pluginExecutionContext.InputParameters["Content"];
                }
                if (pluginExecutionContext.InputParameters.ContainsKey("ApiConfigurationCode"))
                {
                    this.ApiConfigurationCode = (int)pluginExecutionContext.InputParameters["ApiConfigurationCode"];
                }
            }
        }

        public void LogEntry(string message = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {

            this.Log.Info($"Enter ⇒ {message}", sourceFilePath, memberName, sourceLineNumber);
        }

        private WhoAmIResponse GetWhoAmIResponse()
        {
            if (whoAmIResponse == null)
            {
                whoAmIResponse = ((WhoAmIResponse)OrganizationService.Execute(new WhoAmIRequest()));
            }

            return whoAmIResponse;

        }

        public DateTime GetIsraelLocalDateTimeFromUtc(DateTime dateTimeUtc)
        {
            this.LogEntry();
            DateTime dateTimeLocal = dateTimeUtc;

            if (dateTimeUtc != default(DateTime) && dateTimeUtc.Kind == DateTimeKind.Utc)
            {
                var request = new LocalTimeFromUtcTimeRequest
                {
                    TimeZoneCode = 135,
                    UtcTime = dateTimeUtc
                };

                LocalTimeFromUtcTimeResponse response = (LocalTimeFromUtcTimeResponse)OrganizationService.Execute(request);
                dateTimeLocal = response.LocalTime;
            }

            return dateTimeLocal;
        }

        public EntityMetadata GetEntityMetadata(string entityName)
        {
            return this.OrganizationService.GetEntityMetadata(entityName);
        }

        public string SendMessageToParsedInRelay(string parserSettings, string serviceEndpoint)
        {
            if (this.ExecutionContext.SharedVariables.ContainsKey("ParserSettings"))
            {
                this.ExecutionContext.SharedVariables.Remove("ParserSettings");
            }
            this.ExecutionContext.SharedVariables.Add("ParserSettings", parserSettings);
            Guid serviceEndpointId = new Guid(serviceEndpoint);
            return ExecuteCloudServiceFunc(serviceEndpointId);
        }
    }
}
