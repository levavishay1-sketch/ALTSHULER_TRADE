using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework.Logger;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.ServiceModel;

namespace Alt.Framework.EntryPoints.Crm
{
    public abstract class WorkflowBase : CodeActivity
    {
        protected class LocalContext
        {
            internal IWorkflowContext WorkfolwExecutionContext { get; private set; }
            internal IOrganizationServiceFactory OrganizationServiceFactory { get; private set; }
            private IOrganizationService OrganizationService { get; set; }
            private ITracingService TracingService { get; set; }
            private string ChildClassName { get; set; }
            private DateTime ExecutionStartTime { get; set; }

            private LocalContext() { }

            internal LocalContext(CodeActivityContext codeActivityContext, string childClassName, DateTime executionStartTime)
            {
                this.TracingService = codeActivityContext.GetExtension<ITracingService>();
                this.WorkfolwExecutionContext = codeActivityContext.GetExtension<IWorkflowContext>();
                this.OrganizationServiceFactory = codeActivityContext.GetExtension<IOrganizationServiceFactory>();
                this.OrganizationService = OrganizationServiceFactory.CreateOrganizationService(WorkfolwExecutionContext.UserId);
                this.ChildClassName = childClassName;
                this.ExecutionStartTime = executionStartTime;
            }

            private GlobalContext _globalContext = null;
            public GlobalContext ToGlobal()
            {
                if (_globalContext == null)
                {
                    var crmServiceManager = new CrmServiceManager(this.OrganizationService);
                    var log = new CrmLogger(crmServiceManager, this.TracingService, EntryPointTypeCode.Workfolw, this.WorkfolwExecutionContext.OperationCreatedOn, this.ExecutionStartTime, this.WorkfolwExecutionContext.PrimaryEntityName, this.WorkfolwExecutionContext.PrimaryEntityId.ToString(), this.ChildClassName, this.WorkfolwExecutionContext.UserId.ToString(), this.WorkfolwExecutionContext.RequestId.ToString(), this.WorkfolwExecutionContext.CorrelationId.ToString(), WorkfolwExecutionContext.Depth);
                    _globalContext = new GlobalContext(crmServiceManager, log, EntryPointTypeCode.Workfolw, this.WorkfolwExecutionContext.RequestId ?? Guid.Empty, this.WorkfolwExecutionContext.CorrelationId, null, WorkfolwExecutionContext.UserId, WorkfolwExecutionContext.InitiatingUserId, WorkfolwExecutionContext.BusinessUnitId, WorkfolwExecutionContext.Depth);
                }

                return _globalContext;
            }
        }

        protected WorkflowBase(Type childClassName)
        {
            ChildClassName = childClassName.ToString();
        }

        protected string ChildClassName { get; private set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            LocalContext localcontext = new LocalContext(executionContext, ChildClassName, DateTime.UtcNow);
            GlobalContext globalContext = localcontext.ToGlobal();
            globalContext.Log.Info($"Entered {this.ChildClassName}.Execute()");

            try
            {
                ExecuteCrmWorkflow(localcontext);

                return;
            }
            catch (InvalidPluginExecutionException ipex)
            {
                if (localcontext.WorkfolwExecutionContext.Depth == 1)
                {
                    globalContext.Log.Warning(ipex);
                }
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
                    if (localcontext.WorkfolwExecutionContext.Depth == 1 || localcontext.WorkfolwExecutionContext.Mode == 1) //mode 1 = async mode 0 = sync
                    {
                        globalContext.Log.Warning(fe);
                    }
                }
                else
                {
                    globalContext.Log.Critical(fe);
                }

                throw;
            }
            catch (Exception ex)
            {
                if (localcontext.WorkfolwExecutionContext.Depth == 1 || localcontext.WorkfolwExecutionContext.Mode == 1) //mode 1 = async mode 0 = sync
                {
                    globalContext.Log.Critical(ex);
                }
                throw;
            }
            finally
            {
                globalContext.Log.Info($"Exiting {this.ChildClassName}.Execute()");
                globalContext.Log.Execute();
            }
        }

        protected abstract void ExecuteCrmWorkflow(LocalContext localcontext);
    }
}
