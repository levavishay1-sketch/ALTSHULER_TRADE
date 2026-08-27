using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.External.Json;
using Alt.Framework.External.Validators;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Alt.DataModel.ExernalServices
{
    public class ExternalEntityBase: IModifiedProperties, IValidatableObject
    {
        private EntityModifiedPropertiesSerializer serializer = new EntityModifiedPropertiesSerializer();

        [JsonIgnore]
        public string QueryParams { get; set; }

        [JsonIgnore]
        public List<string> DataModelValidationErrors { get; private set; }

        [JsonIgnore]
        public ConcurrentDictionary<string, object> ModifiedProperties { get; } = new ConcurrentDictionary<string, object>();

        public void SetProperty(object value, [CallerMemberName] string propertyName = "")
        {
            string trimedPropertyName = !string.IsNullOrWhiteSpace(propertyName) ? propertyName.Trim(' ') : propertyName;

            if (this.Contains(trimedPropertyName))
            {
                ModifiedProperties[trimedPropertyName] = value;
            }
            else
            {
                this.ModifiedProperties.TryAdd(trimedPropertyName, value);
            }
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

        public override string ToString()
        {
            return serializer.Serialize(this);
        }

        public bool ValidateDataModel()
        {
            List<ValidationResult> validationResult = new List<ValidationResult>();
            bool isValidDataModel = Validator.TryValidateObject(this, new ValidationContext(this), validationResult, true);
            DataModelValidationErrors = validationResult?.Select(r => r.ErrorMessage)?.ToList() ?? new List<string>();
            return isValidDataModel;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return DataAnnotationsValidator.Validate<ExternalEntityBase>(this);
        }
    }
}
