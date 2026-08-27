using Microsoft.IdentityModel.Tokens;
using Owin;
using System.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Alt.Framework.Azure.KeyVault;
using System;
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Alt.External.Services.CrmApi.App_Start.Startup))]

namespace Alt.External.Services.CrmApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // ConfigureAzureADAuth(app);
        }

        private void ConfigureAzureADAuth(IAppBuilder app)
        {
            //app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions()
            //{
            //    AuthenticationMode = AuthenticationMode.Active
            //});
            //app.UseWindowsAzureActiveDirectoryBearerAuthentication(
            // new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            // {
            //     //Tenant = "",
            //     //TokenValidationParameters = new TokenValidationParameters { ValidAudience = "" }
            // });
        }
    }
}