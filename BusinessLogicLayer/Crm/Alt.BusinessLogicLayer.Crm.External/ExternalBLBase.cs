using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.External.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class ExternalBLBase
    {
        protected ApiConfiguration ApiConfiguration { get; private set; }
        protected GlobalContext GlobalContext { get; private set; }

        public ExternalBLBase(GlobalContext globalContext)
        {
            this.GlobalContext = globalContext;
            this.GetAndSetApiConfiguration(this.GlobalContext.ApiConfigurationCode);
        }

        protected ExternalBLBase(GlobalContext globalContext, ApiConfiguration apiConfiguration)
        {
            this.GlobalContext = globalContext;
            this.ApiConfiguration = apiConfiguration;
        }

        protected void GetAndSetApiConfiguration(int? apiConfigurationCode)
        {
            this.GlobalContext.LogEntry();

            ApiConfigurationDAL apiConfigurationDAL = new ApiConfigurationDAL(this.GlobalContext);
            this.ApiConfiguration = apiConfigurationDAL.GetApiConfigurationByCode(apiConfigurationCode);
        }

        protected virtual T GetDeserializedContent<T>(string content = null)
        {
            this.GlobalContext.LogEntry();
            return JsonSerializer.Deserialize<T>(content ?? this.GlobalContext.Content);
        }

        protected virtual T DeserializeSpecial<T>(string content)
        {
            this.GlobalContext.LogEntry();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new NullableIntToStringConverter());

            return JsonSerializer.Deserialize<T>(content,options);
        }

        protected string Serialize<T>(T apiEntity)
        {
            this.GlobalContext.LogEntry();
            return JsonSerializer.Serialize<T>(apiEntity, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        protected void SetDefaultOwner(ApiEntityBase apiEntity, ApiEntity owner)
        {
            if (apiEntity.Id == null)
            {
                apiEntity.Owner = owner;
            }
            foreach (var property in apiEntity.ModifiedProperties)
            {
                var value = property.Value;
                if (value != null)
                {
                    Type type = value.GetType();
                    if (type.IsSubclassOf(typeof(ApiEntityBase))
                        && !((ApiEntityBase)value).IsAlternateKeyExist()
                        && (((ApiEntityBase)value).Id == null || ((ApiEntityBase)value).Id == Guid.Empty))
                    {
                        this.SetDefaultOwner((ApiEntityBase)value, owner);
                    }
                    else if (type != typeof(string) && value is IEnumerable<ApiEntityBase>)
                    {
                        this.SetDefaultOwner((IEnumerable<ApiEntityBase>)value, owner);
                    }
                }
            }
        }

        private void SetDefaultOwner(IEnumerable<ApiEntityBase> apiEntities, ApiEntity owner)
        {
            foreach (var valueProperty in apiEntities)
            {
                this.SetDefaultOwner((ApiEntityBase)valueProperty, owner);
            }
        }

        protected void HandleDefaultOwner<T>(ApiEntityBase apiEntity, string settingKey) where T : ApiEntity
        {
            this.GlobalContext.LogEntry();

            if (this.ApiConfiguration == null)
            {
                GetAndSetApiConfiguration(apiEntity.ApiConfigurationCode);
            }
            if (this.ApiConfiguration.TryGetSettingsItemValue<T>(settingKey, out T defaultOwner)
                && defaultOwner != null)
            {
                this.SetDefaultOwner(apiEntity, defaultOwner);
            }
        }
    }
}
