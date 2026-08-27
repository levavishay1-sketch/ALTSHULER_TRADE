using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Controllers;
using Alt.External.Services.CrmApi.Framework;
using Alt.Framework.EntryPoints.External;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Alt.Test.CrmApi
{
    public class BaseUnitTest
    {

        protected static Guid? testCreatedContact;

        private TestContext testContext;

        public TestContext TestContext
        {
            get => testContext;
            set
            {
                testContext = value;
                OverrideAppConfigFile();
            }
        }

        private void OverrideAppConfigFile()
        {
            System.Configuration.Configuration systemConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Add an Application Setting.
            var conectionString = TestContext.Properties["CRMConnectionString"].ToString();
            systemConfig.AppSettings.Settings.Remove("CRMConnectionString");
            systemConfig.AppSettings.Settings.Add("CRMConnectionString", conectionString);

            var xmlPath = TestContext.Properties["xmlPath"].ToString();
            systemConfig.AppSettings.Settings.Remove("xmlPath");
            systemConfig.AppSettings.Settings.Add("xmlPath", xmlPath);


            // Save the configuration file.
            systemConfig.Save(ConfigurationSaveMode.Modified);

            // Force a reload of a changed section.
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void HandleControllerSetup(BaseController controller, HttpMethod method)
        {
            controller.Request = new HttpRequestMessage();
            controller.Request.Method = method;

            HttpConfiguration config = new HttpConfiguration();
            controller.Configuration = config;

            var conectionString = ConfigurationManager.AppSettings["CRMConnectionString"]; ;
            controller.ThirdPartyBase = new ThirdPartyBase(controller.GetType(), conectionString);
        }

        public bool ValidateModel(ApiEntity model, BaseController controller, HttpMethod httpMethod, string controllerName, string methodName, string routePath)
        {
            //setup for validate
            var request = new HttpRequestMessage(httpMethod, $"http://localhost:51365/{routePath}");

            controller.ControllerContext.Request = new HttpRequestMessage(httpMethod, $"http://localhost:51365/{routePath}"); //request;
            controller.ControllerContext.ControllerDescriptor = new HttpControllerDescriptor() { ControllerName = controllerName };
            controller.ControllerContext.Configuration = new HttpConfiguration();

            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor()
            {
                MethodInfo = controller.GetType().GetMethod(methodName),
                ControllerDescriptor = controller.ControllerContext.ControllerDescriptor
            };

            HttpActionContext actionContext = new HttpActionContext()
            {
                ControllerContext = controller.ControllerContext,
                ActionDescriptor = actionDescriptor
            };

            var validator = new GlobalModelValidator();

            //
            validator.Validate(model, model.GetType(), new System.Web.Http.Metadata.Providers.DataAnnotationsModelMetadataProvider(), actionContext, "");

            return actionContext.ModelState.IsValid;
        }
    }
}
