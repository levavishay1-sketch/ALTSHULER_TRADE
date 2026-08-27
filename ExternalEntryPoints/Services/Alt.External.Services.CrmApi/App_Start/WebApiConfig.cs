using Newtonsoft.Json;
using Alt.Framework.External.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Validation;
using Alt.External.Services.CrmApi.Framework;
using System.Web.Http.Controllers;

namespace Alt.External.Services.CrmApi.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //   authorization attribute that check the certificate validation
            //   config.Filters.Add(new ClientCertificateAuthenticationAttribute());

            //authorization attribute that check barer token
            // config.Filters.Add(new BearerAuthorizationAttribute());

            // handle serialized json of text/html request 
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // dont accept property that isn't in the api object property
            config.Formatters.JsonFormatter.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;

            // add Iso date time converter to JsonFormatter in Serializing
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsraelDateTimeConverter());
            //config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IsoDateTimeConverter());

            // Ignore Serializable Attribute(api object property)
            
           // config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver() { IgnoreSerializableAttribute = true };
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver() { IgnoreSerializableAttribute = true };
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // add modelState validator attribute 
            config.Filters.Add(new ValidateModelAttribute());


            // add custom global Context connerction manager to crm
            //config.Filters.Add(new GlobalContextManagerAttribute());
            

            // add custom Exception handler attribute
            config.Filters.Add(new ExceptionHandlerAttribute());

            // add custom Global Exception handler attribute - Catch all excprions that ExceptionHandlerAttribute not catches 
            config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());

            // add custom Global Model Validator - validate model based on XML
            config.Services.Replace(typeof(IBodyModelValidator), new GlobalModelValidator());

            // config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
        }
    }
}