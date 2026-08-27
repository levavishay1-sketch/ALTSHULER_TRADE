using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using Alt.DataModel.Crm.Core.Enums;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Linq;

namespace Alt.Framework.External.WebJobs
{
    public class GlobalOutgoingBusinessLogicProducer<TApiEntity, TBusinessLogic> : IBusinessLogicStrategy
        where TApiEntity : ApiEntityBase
        where TBusinessLogic : ICrmOutgoing<TApiEntity>
    {
        public ICrmOutgoing<TApiEntity> OutgoingBusinessLogic { get; set; }
        public GlobalContext GlobalContext { get; set; }
        public string MessageName { get; private set; }
        public ApiContext<TApiEntity> ApiEntityContext { get; set; }

        public GlobalOutgoingBusinessLogicProducer(GlobalContext globalContext, object remoteContext)
        {
            this.GlobalContext = globalContext;
            this.InitializeByRemoteContext(remoteContext);
            this.OutgoingBusinessLogic = this.CreateCrmOutgoingBusinessLogic();
        }

        private void InitializeByRemoteContext(object remoteContext)
        {
            this.GlobalContext.LogEntry();
            Dictionary<Type, Delegate> initializationsByRemoteContext = new Dictionary<Type, Delegate>
            {
                {typeof(ServiceBusCustomMessage),new Action<ServiceBusCustomMessage>(InitializeByServiseBusCustomMessage)},
                {typeof(RemoteExecutionContext),new Action<RemoteExecutionContext>(InitializeByRemoteExecutionContext)},
            };

            initializationsByRemoteContext[remoteContext.GetType()].DynamicInvoke(remoteContext);
        }

        private void InitializeByServiseBusCustomMessage(ServiceBusCustomMessage serviceBusMessage)
        {
            this.GlobalContext.LogEntry();

            var targetEntity = JsonSerializer.Deserialize<TApiEntity>(serviceBusMessage.Body);
            this.MessageName = serviceBusMessage.ActionType.ToLower();
            this.InitializeApiContext(null, targetEntity);
            this.SetApiConfigurationCode(null, targetEntity);
        }

        private void InitializeByRemoteExecutionContext(RemoteExecutionContext remoteExecutionContext)
        {
            this.GlobalContext.LogEntry();

            this.MessageName = remoteExecutionContext.MessageName.ToLower();
            this.InitializeApiContext(remoteExecutionContext);
            this.SetApiConfigurationCode(remoteExecutionContext.SharedVariables);
        }

        private void SetApiConfigurationCode(ParameterCollection sharedVariables, TApiEntity apiEntity = null)
        {
            this.GlobalContext.LogEntry();

            string variableName = nameof(ApiConfigurationCode);
            if (sharedVariables != null && sharedVariables.ContainsKey(variableName))
            {
                this.GlobalContext.ApiConfigurationCode = sharedVariables[variableName] != null ?
                    (int?)sharedVariables[variableName] : null;
            }
            else if (apiEntity?.ApiConfigurationCode != null)
            {
                this.GlobalContext.ApiConfigurationCode = apiEntity.ApiConfigurationCode;
            }
            else
            {
                this.GlobalContext.ApiConfigurationCode = this.ApiEntityContext.Target?.ApiConfigurationCode;
            }
        }

        private void InitializeApiContext(RemoteExecutionContext remoteContext, TApiEntity apiEntity = null)
        {
            this.GlobalContext.LogEntry();

            if (remoteContext != null)
            {
                ParameterCollection inputParameters = remoteContext.InputParameters;
                bool containsTarget = inputParameters.Contains("Target");
                var target = containsTarget ? inputParameters["Target"] as Entity
                                        : new Entity(remoteContext.PrimaryEntityName, remoteContext.PrimaryEntityId);

                var preImage = (remoteContext.PreEntityImages != null &&
                                       remoteContext.PreEntityImages.Contains("PreImage"))
                                       ? remoteContext.PreEntityImages["PreImage"]
                                       : null;

                var postImage = (remoteContext.PostEntityImages != null &&
                                     remoteContext.PostEntityImages.Contains("PostImage"))
                                     ? remoteContext.PostEntityImages["PostImage"]
                                     : null;

                this.WriteRemoteContextToLog(target, preImage, postImage);
                this.ApiEntityContext = new ApiContext<TApiEntity>(target, preImage, postImage, this.MessageName, containsTarget);
            }
            else if (apiEntity != null)
            {
                this.ApiEntityContext = new ApiContext<TApiEntity>(apiEntity, this.MessageName);
            }
        }

        private void WriteRemoteContextToLog(Entity target, Entity preImage, Entity postImage)
        {
            try
            {
                List<string> logMessage = new List<string>();
                logMessage.Add($"{Environment.NewLine}\"TargetEntity\": {target?.SerializeAttributes()}");
                logMessage.Add(
                    preImage != null ?
                    $"{Environment.NewLine}\"PreEntity\": {preImage?.SerializeAttributes()}" : null);
                logMessage.Add(
                    postImage != null ?
                    $"{Environment.NewLine}\"PostEntity\": {postImage?.SerializeAttributes()}" : null);

                this.GlobalContext.Log.Info($"{Environment.NewLine}{{{string.Join(",", logMessage.Where(i => !string.IsNullOrWhiteSpace(i)))}{Environment.NewLine}}}");
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Warning($"Error whilte printing target: {ex.Message}");
            }
        }

        public override string ToString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(this, options);
        }

        public ICrmOutgoing<TApiEntity> CreateCrmOutgoingBusinessLogic()
        {
            this.GlobalContext.LogEntry();
            return (TBusinessLogic)Activator.CreateInstance(typeof(TBusinessLogic), this.GlobalContext);
        }

        public virtual ActionResult ExecuteBusinessLogicByEntityMessage()
        {
            return this.OutgoingBusinessLogic.ExecuteOutgoingLogicHandler(this.ApiEntityContext);
        }
    }
}
