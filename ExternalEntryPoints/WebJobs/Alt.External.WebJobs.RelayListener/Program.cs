using Alt.DataModel.Crm.Core.Contracts;
using Alt.Framework.Azure.KeyVault;
using Alt.Framework.EntryPoints.External;
using Alt.Framework.External.WebJobs;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.Utils;
using Microsoft.ServiceBus;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Text.Json;

namespace Alt.External.WebJobs.RelayListener
{
    class Program
    {
        static string serviceBusRelayKVName = ConfigurationManager.AppSettings["ServiceBusRelayKVName"];
        static string serviceBusEndpoint = ConfigurationManager.AppSettings["ServiceBusEndpoint"];


        static void Main(string[] args)
        {
            #region Optimize Connection settings

            //Change max connections from .NET to a remote service default: 2
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;//65000;
            //Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4
            System.Threading.ThreadPool.SetMinThreads(50, 50);
            //Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server
            System.Net.ServicePointManager.Expect100Continue = false;
            //Can decreas overall transmission overhead but can cause delay in data packet arrival
            System.Net.ServicePointManager.UseNagleAlgorithm = false;

            #endregion Optimize Connection settings

            try
            {
                RunRestMain();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public static void RunRestMain()
        {
            var crmServiceBusSecretValue = KeyVaultUtils.GetSecretByNameAsync(serviceBusRelayKVName.Trim());
            List<string> crmServiceBusCredentials = crmServiceBusSecretValue.Split(',').Select(s => s.Trim()).ToList();
            var sharedSecretServiceBusCredential = new TransportClientEndpointBehavior()
            {
                TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(crmServiceBusCredentials[0], crmServiceBusCredentials[1]),
            };

            WebHttpRelayBinding binding = new WebHttpRelayBinding();
            binding.Security.Mode = EndToEndWebHttpSecurityMode.Transport;
            binding.MaxBufferPoolSize = 1000000;
            binding.MaxBufferSize = 1000000;
            binding.MaxReceivedMessageSize = 1000000;

            WebServiceHost host = new WebServiceHost(typeof(RelayServiceEndpoint));
            host.AddServiceEndpoint(typeof(IWebHttpServiceEndpointPlugin), binding, serviceBusEndpoint);
            var serviceRegistrySettings = new ServiceRegistrySettings(DiscoveryType.Public);
            foreach (var endpoint in host.Description.Endpoints)
            {
                endpoint.Behaviors.Add(serviceRegistrySettings);
                endpoint.Behaviors.Add(sharedSecretServiceBusCredential);
            }

            host.Open();
            Console.WriteLine($"Listening for messages from {serviceBusEndpoint}");
        }

        public static string Execute(RemoteExecutionContext executionContext)
        {
            string result = string.Empty;
            string parserSettings = string.Empty;
            ThirdPartyBase thirdPartyBase = null;
            ActionResult actionResult = new ActionResult();
            try
            {
                thirdPartyBase = ExternalEntryPointManager.Connect(typeof(Program), executionContext);

                parserSettings = (string)executionContext.SharedVariables["ParserSettings"];
                
                // string parserSettings = Encoding.UTF8.GetString(CompressionUtils.FromCompressedJson(Encoding.UTF8.GetBytes(compressedParserSettings), CompressionType.GZip));
                ParserSettings deserializedParserSettings = JsonSerializer.Deserialize<ParserSettings>(parserSettings);
                thirdPartyBase.GlobalContext.Log.Info($"parserSettings: {parserSettings}");
                Parser parser = new Parser(deserializedParserSettings);

                actionResult.ReturnObject = parser.GetParsedMessage(thirdPartyBase.GlobalContext.OrganizationService);
                Console.WriteLine($"ParsedMessage: {actionResult.ReturnObject}");
            }
            catch (Exception ex)
            {
                actionResult.SetToFailedActionResult(ex.ToString());
                Console.WriteLine($"Exeption: {ex}");
                thirdPartyBase?.GlobalContext?.Log.Critical($"parserSettings: {parserSettings}{Environment.NewLine},Exeption: {ex}");
            }
            finally
            {
                result = JsonSerializer.Serialize(actionResult);
                thirdPartyBase.GlobalContext.Log.Info($"Parsed Result: {actionResult}");
                thirdPartyBase?.Dispose();
            }
     
            return result;
        }
    }
}
