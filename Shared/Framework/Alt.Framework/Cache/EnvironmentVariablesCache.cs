using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;

namespace Alt.Framework.Cache
{
    public sealed class EnvironmentVariablesCache
    {
        private ConcurrentDictionary<string, object> envioronmentVariables;
        private DateTime lastRetrieveDate;
        private int? cachePeriodInMinutes = 10;
        private static object lockObject = new object();

        private static readonly Lazy<EnvironmentVariablesCache> lazy = new Lazy<EnvironmentVariablesCache>(() => new EnvironmentVariablesCache());
        public static EnvironmentVariablesCache Instance { get { return lazy.Value; } }

        private EnvironmentVariablesCache()
        {
            this.envioronmentVariables = new ConcurrentDictionary<string, object>();
        }

        public ConcurrentDictionary<string, object> GetEnvironmentVariables(IOrganizationService service, ILog log)
        {
            log.Info($"Get environment variables request. Last retrieve date {this.lastRetrieveDate}. Current cache period in minutes ({this.cachePeriodInMinutes}).");
            if (this.envioronmentVariables == null || this.IsNeedToUpdateCache())
            {
                lock (lockObject)
                {
                    if (this.envioronmentVariables == null || this.IsNeedToUpdateCache())
                    {
                        string cashPeriodVariableName = "alt_EnvVariablesCachePeriodInMinutes";
                        log.Info("Retrive environment variables executing...");
                        this.envioronmentVariables = this.RetrieveEnvironmentVariables(service);
                        this.lastRetrieveDate = DateTime.UtcNow;

                        if (int.TryParse(this.GetEnvironmentVariable(cashPeriodVariableName), out int result))
                        {
                            this.cachePeriodInMinutes = result;
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException($"Falid to parse {cashPeriodVariableName} environment variable value.");
                        }
                    }
                }
            }
            else
            {
                log.Info("Using environment variables from cache.");
            }
            return this.envioronmentVariables;
        }

        public string GetEnvironmentVariable(IOrganizationService service, ILog log, string schemaName)
        {
            this.GetEnvironmentVariables(service, log);
            return this.GetEnvironmentVariable(schemaName);
        }

        private string GetEnvironmentVariable(string schemaName)
        {
            if (this.envioronmentVariables.TryGetValue(schemaName, out object value))
            {
                return value?.ToString();
            }
            else
            {
                throw new InvalidPluginExecutionException($"{schemaName} environment variable not found.");
            }
        }

        private ConcurrentDictionary<string, object> RetrieveEnvironmentVariables(IOrganizationService service)
        {
            ConcurrentDictionary<string, object> variables = new ConcurrentDictionary<string, object>();
            QueryExpression query = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("defaultvalue", "valueschema", "schemaname", "environmentvariabledefinitionid", "type"),
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
            var result = service.RetrieveMultiple(query);
            if (result?.Entities.Count > 0)
            {
                foreach (var entity in result.Entities)
                {
                    string schemaName = entity.GetAttributeValue<string>("schemaname");
                    var value = entity.GetAttributeValue<AliasedValue>("variable.value")?.Value;
                    var defaultValue = entity.GetAttributeValue<string>("defaultvalue");

                    if (!string.IsNullOrEmpty(schemaName)
                        && !variables.ContainsKey(schemaName))
                    {
                        variables.TryAdd(schemaName, value ?? defaultValue);
                    }
                }
            }
            return variables;
        }

        private bool IsNeedToUpdateCache()
        {
            return this.cachePeriodInMinutes == null
                || this.lastRetrieveDate.AddMinutes((int)this.cachePeriodInMinutes) <= DateTime.UtcNow;
        }
    }
}
