using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Alt.Crm.CustomAPIs.CrmApi
{
    public class GlobalModelValidator
    {
        public ActionResult Validate(ApiEntityBase apiModel, string controllerName, string routePath, string actionName, string xmlModel)
        {
            ActionResult actionResult = new ActionResult();
            if (apiModel != null)
            {
                List<ControllersControllerRouteActionProperty> actionProperties = this.GetActionPropsFromXml(apiModel, controllerName, routePath, actionName, xmlModel);
                if (actionProperties == null)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.XmlValidationModelNotFound, null, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.XmlValidationModelNotFound));
                }
                else
                {
                    this.ValidateModelFields(apiModel, actionProperties, actionResult);
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiNullableInput, null, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.ApiNullableInput));
            }
            return actionResult;
        }

        private List<ControllersControllerRouteActionProperty> GetActionPropsFromXml(ApiEntityBase apiModel, string controllerName, string routePath, string actionName, string xmlModel)
        {
            var serializer = new XmlSerializer(typeof(Controllers));
            Controllers result;

            using (TextReader reader = new StringReader(xmlModel))
            {
                result = (Controllers)serializer.Deserialize(reader);
            }
            var routeAction = result.Controller
                .Where((controller) => controller.Name == controllerName).FirstOrDefault().Route
                .Where((route) => route.Path == routePath).FirstOrDefault().Action.
                Where((action) => action.Name == actionName).FirstOrDefault();
            var sourceSystemValidation = this.GetSourceSystemValidationHandler(apiModel, routeAction);

            return sourceSystemValidation != null ? sourceSystemValidation.Property.ToList() : null;
        }

        private ControllersControllerRouteActionSourceSystemValidation GetSourceSystemValidationHandler(ApiEntityBase apiModel, ControllersControllerRouteAction routeAction)
        {
            var sourceSystemValidation = routeAction.SourceSystemValidation.Where(s => s.Default == true).FirstOrDefault();
            var sourceSystemValidationCount = routeAction.SourceSystemValidation.Count();
            if (sourceSystemValidationCount > 1 || (sourceSystemValidationCount == 1 && sourceSystemValidation == null))
            {
                var sourceSystemValidationByPropertyToCheck = routeAction.SourceSystemValidation.Where(s => !string.IsNullOrWhiteSpace(s.PropertyToCheck)
                && !string.IsNullOrWhiteSpace(s.CheckValue) && apiModel.Contains(s.PropertyToCheck)
                && apiModel.GetValueByKey(s.PropertyToCheck)?.ToString() == s.CheckValue).FirstOrDefault();
                if (sourceSystemValidationByPropertyToCheck != null)
                {
                    sourceSystemValidation = sourceSystemValidationByPropertyToCheck;
                }
            }
            return sourceSystemValidation;
        }

        private void ValidateModelFields(ApiEntityBase apiModel, IEnumerable<ControllersControllerRouteActionProperty> actionProperties, ActionResult actionResult)
        {
            //get all modified properties names of model
            HashSet<string> copiedModifed = new HashSet<string>(apiModel.GetModifiedPropertiesKeys());

            foreach (var property in actionProperties)
            {
                //check if property is of type ApiEntityBase
                if (property.InnerProperty?.Length > 0 && copiedModifed.Contains(property.Name))
                {
                    var value = apiModel.GetValueByKey(property.Name);
                    //dive into inner properties if property has value
                    if (value != null)
                    {
                        if (value is IEnumerable<object>)
                        {
                            this.ValidateEnumerable(property, value, actionResult);

                        }
                        else
                        {
                            ApiEntityBase innerModel = value as ApiEntityBase;
                            this.ValidateModelFields(innerModel, property.InnerProperty.ToList(), actionResult);
                        }
                    }
                }
                //required property validation
                if (property?.Required == 1)
                {
                    ValidateRequiredProperty(apiModel, copiedModifed, property.Name, actionResult);
                }
                if (property.MaxLength != null)
                {
                    var propertyValue = apiModel.GetValueByKey(property.Name);

                    if (property.MaxLength < propertyValue?.ToString().Length)
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.CommonMaxLengthValidationMessage, null, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonMaxLengthValidationMessage), property.Name));
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
                        actionResult.SetToFailedActionResult(CustomErrorCodes.WebApiInvalidProperty, null, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.WebApiInvalidProperty), invalidProperyName));
                    }
                }
            }
        }

        private void ValidateEnumerable(ControllersControllerRouteActionProperty property, object apiPropertyValue, ActionResult actionResult)
        {
            IEnumerable<ApiEntityBase> apiModelCollection = apiPropertyValue as IEnumerable<ApiEntityBase>;

            if (property?.Required == 1 && apiModelCollection.Count() == 0)
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.CommonEmptyEnumerableRequiredFieldMessage, null, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonEmptyEnumerableRequiredFieldMessage), property.Name));
            }

            foreach (var modelItem in apiModelCollection)
            {
                this.ValidateModelFields(modelItem, property.InnerProperty.ToList(), actionResult);
            }
        }

        private void ValidateRequiredProperty(ApiEntityBase apiModel, IEnumerable<string> copiedModifed, string propertyName, ActionResult actionResult)
        {
            if (!copiedModifed.Contains(propertyName) || (apiModel.GetValueByKey(propertyName) == null) ||
                string.IsNullOrWhiteSpace(apiModel.GetValueByKey(propertyName)?.ToString()))
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.CommonRequiredFieldMessage, null, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), propertyName));
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
