using Alt.DataModel.Crm.Core.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.Cache
{
    public sealed class ApiConfigurationCache
    {
        private ConcurrentDictionary<int, Entity> apiConfigurations;
        private DateTime lastRetrieveDate;
        private int? cachePeriodInMinutes = 10;
        private static object lockObject = new object();

        private static readonly Lazy<ApiConfigurationCache> lazy = new Lazy<ApiConfigurationCache>(() => new ApiConfigurationCache());
        public static ApiConfigurationCache Instance { get { return lazy.Value; } }

        private ApiConfigurationCache()
        {
            this.apiConfigurations = new ConcurrentDictionary<int, Entity>();
        }

        public ConcurrentDictionary<int, Entity> GetApiConfigurations(IOrganizationService service, ILog log)
        {
            log.Info($"Get api configuration request. Last retrieve date {this.lastRetrieveDate}. Current cache period in minutes ({this.cachePeriodInMinutes}).");
            if (this.apiConfigurations == null || this.IsNeedToUpdateCache())
            {
                lock (lockObject)
                {
                    if (this.apiConfigurations == null || this.IsNeedToUpdateCache())
                    {
                        log.Info("Retrive api configurations executing...");
                        this.apiConfigurations = this.RetrieveApiConfigurations(service);
                        this.lastRetrieveDate = DateTime.UtcNow;
                    }
                }
            }
            else
            {
                log.Info("Using api configurations from cache.");
            }
            return this.apiConfigurations;
        }

        public Entity GetApiConfigurationByCode(IOrganizationService service, ILog log, int code)
        {
            this.GetApiConfigurations(service, log);
            return this.GetApiConfigurationByCode(code);
        }

        private Entity GetApiConfigurationByCode(int code)
        {
            if (this.apiConfigurations.ContainsKey(code))
            {
                if (this.apiConfigurations.TryGetValue(code, out Entity value))
                {
                    return value;
                }
                else
                {
                    throw new InvalidPluginExecutionException($"Falid to get api configuration by code {code}.");
                }
            }
            else
            {
                throw new InvalidPluginExecutionException($"Api configuration with code {code} not found.");
            }
        }

        private ConcurrentDictionary<int, Entity> RetrieveApiConfigurations(IOrganizationService service)
        {
            ConcurrentDictionary<int, Entity> apiConfigurations = new ConcurrentDictionary<int, Entity>();
            QueryExpression query = new QueryExpression("alt_apiconfiguration")
            {
                ColumnSet = new ColumnSet("alt_apiconfigurationid", "alt_codeint", "alt_httpheaders", "alt_url", "alt_requestmethodcode", "alt_destinationsystemcode", "alt_debugmodebit"),             
            };
            var result = service.RetrieveMultiple(query);
            if (result?.Entities.Count > 0)
            {
                foreach (var entity in result.Entities)
                {
                    var code = entity.GetAttributeValue<int?>("alt_codeint");
                    if (code != null && !apiConfigurations.ContainsKey(code.Value))
                    {
                        apiConfigurations.TryAdd(code.Value, entity);
                    }
                }
            }
            return apiConfigurations;
        }

        private bool IsNeedToUpdateCache()
        {
            return this.cachePeriodInMinutes == null
                || this.lastRetrieveDate.AddMinutes((int)this.cachePeriodInMinutes) <= DateTime.UtcNow;
        }
    }
}
