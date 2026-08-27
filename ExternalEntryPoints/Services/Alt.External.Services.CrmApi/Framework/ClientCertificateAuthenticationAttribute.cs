using Alt.External.Services.CrmApi.Controllers;
using Alt.Framework.Azure.KeyVault;
using Alt.Framework.EntryPoints.External;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Alt.External.Services.CrmApi.Framework
{
    public class ClientCertificateAuthenticationAttribute : AuthorizeAttribute
    {
        private BaseController baseControllerInstance = null;
        private Guid? requestId = null;

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                return this.IsValidClientCertificate(actionContext, actionContext.RequestContext.ClientCertificate);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsValidClientCertificate(HttpActionContext actionContext, X509Certificate2 certificate)
        {
            bool isValidCertificate = false;
            try
            {
                bool certificateValidationResult = false;
                baseControllerInstance = actionContext?.ControllerContext?.Controller as BaseController;
                if (!string.IsNullOrWhiteSpace(certificate?.Thumbprint)) // check if certificate is in trust store of azure
                {
                    var certificateFromAzureStore = LoadCertificateFromMyStoreByThumbprint(StoreLocation.CurrentUser, StoreName.My, certificate.Thumbprint);
                    if (certificateFromAzureStore != null)
                    {
                        var chainPolicyVerificationFlags = this.GetChainPolicyVerificationFlags(actionContext);
                        X509Chain chain = new X509Chain();
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = (X509VerificationFlags)chainPolicyVerificationFlags;//(X509VerificationFlags)3856;//0 // X509VerificationFlags.NoFlag;
                        certificateValidationResult = chain.ChainPolicy.VerificationFlags == X509VerificationFlags.NoFlag ? false : true;
                        try
                        {
                            certificateValidationResult = chain.Build(certificate);
                        }
                        catch (Exception ex)
                        {
                            this.InitializeLoggerHandler(actionContext);
                            string errorMessage = $"unauthorized reuquest in chain build- {actionContext?.Request}, Exception:{ex.ToString()}";

                            baseControllerInstance?.ThirdPartyBase?.GlobalContext?.Log?.Error(errorMessage);
                        }
                    }
                }
                isValidCertificate = this.CheckCertificateValidationResult(actionContext, certificateValidationResult);
            }
            catch (Exception ex)
            {
                this.InitializeLoggerHandler(actionContext);
                baseControllerInstance?.ThirdPartyBase?.GlobalContext?.Log?.Error(ex);
            }
            if (isValidCertificate)
            {
                actionContext.Request.Headers.Remove("X-ARR-ClientCert");
            }
            else
            {
                this.ExecuteLoggersContent();
            }
            return isValidCertificate;
        }

        private int GetChainPolicyVerificationFlags(HttpActionContext actionContext)
        {
            var chainPolicyVerificationFlags = 0;
            try
            {
                chainPolicyVerificationFlags = int.Parse(ConfigurationManager.AppSettings["ChainPolicyVerificationFlags"]);
            }
            catch (Exception ex)
            {
                this.InitializeLoggerHandler(actionContext);
                baseControllerInstance?.ThirdPartyBase?.GlobalContext?.Log?.Error(ex);
            }
            return chainPolicyVerificationFlags;
        }

        private bool CheckCertificateValidationResult(HttpActionContext actionContext, bool isCertificateValid)
        {
            if (!isCertificateValid)
            {
                this.InitializeLoggerHandler(actionContext);
                var isDebugMode = bool.Parse(ConfigurationManager.AppSettings["DebugMode"]);
                if (isDebugMode)
                {
                    baseControllerInstance?.ThirdPartyBase?.GlobalContext?.Log?.Warning($"Debug-Mode reuquest - {actionContext?.Request}");
                    isCertificateValid = true;
                }
                else
                {
                    baseControllerInstance?.ThirdPartyBase?.GlobalContext?.Log?.Error($"unauthorized reuquest - {actionContext.Request}");
                }
            }
            return isCertificateValid;
        }

        private void ExecuteLoggersContent()
        {
            if (baseControllerInstance?.ThirdPartyBase != null)
            {
                baseControllerInstance?.ThirdPartyBase?.Dispose();
            }
        }

        private void InitializeLoggerHandler(HttpActionContext actionContext)
        {
            this.ExtractRequestIdHandler(actionContext);
            if (baseControllerInstance != null && baseControllerInstance.ThirdPartyBase == null)
            {
                var conectionString = KeyVaultUtils.GetSecretByNameAsync(ConfigurationManager.AppSettings["CrmConnectionKVName"]);
                baseControllerInstance.ThirdPartyBase = new ThirdPartyBase(actionContext.ControllerContext.Controller.GetType(), conectionString, requestId, actionContext.ActionDescriptor.ActionName);
            }
        }

        private void ExtractRequestIdHandler(HttpActionContext actionContext)
        {
            if (requestId == null)
            {
                if (!actionContext.Request.Headers.TryGetValues("RequestId", out IEnumerable<string> headerValues)
                      || string.IsNullOrWhiteSpace(headerValues.FirstOrDefault()))
                {
                    requestId = Guid.NewGuid();
                }
                else if (Guid.TryParse(headerValues.FirstOrDefault(), out Guid paresdRequestId))
                {
                    requestId = paresdRequestId;
                }
                else
                {
                    requestId = Guid.NewGuid();
                }
            }
        }

        private X509Certificate2 LoadCertificateFromMyStoreByThumbprint(StoreLocation storeLocation, StoreName storeName, string certificateThumbprint)
        {
            X509Certificate2 certificate = null;
            try
            {
                X509Store certStore = new X509Store(storeName, storeLocation);
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                            X509FindType.FindByThumbprint,
                                            certificateThumbprint,
                                            false);
                if (certCollection.Count > 0)
                {
                    certificate = certCollection[0];
                }
            }
            catch (Exception ex)
            {
                baseControllerInstance?.ThirdPartyBase?.GlobalContext?.Log?.Error(ex);
                certificate = null;
            }
            return certificate;
        }
    }
}