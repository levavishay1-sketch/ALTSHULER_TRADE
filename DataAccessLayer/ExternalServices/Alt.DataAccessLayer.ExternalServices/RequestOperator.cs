using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Alt.DataAccessLayer.ExternalServices
{
    public class RequestOperator
    {
        public GlobalContext GlobalContext { get; private set; }

        public RequestOperator(GlobalContext globalContext)
        {
            this.GlobalContext = globalContext;
        }

        public ActionResult SendRequestToEsb(HttpRequestMessage requestToExecute)
        {
            ActionResult dalActionResult = null;
            var handler = this.GetHttpClientCertificateHandler();
            using (HttpClient client = new HttpClient(handler))
            {
                var response = client.SendAsync(requestToExecute, HttpCompletionOption.ResponseHeadersRead).Result;
                dalActionResult = this.CreateDalActionResultByEsbHttpResponseMessage(response);
            }

            return dalActionResult;
        }

        public ActionResult SendGetFileRequest(HttpRequestMessage request)
        {
            this.GlobalContext.LogEntry();
            ActionResult dalActionResult = new ActionResult();
            var handler = this.GetHttpClientCertificateHandler();
            using (HttpClient client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                this.GlobalContext.Log.Info($"{request.ToString()}, RequestId:{this.GlobalContext.RequestId.ToString()}, CorrelationId:{this.GlobalContext.CorrelationId.ToString()} ");
                var response = client.SendAsync(request).Result;
                var responseContent = response.Content.ReadAsStreamAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    this.GlobalContext.Log.Critical($"{response.ToString()}, response-content:{responseContent}");
                    dalActionResult.IsSuccess = false;
                    dalActionResult.ReturnObject = null;
                }
                else
                {
                    this.GlobalContext.Log.Info($"response : {response.ToString()}, response-content:{responseContent}");
                    dalActionResult.ReturnObject = responseContent;
                }
            }

            return dalActionResult;
        }

        private WebRequestHandler GetHttpClientCertificateHandler()
        {
            this.GlobalContext.LogEntry();
           // var azureCertificateThumbPrint = KeyVaultUtils.GetSecretByNameAsync(ConfigurationManager.AppSettings["OutgoingCertificateThumbprintKVName"]);
            string azureCertificateThumbPrint = null;

            if (string.IsNullOrWhiteSpace(azureCertificateThumbPrint))
            {
                this?.GlobalContext?.Log.Critical($"Untrusted Certificates");
                throw new Exception("Azure Certificate Not Found Or Invalid");
            }

            var certificate = LoadCertificateFromMyStoreByThumbprint(StoreLocation.CurrentUser, StoreName.My, azureCertificateThumbPrint);
            if (certificate != null)
            {
                var handler = new WebRequestHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,                     
                    //SslProtocols = SslProtocols.Tls12
                };

                handler.ServerCertificateValidationCallback = ValidateServerCertificate;
                handler.ClientCertificates.Add(certificate);
                return handler;
            }
            else
            {
                this.GlobalContext.Log.Critical($"Azure Certificate Not Found Or Invalid");
                throw new Exception("Azure Certificate Not Found Or Invalid");
            }
        }

        private ActionResult CreateDalActionResultByEsbHttpResponseMessage(HttpResponseMessage response)
        {
            this.GlobalContext.LogEntry();
            string responseContent = response.Content.ReadAsStringAsync().Result;
            ActionResult dalActionResult = new ActionResult();
            if (!response.IsSuccessStatusCode)
            {
                this.GlobalContext.Log.Critical($"{response.ToString()}, response-content:{responseContent}");
                dalActionResult.SetToFailedActionResult(CustomErrorCodes.EsbInvalidResponseError, new[] { responseContent });
            }
            else
            {
                this.GlobalContext.Log.Info($"response : {response.ToString()}, response-content:{responseContent}");
            }

            dalActionResult.ReturnObject = responseContent;
            return dalActionResult;
        }

        private X509Certificate2 LoadCertificateFromMyStoreByThumbprint(StoreLocation storeLocation, StoreName storeName, string certificateThumbprint)
        {
            this.GlobalContext.LogEntry();

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
                this.GlobalContext?.Log?.Error(ex);
                certificate = null;
            }

            return certificate;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                this.GlobalContext.LogEntry();
                // check if no SslPolicyErrors and the thumbprint (of public certificate of esb)is for the esb server 
                if (sslPolicyErrors != SslPolicyErrors.None)
                {
                    var chainPolicyVerificationFlags = this.GetChainPolicyVerificationFlags();
                    X509Chain customChain = new X509Chain();
                    customChain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    customChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    customChain.ChainPolicy.VerificationFlags = (X509VerificationFlags)chainPolicyVerificationFlags;

                    bool isValid = customChain.ChainPolicy.VerificationFlags == X509VerificationFlags.NoFlag ? false : true;

                    try
                    {
                        isValid = customChain.Build((X509Certificate2)certificate);
                    }
                    catch (Exception ex)
                    {
                        this?.GlobalContext?.Log?.Critical(ex);
                    }
                    if (!isValid)
                    {
                        return false;
                    }
                }

                this.GlobalContext?.Log?.Info($"sslPolicyErrors : {sslPolicyErrors}.");
                var certificateHashToCheck = certificate.GetCertHashString();
                if (string.IsNullOrWhiteSpace(certificateHashToCheck))
                {
                    this?.GlobalContext?.Log?.Critical($"Untrusted Certificates");
                    return false;
                }

                var esbCertificate = LoadCertificateFromMyStoreByThumbprint(StoreLocation.CurrentUser, StoreName.My, certificateHashToCheck);
                // check if esb certificate is equal to esbCertificate that configured in azure
                if (esbCertificate == null || string.IsNullOrWhiteSpace(esbCertificate.Thumbprint) || esbCertificate.Thumbprint != certificateHashToCheck)
                {
                    this?.GlobalContext?.Log?.Critical($"Trusted Certificates Not Found In Azure");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                this.GlobalContext?.Log?.Error(ex);
                return false;
            }
        }

        private int GetChainPolicyVerificationFlags()
        {
            this.GlobalContext.LogEntry();
            var chainPolicyVerificationFlags = 0;
            try
            {
                chainPolicyVerificationFlags = int.Parse(ConfigurationManager.AppSettings["ChainPolicyVerificationFlags"]);
            }
            catch (Exception ex)
            {
                this?.GlobalContext?.Log?.Error(ex);
            }
            return chainPolicyVerificationFlags;
        }
    }
}
