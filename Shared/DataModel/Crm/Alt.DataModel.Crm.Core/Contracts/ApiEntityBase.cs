using Alt.DataModel.Crm.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class ApiEntityBase : IModifiedProperties, ICrmEntityMapperable
    {
        protected Guid? id;
        public virtual Guid? Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.SetProperty(value);
                this.id = value;
            }
        }

        protected string logicalName;
        public virtual string LogicalName
        {
            get
            {
                return this.logicalName;
            }
            set
            {
                this.SetProperty(value);
                this.logicalName = value;
            }
        }

        protected int? statusCode;
        public virtual int? StatusCode
        {
            get
            {
                return statusCode;
            }
            set
            {
                this.SetProperty(value);
                statusCode = value;
            }
        }

        protected int? stateCode;
        public virtual int? StateCode
        {
            get
            {
                return stateCode;
            }
            set
            {
                this.SetProperty(value);
                stateCode = value;
            }
        }

        protected DateTime? createdOn;
        public virtual DateTime? CreatedOn
        {
            get
            {
                return this.createdOn;
            }
            set
            {
                this.SetProperty(value);
                this.createdOn = value;
            }
        }

        protected DateTime? modifiedOn;
        public virtual DateTime? ModifiedOn
        {
            get
            {
                return this.modifiedOn;
            }
            set
            {
                this.SetProperty(value);
                this.modifiedOn = value;
            }
        }

        protected ApiEntityBase owner;
        public virtual ApiEntityBase Owner
        {
            get
            {
                return this.owner;
            }
            set
            {
                this.SetProperty(value);
                this.owner = value;
            }
        }

        protected int? creationMethodCode;
        public virtual int? CreationMethodCode
        {
            get
            {
                return creationMethodCode;
            }
            set
            {
                this.SetProperty(value);
                creationMethodCode = value;
            }
        }

        private int? apiConfigurationCode;
        public virtual int? ApiConfigurationCode
        {
            get
            {
                return apiConfigurationCode;
            }
            set
            {
                this.SetProperty(value);
                apiConfigurationCode = value;
            }
        }

        protected string recordUrl;
        public virtual string RecordUrl
        {
            get
            {
                return this.recordUrl;
            }
            set
            {
                this.SetProperty(value);
                this.recordUrl = value;
            }
        }

        public List<string> DataModelValidationErrors { get; protected set; }

        [JsonIgnore]
        public ConcurrentDictionary<string, object> ModifiedProperties { get; } = new ConcurrentDictionary<string, object>();

        protected Dictionary<string, object> EntityKeys { get; }

        public ApiEntityBase(string logicalName)
        {
            this.ModifiedProperties = new ConcurrentDictionary<string, object>();
            this.EntityKeys = new Dictionary<string, object>();
            this.LogicalName = logicalName;
        }

        public bool Contains(string propertyName)
        {
            return ModifiedProperties.ContainsKey(propertyName);
        }

        public List<string> GetModifiedPropertiesKeys()
        {
            return new List<string>(this.ModifiedProperties.Keys);
        }

        public object GetValueByKey(string key)
        {
            return this.Contains(key) ? this.ModifiedProperties[key] : null;
        }

        public void SetProperty(object value, [CallerMemberName] string propertyName = "")
        {
            string trimedPropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName.Trim(' ') : propertyName;
            if (this.Contains(trimedPropertyName))
            {
                this.ModifiedProperties[trimedPropertyName] = value;
            }
            else
            {
                this.ModifiedProperties.TryAdd(trimedPropertyName, value);
            }
        }

        public KeyValuePair<string, object> GetFirstOrDefaultEntityKeyValue(Func<KeyValuePair<string, object>, bool> predicate = null)
        {
            return predicate != null ? this.EntityKeys.FirstOrDefault(predicate) : this.EntityKeys.FirstOrDefault();
        }

        protected void SetEntityKeys(string key, object value)
        {
            if (value != null)
            {
                if (this.EntityKeys.ContainsKey(key) && value != null)
                {
                    this.EntityKeys[key] = value;
                }
                else
                {
                    this.EntityKeys.Add(key, value);
                }
            }
        }

        public bool IsAlternateKeyExist()
        {
            return this.EntityKeys.Count > 0;
        }

        public override bool Equals(object obj)
        {
            var castedObj = obj as ApiEntityBase;
            return castedObj != null && this.Id != null && castedObj.Id != null && this.Id.Value.Equals(castedObj.Id.Value) && this.LogicalName.Equals(castedObj.LogicalName);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
