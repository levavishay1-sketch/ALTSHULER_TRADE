using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.Framework.EntryPoints.External;
using Alt.Framework.External.WebJobs;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
//using Microsoft.Xrm.Sdk;
using System;
using System.Configuration;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Alt.External.WebJobs.ArchiveOutgoing
{
    class Program
    {
        static readonly ThirdPartyTracingService tracingService = new ThirdPartyTracingService();
        static async Task Main(string[] args)
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
                string serviceBusHostName = ConfigurationManager.AppSettings["ServiceBusHostName"];
                string queueName = ConfigurationManager.AppSettings["ArchiveOutgoingQueueName"];
                string environmentName = bool.Parse(ConfigurationManager.AppSettings["IsProduction"]) ?
                    EnvironmentName.Production : EnvironmentName.Development;

                var builder = new HostBuilder();
                builder.UseEnvironment(environmentName);
                var host = builder.Build();

                var clientOptions = new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                    RetryOptions = new ServiceBusRetryOptions() { MaxRetries = 3 }

                };
                ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusHostName, new DefaultAzureCredential(), clientOptions);

                var options = new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 50,
                    PrefetchCount = 1000
                };
                ServiceBusProcessor processor = serviceBusClient.CreateProcessor(queueName, options);

                processor.ProcessMessageAsync += ProcessMessagesAsync;
                processor.ProcessErrorAsync += ErrorHandler;

                using (host)
                {
                    await processor.StartProcessingAsync();
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"{nameof(Exception)}: {ex}");
            }
        }

        static async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            WebJobProcessHandler webJobProcessHandler = null;
            try
            {
                webJobProcessHandler = new WebJobProcessHandler(args, typeof(Program));
                IBusinessLogicStrategy businessLogicStrategy = BusinessLogicStrategyFactory.GenerateBusinessLogic(webJobProcessHandler);
                ActionResult actionResult = businessLogicStrategy.ExecuteBusinessLogicByEntityMessage();
                if (!actionResult.IsSuccess)
                {
                    string errorMessage = actionResult.Error != null ?
                        actionResult.Error.ToString() : actionResult.ReturnObject?.ToString();
                    webJobProcessHandler.ThirdPartyBase.GlobalContext.Log.Error(errorMessage);
                }
            }
            //catch (FaultException<OrganizationServiceFault> faultExeption)
            //{
            //    tracingService.Trace($"{nameof(FaultException<OrganizationServiceFault>)}: {faultExeption}");
            //    webJobProcessHandler?.ThirdPartyBase?.GlobalContext?.Log.Critical(faultExeption);
            //}
            catch (Exception ex)
            {
                tracingService.Trace($"{nameof(Exception)}: {ex}");
                webJobProcessHandler?.ThirdPartyBase?.GlobalContext?.Log.Critical(ex);
            }
            finally
            {
                await args.CompleteMessageAsync(args.Message);
                webJobProcessHandler?.ThirdPartyBase?.Dispose();
            }
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error occurred. Exception: {args.Exception}");
            return Task.CompletedTask;
        }
    }
}
