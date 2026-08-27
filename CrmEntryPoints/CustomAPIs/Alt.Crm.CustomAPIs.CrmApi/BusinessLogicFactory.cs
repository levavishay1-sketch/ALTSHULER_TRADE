using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Alt.Crm.CustomAPIs.CrmApi
{
    public class BusinessLogicFactory
    {
        private const string POST = "Post";
        private const string PUT = "Put";
        GlobalContext globalContext;

        public BusinessLogicFactory(GlobalContext globalContext)
        {
            this.globalContext = globalContext;
        }

        public ActionResult Execute()
        {
            this.globalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            try
            {
                FetchConfigurationManagerBL fetchConfigurationManagerBL = new FetchConfigurationManagerBL(globalContext);
                actionResult = fetchConfigurationManagerBL.FetchRecords(globalContext.Content);
            }
            catch (Exception ex)
            {
                actionResult.SetToFailedActionResult(ex.Message);
                throw;
            }
            finally
            {                
                globalContext.Log.Info(actionResult.ReturnObject?.ToString());
            }

            return actionResult;
        }

        private ApiConfigurationCode? GetApiCode(string content, string keyName)
        {
            this.globalContext.LogEntry();
            ApiConfigurationCode? apiCode = null;
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            if (result.ContainsKey(keyName)
                && int.TryParse((result[keyName]).ToString(), out int apiConfigurationCode))
            {
                apiCode = (ApiConfigurationCode?)apiConfigurationCode;
            }
            return apiCode;
        }

        public ActionResult ValidateModel(ApiEntityBase apiEntity, string actionName, string controllerName, string routePath, string validationModel)
        {
            this.globalContext.LogEntry();

            GlobalModelValidator globalModelValidator = new GlobalModelValidator();
            return globalModelValidator.Validate(apiEntity, controllerName, routePath, actionName, validationModel);
        }

        protected virtual T GetDeserializedContent<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content);
        }
    }
}

