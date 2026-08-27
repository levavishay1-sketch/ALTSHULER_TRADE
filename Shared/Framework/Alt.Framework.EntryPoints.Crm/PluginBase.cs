using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework.Extensions;
using Alt.Framework.Logger;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.ServiceModel;

namespace Alt.Framework.EntryPoints.Crm
{
    public abstract class PluginBase : IPlugin
    {
        protected class LocalContext
        {
            protected IServiceProvider localContextServiceProvider;
            public IPluginExecutionContext PluginExecutionContext { get; private set; }
            public IOrganizationServiceFactory OrganizationServiceFactory { get; private set; }
            public IServiceEndpointNotificationService NotificationService { get; private set; }
            private IOrganizationService OrganizationService { get; set; }
            private ITracingService TracingService { get; set; }
            public Entity TargetEntity { get; private set; }
            public Entity PreEntity { get; private set; }
            public Entity PostEntity { get; private set; }
            private string ChildClassName { get; set; }
            private DateTime ExecutionStartTime { get; set; }

            private GlobalContext _globalContext = null;

            private LocalContext() { }
            internal LocalContext(IServiceProvider serviceProvider, string childClassName, bool runByCallingUser, DateTime executionStartTime)
            {
                if (serviceProvider == null)
                {
                    throw new InvalidPluginExecutionException("serviceProvider");
                }
                localContextServiceProvider = serviceProvider;

                ChildClassName = childClassName;

                ExecutionStartTime = executionStartTime;

                PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

                OrganizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                OrganizationService = runByCallingUser ?
                    OrganizationServiceFactory.CreateOrganizationService(PluginExecutionContext.UserId)
                    : OrganizationService = OrganizationServiceFactory.CreateOrganizationService(null);

                TargetEntity = (PluginExecutionContext.InputParameters.Contains("Target") &&
                                        PluginExecutionContext.InputParameters["Target"] is Entity)
                                        ? PluginExecutionContext.InputParameters["Target"] as Entity
                                        : null;

                PreEntity = (PluginExecutionContext.PreEntityImages != null &&
                                    PluginExecutionContext.PreEntityImages.Contains("PreImage"))
                                    ? PluginExecutionContext.PreEntityImages["PreImage"]
                                    : null;

                PostEntity = (PluginExecutionContext.PostEntityImages != null &&
                                     PluginExecutionContext.PostEntityImages.Contains("PostImage"))
                                     ? PluginExecutionContext.PostEntityImages["PostImage"]
                                     : null;
            }

            public IOrganizationService GetSystemProxy()
            {
                return OrganizationServiceFactory.CreateOrganizationService(null);
            }
            public GlobalContext ToGlobal()
            {
                if (_globalContext == null)
                {
                    var crmServiceManager = new CrmServiceManager(OrganizationService);
                    var log = new CrmLogger(crmServiceManager, TracingService, EntryPointTypeCode.Plugin, PluginExecutionContext.OperationCreatedOn, ExecutionStartTime, PluginExecutionContext.PrimaryEntityName, PluginExecutionContext.PrimaryEntityId.ToString(), ChildClassName, PluginExecutionContext.UserId.ToString(), PluginExecutionContext.RequestId.ToString(), PluginExecutionContext.CorrelationId.ToString(), PluginExecutionContext.Depth);
                    _globalContext = new GlobalContext(crmServiceManager, log, EntryPointTypeCode.Workfolw, PluginExecutionContext);
                    _globalContext.ExecuteCloudServiceFunc = ExecuteCloudService;
                }
                return _globalContext;
            }
            
            public T GetEntityDataSourceRetrieverService<T>() where T : IEntityDataSourceRetrieverService
            {
                return localContextServiceProvider.Get<T>();
            }

            public string ExecuteCloudService(Guid serviceEndpointId)
            {
                IServiceEndpointNotificationService cloudService = NotificationService;

                if (cloudService == null)
                {
                    throw new InvalidPluginExecutionException("Failed to retrieve the service bus service.");

                }
                return cloudService.Execute(new EntityReference("serviceendpoint", serviceEndpointId), PluginExecutionContext);
            }
        }

        protected string ChildClassName { get; private set; }
        protected bool RunByCallingUser { get; private set; }

        protected PluginBase(Type childClassName, bool runByCallingUser = true)
        {
            ChildClassName = childClassName.ToString();
            RunByCallingUser = runByCallingUser;
        }
       
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException("serviceProvider");
            }
            DateTime executionStartTime = DateTime.UtcNow;
            LocalContext localcontext = new LocalContext(serviceProvider, ChildClassName, RunByCallingUser, executionStartTime);

            string recordIdMessage = localcontext.PluginExecutionContext.PrimaryEntityId != Guid.Empty
             ? $" PrimaryEntityId: {localcontext.PluginExecutionContext.PrimaryEntityId}"
             : string.Empty;

            string traceMessage = $"Entered {ChildClassName}.Execute(Entity: {localcontext.PluginExecutionContext.PrimaryEntityName} {recordIdMessage} , Message: {localcontext.PluginExecutionContext.MessageName}, Stage: {localcontext.PluginExecutionContext.Stage})";

            GlobalContext globalContext = localcontext.ToGlobal();
            globalContext.Log.Info(traceMessage);

            try
            {
                string targetEntityMessage = localcontext.TargetEntity != null ? $"TargetEntity : {localcontext.TargetEntity?.ToJson()}" : string.Empty;
                string preEntityMessage = localcontext.PreEntity != null ? $"{Environment.NewLine}PreEntity : {localcontext.PreEntity?.ToJson()}" : string.Empty;
                string postEntityMessage = localcontext.PostEntity != null ? $"{Environment.NewLine}PostEntity: {localcontext.PostEntity?.ToJson()}" : string.Empty;

                if (!string.IsNullOrWhiteSpace(targetEntityMessage) || !string.IsNullOrWhiteSpace(preEntityMessage) || !string.IsNullOrWhiteSpace(postEntityMessage))
                {
                    globalContext.Log.Info($"{targetEntityMessage}{preEntityMessage}{postEntityMessage}");
                }

                ExecuteCrmPlugin(localcontext);

                return;
            }
            catch (InvalidPluginExecutionException ipex)
            {
                globalContext.Log.Warning(ipex);
                throw;
            }
            catch (FaultException<OrganizationServiceFault> fe)
            {

                if (fe.Detail != null
                       && fe.Detail != null
                       && fe.Detail.ErrorDetails != null
                       && fe.Detail.ErrorDetails.Contains("SubErrorCode")
                       && CustomErrorCodes.ContainsCode((int)fe.Detail.ErrorDetails["SubErrorCode"]))
                {
                    if (localcontext.PluginExecutionContext.Depth == 1 || localcontext.PluginExecutionContext.Mode == 1) //mode 1 = async mode 0 = sync
                    {
                        globalContext.Log.Warning(fe);
                    }
                }
                else
                {
                    string targetEntityMessage = localcontext.TargetEntity != null ? $"TargetEntity : {localcontext.TargetEntity?.ToJson()}" : string.Empty;
                    globalContext.Log.Critical(fe);
                    globalContext.Log.Critical($"target on criticat exception, {targetEntityMessage}");
                }

                throw;
            }
            catch (Exception ex)
            {
                if (localcontext.PluginExecutionContext.Depth == 1 || localcontext.PluginExecutionContext.Mode == 1) //mode 1 = async mode 0 = sync
                {
                    string targetEntityMessage = localcontext.TargetEntity != null ? $"TargetEntity : {localcontext.TargetEntity?.ToJson()}" : string.Empty;
                    globalContext.Log.Critical(ex);
                    globalContext.Log.Critical($"target on critical exception, {targetEntityMessage}");
                }
                throw;
            }
            finally
            {
                globalContext.Log.Info($"Exiting {ChildClassName}.Execute()");
                globalContext.Log.Execute();
            }
        }

        protected abstract void ExecuteCrmPlugin(LocalContext localContext);
    }
}
