using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework;
using Alt.Framework.Azure.KeyVault;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Alt.External.Services.CrmApi.Framework
{
    public class HttpRequestManager
    {
        private static SASToken SasTokenObj = null;
        private static readonly object lockobject = new object();
        GlobalContext GlobalContext { get; set; }

        public HttpRequestManager(GlobalContext globalContext)
        {
            this.GlobalContext = globalContext;
        }

        public Task<HttpResponseMessage> PostMessageToServiceBusQueue(ServiceBusCustomMessage serviceBusMessage, string serviceBusNamespaceUrl, string serviceBusQueueUrl, string sasKeyName, string sasValue = null)
        {
            this.GlobalContext.LogEntry();
            string sasToken = this.GetSasToken(serviceBusNamespaceUrl, sasKeyName, sasValue);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(serviceBusMessage);
            Task<HttpResponseMessage> response;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", sasToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = client.PostAsync(serviceBusQueueUrl, content);
                response.Wait();
            }
            return response;
        }

        private string GetSasToken(string serviceBusNamespaceUrl, string sasKeyName, string sasValue)
        {
            this.GlobalContext.LogEntry();
            if (SasTokenObj == null)
            {
                lock (lockobject)
                {
                    if (SasTokenObj == null)
                    {
                        var sasKeyValue = sasValue ?? KeyVaultUtils.GetSecretByNameAsync(sasKeyName);
                        SasTokenObj = new SASToken(serviceBusNamespaceUrl, sasKeyName, sasKeyValue);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(SasTokenObj.Token)
                || SasTokenObj.Expiry <= DateTime.UtcNow - SasTokenObj.StartTime)
            {
                lock (lockobject)
                {
                    if (string.IsNullOrWhiteSpace(SasTokenObj.Token)
                        || SasTokenObj.Expiry <= DateTime.UtcNow - SasTokenObj.StartTime)
                    {
                        SasTokenObj.GenerateToken();
                    }
                }
            }
            return SasTokenObj.Token;
        }
    }
}