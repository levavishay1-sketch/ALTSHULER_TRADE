using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class ApiConfigurationDAL : CrmExternalBaseDAL<ApiConfiguration>
    {
        public ApiConfigurationDAL(GlobalContext globalContext) : base(globalContext, ApiConfiguration.EntityLogicalName) { }

        public List<ApiConfiguration> GetApiConfigurationByType(ApiTypeCode apiTypeCode)
        {
            this.GlobalContext.LogEntry(apiTypeCode.ToString());

            var apiConfigurations = this.GetAll();
            return apiConfigurations.Where(a => a.ApiTypeCode != null && a.ApiTypeCode.Value == (int)apiTypeCode).ToList();
        }

        public ApiConfiguration GetApiConfigurationByCode(int? code)
        {
            this.GlobalContext.LogEntry(code?.ToString());

            var apiConfigurations = this.GetAll();
            return apiConfigurations?.Where(a => a.Code != null && a.Code.Value == code).FirstOrDefault();
        }

        public List<ApiConfiguration> GetAll()
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiConfiguration.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                NoLock = true
            };
            string envVariableName = "alt_ApiConfigurationCacheLifeTimeInMinutes";
            int cachTime = GlobalContext.CacheManager.GetCacheItemLifeTime(envVariableName);

            return GlobalContext.CacheManager.GetCachedItem<List<ApiConfiguration>>($"{nameof(ApiConfiguration)}s"
                , () => { return GetMultiple(query); }, cachTime);
        }

        public ApiConfiguration GetApiConfigurationById(Guid? id)
        {
            this.GlobalContext.LogEntry();

            var apiConfigurations = this.GetAll();
            return apiConfigurations?.Where(a => a.Id.Value == id).FirstOrDefault();
        }
    }
}
