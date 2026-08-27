using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Alt.Framework.Cache
{
    public sealed class CacheManager
    {
        private static ConcurrentDictionary<string, CacheItem> cachedItems = null;
        private static object executionLockbOject = new object();

        GlobalContext globalContext;
        public CacheManager(GlobalContext globalContext)
        {
            this.globalContext = globalContext;
        }

        static CacheManager()
        {
            cachedItems = new ConcurrentDictionary<string, CacheItem>();
        }

        public T GetCachedItem<T>(string key, Func<T> retrieveCallback, int? retrieveLifeTime = 5)
        {
            this.globalContext?.LogEntry($"Key : {key}");
            if (!cachedItems.ContainsKey(key) || IsCacheItemExpired(key))
            {
                lock (executionLockbOject)
                {
                    if (!cachedItems.ContainsKey(key) || IsCacheItemExpired(key))
                    {
                        CacheItem cacheItem = RetrieveCacheItem<T>(key, retrieveLifeTime, retrieveCallback);
                        TryAddCacheItem(key, cacheItem);
                    }
                    else
                    {
                        this.globalContext.Log.Info($"Using item from cache.");
                    }
                }
            }
            return (T)cachedItems[key].Value;
        }

        public Entity GetApiConfiguration(int apiConfigurationCode)
        {
            this.globalContext.LogEntry();
            string keyPreffix = "ApiConfiguration";
            string envVariableName = "alt_ApiConfigurationCacheLifeTimeInMinutes";

            return this.GetCachedItem<Entity>($"{keyPreffix}({apiConfigurationCode})", () =>
            {
                return this.GetApiConfigurationByCode(apiConfigurationCode);
            }, 
            this.GetCacheItemLifeTime(envVariableName));
        }

        public List<Entity> GetApiConfigurationsByRoute(string routePath)
        {
            this.globalContext.LogEntry();
            string keyPreffix = "ApiConfiguration";
            string envVariableName = "alt_ApiConfigurationCacheLifeTimeInMinutes";

            return this.GetCachedItem<List<Entity>>($"{keyPreffix}({routePath})", () =>
            {
                return this.GetActiveApiConfigurationsByRoute(routePath);
            },
            this.GetCacheItemLifeTime(envVariableName));
        }

        private List<Entity> GetActiveApiConfigurationsByRoute(string routePath)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "alt_apiconfiguration",
                ColumnSet = new ColumnSet(true),
                NoLock = true,
                Criteria =
                   {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("alt_url", ConditionOperator.Equal, routePath),
                            new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                        }
                   }
            };

            var entity = this.globalContext.OrganizationService.RetrieveMultiple(query).Entities.ToList();

            return entity != null ? entity
                : throw new Exception($"Default Incoming Api Configuration for Route ({routePath}) Not Found");
        }

        public T GetGlobalParameter<T>(string parameterName)
        {
            this.globalContext.LogEntry();
            string keyPreffix = "GlobalParameter";
            string envVariableName = "alt_GlobalParameterCacheLifeTimeInMinutes";
            string value = this.GetCachedItem<string>($"{keyPreffix}({parameterName})", () =>
            {
                return this.GetGlobalParameterByName(parameterName);
            }, this.GetCacheItemLifeTime(envVariableName));

            return value.TryParseValue<T>();
        }

        public string GetEnvironmentVariable(string variableSchemaName)
        {
            this.globalContext.LogEntry();
            string keyPreffix = "EnvironmentVariable";
            string envVariableName = "alt_EnvVariableCacheLifeTimeInMinutes";
            int cachTime = this.GetCacheItemLifeTime(envVariableName);
            return this.GetCachedItem<string>($"{keyPreffix}({variableSchemaName})", () =>
            {
                return this.GetEnvironmentVariableValueBySchemaName(variableSchemaName);
            }, cachTime);
        }

        private CacheItem RetrieveCacheItem<T>(string key, int? retrieveLifeTime = 5, Func<T> retrieveCallback = null)
        {
            this.globalContext.LogEntry();
            T value = retrieveCallback.Invoke();

            if (value != null)
            {
                DateTime latestCacheItemRetrieveDate = DateTime.UtcNow;
                this.globalContext.Log.Info($"Latest cache item retrieve date: {latestCacheItemRetrieveDate}. Current cache item life time in minutes: {retrieveLifeTime}");
                return new CacheItem()
                {
                    LatestCacheItemRetrieveDate = latestCacheItemRetrieveDate,
                    RetrieveLifeTime = retrieveLifeTime,
                    Value = value
                };
            }
            else
            {
                throw new Exception($"({key}) Not Found");
            }
        }

        private void TryAddCacheItem(string key, CacheItem cacheItem)
        {
            this.globalContext.LogEntry();
            if (!cachedItems.TryAdd(key, cacheItem))
            {
                this.globalContext.Log.Info($"Renew existing cache item.");
                cachedItems[key] = cacheItem;
            }
        }

        private bool IsCacheItemExpired(string key)
        {
            DateTime? latestCacheItemRetrieveDate = cachedItems[key].LatestCacheItemRetrieveDate;
            int? cachedItemLifeTime = cachedItems[key].RetrieveLifeTime;
            TimeSpan time = DateTime.UtcNow - latestCacheItemRetrieveDate.Value;
            return time.TotalMinutes >= cachedItemLifeTime.Value;
        }

        private Entity GetApiConfigurationByCode(int apiConfigurationCode)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "alt_apiconfiguration",
                ColumnSet = new ColumnSet(true),
                NoLock = true,
                Criteria =
                   {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("alt_codeint", ConditionOperator.Equal, apiConfigurationCode),
                            new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                        }
                   }
            };

            var entity = this.globalContext.OrganizationService.RetrieveMultiple(query).Entities.FirstOrDefault();

            return entity != null ? entity
                : throw new Exception($"Api Configuration ({apiConfigurationCode}) Not Found");
        }

        private string GetGlobalParameterByName(string parameterName)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "alt_globalparameter",
                ColumnSet = new ColumnSet(true),
                NoLock = true,
                Criteria =
                   {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("alt_name", ConditionOperator.Equal, parameterName),
                            new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                        }
                   }
            };

            var entity = this.globalContext.OrganizationService.RetrieveMultiple(query).Entities.FirstOrDefault();

            return entity != null ? entity.GetAttributeValue<string>("alt_value")
                : throw new Exception($"Global Parameter {parameterName} Not Found");
        }

        private string GetEnvironmentVariableValueBySchemaName(string schemaName)
        {
            this.globalContext.LogEntry();
            QueryExpression query = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("defaultvalue", "valueschema", "schemaname", "environmentvariabledefinitionid", "type"),
                NoLock = true,
                Criteria =
                   {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.Equal, schemaName)
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
            var entity = this.globalContext.OrganizationService.RetrieveMultiple(query).Entities.FirstOrDefault();

            return entity.GetAttributeValue<AliasedValue>("variable.value")?.Value?.ToString()
            ?? entity.GetAttributeValue<string>("defaultvalue");
        }

        public int GetCacheItemLifeTime(string name)
        {
            this.globalContext.LogEntry();
            string keyPreffix = "EnvironmentVariable";
            string cacheItemKey = $"{keyPreffix}({name})";
            string value = this.GetCachedItem<string>(cacheItemKey, () =>
            {
                return this.GetEnvironmentVariableValueBySchemaName(name);
            });
            if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int time))
            {
                return time;
            }
            else
            {
                throw new InvalidPluginExecutionException($"Invalid cache item life time (Key: {cacheItemKey} Value: {value})");
            }
        }
    }
}
