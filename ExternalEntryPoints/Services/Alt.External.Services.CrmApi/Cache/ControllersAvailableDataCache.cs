using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Framework;
using Alt.External.Services.CrmApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http.Controllers;
using System.Xml.Serialization;

namespace Alt.External.Services.CrmApi.Cache
{
    internal class ControllersAvailableDataCache
    {

        internal static Models.Controllers GetControllers(string xmlValidationModel)
        {
            Models.Controllers result = null;
            if (!string.IsNullOrWhiteSpace(xmlValidationModel))
            {
                var serializer = new XmlSerializer(typeof(Models.Controllers));

                using (TextReader reader = new StringReader(xmlValidationModel))
                {
                    result = (Models.Controllers)serializer.Deserialize(reader);
                }
            }

            return result;
        }


        private static ControllersControllerRouteActionSourceSystemValidation GetSourceSystemValidationHandler(ApiEntityBase apiModel, ControllersControllerRouteAction routeAction)
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

        public static List<ControllersControllerRouteActionProperty> GetActionValidationModelProperties(HttpActionContext actionContext, ApiEntityBase apiModel)
        {
            Models.Controllers controllers = new Models.Controllers();
            ExternalEntryPointManager.Connect(actionContext);

            var baseController = (actionContext.ControllerContext.Controller as Controllers.BaseController);
            string routePath = actionContext.Request.RequestUri.AbsolutePath.Trim('/');
            string controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = actionContext.ActionDescriptor.ActionName;

            List<ApiConfiguration> apiConfigurations = GetControllerApiConfigurationsByRoute(baseController, routePath);
            if (apiConfigurations != null && apiConfigurations.Count() > 0)
            {
                foreach (var apiConfiguration in apiConfigurations)
                {
                    var result = GetControllers(apiConfiguration.XmlValidationModel);
                    if (result != null && result.Controller.Count > 0 && result.Controller[0].Name == controllerName)
                    {
                        if (controllers.Controller != null)
                        {
                            string resultActionName = result.Controller[0].Route[0].Action[0].Name;
                            bool isAddedAction = controllers.Controller[0].Route[0].Action.Where(a => a.Name == resultActionName).Any();
                            if (isAddedAction)
                            {
                                controllers.Controller[0].Route[0].Action
                                    .Where(a => a.Name == resultActionName).First().SourceSystemValidation
                                    .AddRange(result.Controller[0].Route[0].Action[0].SourceSystemValidation);
                            }
                            else
                            {
                                controllers.Controller[0].Route[0].Action.AddRange(result.Controller[0].Route[0].Action);
                            }
                        }
                        else
                        {
                            controllers.Controller = result.Controller;
                        }
                    }
                }
            }

            var routeAction = controllers.Controller
                .Where((controller) => controller.Name == controllerName).FirstOrDefault().Route
                .Where((route) => route.Path == routePath).FirstOrDefault().Action.
                Where((action) => action.Name == actionName).FirstOrDefault();
            var sourceSystemValidation = GetSourceSystemValidationHandler(apiModel, routeAction);

            return sourceSystemValidation != null ? sourceSystemValidation.Property.ToList() : null;
        }

        public static List<ApiConfiguration> GetControllerApiConfigurationsByRoute(Controllers.BaseController baseController, string routePath)
        {
            var apiConfigurations = GetControllersApiConfigurations(baseController);
            return apiConfigurations.Where(a => a.Url == routePath).ToList();
        }

        public static ApiConfiguration GetApiConfigurationByCode(HttpActionContext actionContext, int? code)
        {
            var baseController = (actionContext.ControllerContext.Controller as Controllers.BaseController);
            var apiConfigurations = GetControllersApiConfigurations(baseController);
            return apiConfigurations.Where(a => a.Code == code).FirstOrDefault();
        }

        public static List<ApiConfiguration> GetControllersApiConfigurations(Controllers.BaseController baseController)
        {
            return CommonBL.GetApiConfigurationByType(baseController.ThirdPartyBase.GlobalContext, ApiTypeCode.Incoming);
        }
    }
}