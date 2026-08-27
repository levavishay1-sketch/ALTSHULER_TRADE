using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiEntity : ApiEntityBase, IValidatableObject
    {
        public ApiEntity(string logicalName) : base(logicalName)
        {
        }
        public ApiEntity() : base(null)
        {
        }

        [CrmEntityMapper("{0}id", CrmPropertyType.Guid)]
        public override Guid? Id
        {
            get
            {
                return base.id;
            }
            set
            {
                base.SetProperty(value);
                base.id = value;
            }
        }

        [CrmEntityMapper(null, CrmPropertyType.String)]
        public override string LogicalName
        {
            get
            {
                return base.logicalName;
            }
            set
            {
                base.SetProperty(value);
                base.logicalName = value;
            }
        }

        [CrmEntityMapper("statuscode", CrmPropertyType.OptionSet)]
        public override int? StatusCode
        {
            get
            {
                return statusCode;
            }
            set
            {
                base.SetProperty(value);
                statusCode = value;
            }
        }

        [CrmEntityMapper("statecode", CrmPropertyType.OptionSet)]
        public override int? StateCode
        {
            get
            {
                return stateCode;
            }
            set
            {
                base.SetProperty(value);
                stateCode = value;
            }
        }

        [CrmEntityMapper("createdon", CrmPropertyType.DateTime)]
        public override DateTime? CreatedOn
        {
            get
            {
                return base.createdOn;
            }
            set
            {
                base.SetProperty(value);
                base.createdOn = value;
            }
        }

        [CrmEntityMapper("modifiedon", CrmPropertyType.DateTime)]
        public override DateTime? ModifiedOn
        {
            get
            {
                return base.modifiedOn;
            }
            set
            {
                base.SetProperty(value);
                base.modifiedOn = value;
            }
        }

        [CrmEntityMapper("ownerid", CrmPropertyType.EntityReference)]
        public override ApiEntityBase Owner
        {
            get
            {
                return base.owner;
            }
            set
            {
                base.SetProperty(value);
                base.owner = value;
            }
        }

        [CrmEntityMapper("alt_creationmethodcode", CrmPropertyType.OptionSet)]
        public override int? CreationMethodCode
        {
            get
            {
                return creationMethodCode;
            }
            set
            {
                base.SetProperty(value);
                base.creationMethodCode = value;
            }
        }

        [CrmEntityMapper(null, CrmPropertyType.EntityReference)]
        public override string RecordUrl
        {
            get
            {
                return base.recordUrl;
            }
            set
            {
                base.SetProperty(value);
                base.recordUrl = value;
            }
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
            return this.Validate<ApiEntity>(this);
        }

        private IEnumerable<ValidationResult> Validate<T>(object objectToValidate)
            where T : class
        {
            var results = new List<ValidationResult>();
            var properties = objectToValidate.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(objectToValidate);
                if (value is T)
                {
                    HandleBuildPropertyValidationResult<T>(results, (value as T), propertyInfo.Name);
                }
                else if (value != null && value is IEnumerable<T>)
                {
                    IEnumerable<T> collection = value as IEnumerable<T>;
                    foreach (var item in collection)
                    {
                        HandleBuildPropertyValidationResult<T>(results, item, propertyInfo.Name);
                    }
                }
            }
            return results;
        }

        private static void HandleBuildPropertyValidationResult<T>(List<ValidationResult> globalValidationResults, T propertyValue, string propertyName) where T : class
        {
            var validationResult = new List<ValidationResult>();
            Validator.TryValidateObject(propertyValue, new ValidationContext(propertyValue), validationResult, true);
            globalValidationResults.AddRange(validationResult.Select(e =>
            {
                e.ErrorMessage = $"{propertyName}.{e.ErrorMessage}";
                return e;
            }).ToList());
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


        public string[] GetMapperableProperties(bool validToMappFromCrm = true, bool validToMappToCrm = true)
        {

            List<string> mapperableProperties = new List<string>();

            var props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is CrmEntityMapperAttribute attribute)
                    {
                        if (attribute.MappFromCrm == validToMappFromCrm && attribute.MappToCrm == validToMappToCrm)
                        {
                            if (attribute.CrmPropertyName == "{0}id")
                            {
                                mapperableProperties.Add(string.Format(attribute.CrmPropertyName, this.logicalName));
                            }
                            else
                            {
                                mapperableProperties.Add(attribute.CrmPropertyName);
                            }
                        }
                    }
                }
            }
            return mapperableProperties.Where(p => p != null).ToArray();
        }

        public Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> mapperableProperties =new Dictionary<string, string>();

            var props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is CrmEntityMapperAttribute attribute 
                        && !string.IsNullOrWhiteSpace(attribute.CrmPropertyName))
                    {

                            if (attribute.CrmPropertyName == "{0}id")
                            {
                                mapperableProperties.Add(prop.Name,string.Format(attribute.CrmPropertyName, this.logicalName));
                            }
                            else
                            {
                                mapperableProperties.Add(prop.Name, attribute.CrmPropertyName);
                            }
                    }
                }
            }
            return mapperableProperties;
        }
    }
}
