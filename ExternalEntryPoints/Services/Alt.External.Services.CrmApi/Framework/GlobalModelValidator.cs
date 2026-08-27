using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Alt.External.Services.CrmApi.Cache;
using Alt.External.Services.CrmApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;

namespace Alt.External.Services.CrmApi.Framework
{
    public class GlobalModelValidator : DefaultBodyModelValidator, IBodyModelValidator
    {
        public new bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix)
        {
            ApiEntityBase apiModel = model as ApiEntityBase;
            if (apiModel != null)
            {
                List<ControllersControllerRouteActionProperty> actionProperties = ControllersAvailableDataCache.GetActionValidationModelProperties(actionContext, apiModel);
                if (actionProperties == null)
                {
                    string message = CustomErrorCodes.GetErrorMessage(CustomErrorCodes.XmlValidationModelNotFound);
                    actionContext.ModelState.AddModelError(message, $"Error code:{CustomErrorCodes.XmlValidationModelNotFound} message: {message}");
                }
                else
                {
                    this.ValidateModelFields(actionContext, apiModel, actionProperties);
                }
            }
            else
            {
                actionContext.ModelState.AddModelError(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.ApiNullableInput), CustomErrorCodes.GetErrorMessage(CustomErrorCodes.ApiNullableInput));
            }

            return base.Validate(model, type, metadataProvider, actionContext, keyPrefix) && actionContext.ModelState.IsValid;
        }

        private void ValidateModelFields(HttpActionContext actionContext, ApiEntityBase apiModel, IEnumerable<ControllersControllerRouteActionProperty> actionProperties)
        {
            //get all modified properties names of model
            HashSet<string> copiedModifed = new HashSet<string>(apiModel.GetModifiedPropertiesKeys());

            foreach (var property in actionProperties)
            {
                //check if property is of type ApiEntityBase
                if (property.InnerProperty?.Count > 0 && copiedModifed.Contains(property.Name))
                {
                    var value = apiModel.GetValueByKey(property.Name);
                    //dive into inner properties if property has value
                    if (value != null)
                    {
                        if (value is IEnumerable<object>)
                        {
                            this.ValidateEnumerable(actionContext, property, value);

                        }
                        else
                        {
                            ApiEntityBase innerModel = value as ApiEntityBase;
                            this.ValidateModelFields(actionContext, innerModel, property.InnerProperty.ToList());
                        }
                    }
                }
                //required property validation
                if (property?.Required == 1)
                {
                    ValidateRequiredProperty(actionContext, apiModel, copiedModifed, property.Name);
                }
                if (property.MaxLength != null)
                {
                    var propertyValue = apiModel.GetValueByKey(property.Name);

                    if (property.MaxLength < propertyValue?.ToString().Length)
                    {
                        actionContext.ModelState.AddModelError("Max Length Field Validation", $"Error code:{CustomErrorCodes.CommonMaxLengthValidationMessage} message: {string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonMaxLengthValidationMessage), property.Name)}");
                    }
                }

                //check if default value is provided and no value provided to property
                if (!string.IsNullOrWhiteSpace(property?.DefaultValue) && !copiedModifed.Contains(property.Name))
                {
                    this.SetDefaultValue(apiModel, property.Name, property.DefaultValue);
                }
                if (property.Required != null)
                {
                    copiedModifed.Remove(property.Name);
                }

            }

            //check if recieved property which is not listed in XML
            if (copiedModifed.Count > 0)
            {
                foreach (var invalidProperyName in copiedModifed)
                {
                    if (invalidProperyName.ToLower() != "logicalname")
                    {
                        actionContext.ModelState.AddModelError("Invalid Property", $"Error code:{CustomErrorCodes.WebApiInvalidProperty} message: {string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.WebApiInvalidProperty), invalidProperyName)}");
                    }
                }
            }
        }

        private void ValidateEnumerable(HttpActionContext actionContext, ControllersControllerRouteActionProperty property, object apiPropertyValue)
        {
            IEnumerable<ApiEntityBase> apiModelCollection = apiPropertyValue as IEnumerable<ApiEntityBase>;

            if (property?.Required == 1 && apiModelCollection.Count() == 0)
            {
                actionContext.ModelState.AddModelError("Required Field", $"Error code:{CustomErrorCodes.CommonEmptyEnumerableRequiredFieldMessage} message: {string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonEmptyEnumerableRequiredFieldMessage), property.Name)}");
            }

            foreach (var modelItem in apiModelCollection)
            {
                this.ValidateModelFields(actionContext, modelItem, property.InnerProperty.ToList());
            }
        }

        private void ValidateRequiredProperty(HttpActionContext actionContext, ApiEntityBase apiModel, IEnumerable<string> copiedModifed, string propertyName)
        {
            if (!copiedModifed.Contains(propertyName) || (apiModel.GetValueByKey(propertyName) == null) ||
                string.IsNullOrWhiteSpace(apiModel.GetValueByKey(propertyName)?.ToString()))
            {
                actionContext.ModelState.AddModelError("Required Field", $"Error code:{CustomErrorCodes.CommonRequiredFieldMessage} message: {string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), propertyName)}");
            }
        }

        private void SetDefaultValue(ApiEntityBase apiModel, string propertyName, string defaultValue)
        {
            PropertyInfo modelProp = apiModel.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (modelProp != null)
            {
                Type modelPropType = Nullable.GetUnderlyingType(modelProp.PropertyType) ?? modelProp.PropertyType;

                string[] splittedValue = defaultValue.Split('.');
                string value;
                if (splittedValue.Length == 1)
                {
                    value = splittedValue[0];
                }
                else
                {
                    Type staticType = Type.GetType($"System.{splittedValue[0]}");
                    if (splittedValue[1].Last() == ')')
                    {
                        value = staticType.GetMethod(splittedValue[1].TrimEnd('(', ')')).Invoke(null, null).ToString();
                    }
                    else
                    {
                        value = staticType.GetProperty(splittedValue[1]).GetMethod.Invoke(null, null).ToString();
                    }
                }
                var val = Convert.ChangeType(value, modelPropType);
                modelProp.SetValue(apiModel, val);
            }
            else
            {
                throw new Exception($"{CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidApiInput)} Property {propertyName} not exist in model.");
            }
        }
    }
}