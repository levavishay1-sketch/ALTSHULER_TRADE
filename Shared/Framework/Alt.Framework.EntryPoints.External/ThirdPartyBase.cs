using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Logger;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace Alt.Framework.EntryPoints.External
{
    public class ThirdPartyBase : IDisposable
    {
        private bool disposedValue = false;
        public GlobalContext GlobalContext { get; private set; }
        public Guid CallerId { get; set; }
        protected string ChildClassName { get; private set; }

        public ThirdPartyBase(Type childClassName, string crmConnectionString, Guid? requestId = null, string customTitle = null, string primaryEntityName = null, Guid? primaryEntityId = null)
        {
            ChildClassName = childClassName.ToString();
            this.InitializeConnection(crmConnectionString, requestId, customTitle, primaryEntityName, primaryEntityId?.ToString());
        }

        public ThirdPartyBase(CrmServiceManager crmServiceManager,Type childClassName, Guid? requestId = null, string customTitle = null, string primaryEntityName = null, Guid? primaryEntityId = null)
        {
            ChildClassName = childClassName.ToString();
            this.InitializeConnection(crmServiceManager, requestId, customTitle, primaryEntityName, primaryEntityId?.ToString());
        }

        private void InitializeConnection(CrmServiceManager crmServiceManager, Guid? requestId = null, string customTitle = null, string primaryEntityName = null, string primaryEntityId = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            DateTime operationStartTime = DateTime.UtcNow;
            Guid correctRequestId = requestId ?? Guid.NewGuid();
            Guid correlationId = Guid.NewGuid();
            ITracingService traceService = new ThirdPartyTracingService();
            MessageLevel levelToLog = this.GetLogLevel();
            string organizationUrl = this.GetOrganizationUrl(crmServiceManager.CrmConnectionString);
            string logTitle = this.GetLogTitle(customTitle);

            var log = new ThirdPartyLogger(crmServiceManager, traceService, EntryPointTypeCode.ThirdParty, operationStartTime, DateTime.UtcNow, logTitle,
                requestId: correctRequestId.ToString(),
                correlationId: correlationId.ToString(),
                levelToLog: levelToLog,
                primaryEntityName: primaryEntityName,
                primaryEntityId: primaryEntityId);

            GlobalContext = new GlobalContext(crmServiceManager, log, EntryPointTypeCode.ThirdParty, correctRequestId, correlationId, organizationUrl.TrimEnd('/'), Guid.Empty);
            GlobalContext.Log.Info($"Entered {ChildClassName}.Execute()");
        }

        private void InitializeConnection(string crmConnectionString, Guid? requestId = null, string customTitle = null, string primaryEntityName = null, string primaryEntityId = null)
        {
            CrmServiceManager crmServiceManager = new CrmServiceManager(crmConnectionString);
            this.InitializeConnection(crmServiceManager, requestId, customTitle, primaryEntityName, primaryEntityId);
        }

        private string GetLogTitle(string customTitle)
        {
            string logTitle = ChildClassName;
            if (!string.IsNullOrWhiteSpace(customTitle))
            {
                logTitle += $" - {customTitle}";
            }
            return logTitle;
        }

        private string GetOrganizationUrl(string crmConnectionString)
        {
            string key = "url";
            Dictionary<string, string> connStringParts = this.GetConnectionStringParts(crmConnectionString);
            return connStringParts.ContainsKey(key) ? connStringParts[key].ToString() : null;
        }

        private MessageLevel GetLogLevel()
        {
            MessageLevel levelToLog = MessageLevel.Information;
            if (int.TryParse(ConfigurationManager.AppSettings["levelToLog"], out int levelFromConfig))
            {
                levelToLog = (MessageLevel)levelFromConfig;
            }
            return levelToLog;
        }

        private Dictionary<string, string> GetConnectionStringParts(string connectionString)
        {
            return connectionString.Split(';')
                    .Select(t => t.Split(new char[] { '=' }, 2))
                    .ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    GlobalContext.Log.Info($"Exiting {ChildClassName}.Execute()");
                    GlobalContext.Log.Execute();
                }
                disposedValue = true;
            }
        }

        ~ThirdPartyBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
