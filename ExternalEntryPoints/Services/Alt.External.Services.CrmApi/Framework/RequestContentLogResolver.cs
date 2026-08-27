using Alt.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http.Controllers;

namespace Alt.External.Services.CrmApi.Framework
{
    public class RequestContentLogResolver
    {
        private GlobalContext globalContext;
        private static readonly ConcurrentDictionary<string, string> ModelPropertiesLogToIgnoreSettings;
        private static object lockObject = new object();

        public RequestContentLogResolver(GlobalContext globalContext)
        {
            this.globalContext = globalContext;
        }
        static RequestContentLogResolver()
        {
            ModelPropertiesLogToIgnoreSettings = new ConcurrentDictionary<string, string>();
            //string ModelPropertiesLogToIgnoreSettingsJson = ConfigurationManager.AppSettings["ModelPropertiesToIgnoreInLogJson"]?.ToString().ToLower();
            //ModelPropertiesLogToIgnoreSettings = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(ModelPropertiesLogToIgnoreSettingsJson);
        }

        public string GetRequestBodyToLog(HttpActionContext actionContext, string bodyContent)
        {
            try
            {
                if (actionContext.ActionArguments != null && actionContext.ActionArguments.Count() > 0)
                {
                    var actionArgument = actionContext.ActionArguments.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(bodyContent)
                        && !actionArgument.Equals(default(KeyValuePair<string, string>))
                        && ModelPropertiesLogToIgnoreSettings.ContainsKey(actionArgument.Key.ToLower()))
                    {
                        bodyContent = this.RemoveBodyContentProperty(bodyContent, ModelPropertiesLogToIgnoreSettings[actionArgument.Key.ToLower()]);
                    }
                }

            }
            catch (Exception ex)
            {
                this.globalContext.Log.Warning(ex.ToString());
            }
            return bodyContent;

        }

        private string RemoveBodyContentProperty(string bodyContent, string propertiesToRemoveString)
        {
            string requestBodyContent = bodyContent;

            var jsonObj = JObject.Parse(bodyContent);
            var propertiesToRemove = propertiesToRemoveString.Split(',').Select(s => s.Trim()).ToHashSet();
            var propertiesToRemoveList = jsonObj.Descendants().OfType<JProperty>().Where(attr => propertiesToRemove.Contains(attr.Name.ToLower())).Select(attr => attr.Name).ToList();

            foreach (var property in propertiesToRemoveList)
            {
                if (jsonObj.ContainsKey(property))
                {
                    jsonObj.Remove(property);
                }
            }
            requestBodyContent = jsonObj.ToString();

            return requestBodyContent;
        }
    }
}