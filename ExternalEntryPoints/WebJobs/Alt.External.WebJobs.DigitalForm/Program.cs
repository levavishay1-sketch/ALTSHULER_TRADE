using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.Framework.EntryPoints.External;
using Alt.Framework.External.WebJobs;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Alt.External.WebJobs.DigitalForm
{
    class Program
    {
        static readonly ThirdPartyTracingService tracingService = new ThirdPartyTracingService();
        static async Task Main()
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
                string queueName = ConfigurationManager.AppSettings["DigitalFormsQueueName"];
                string environmentName = bool.Parse(ConfigurationManager.AppSettings["IsProduction"]) ?
                    EnvironmentName.Production : EnvironmentName.Development;

                var builder = new HostBuilder();
                builder.UseEnvironment(environmentName);
                var host = builder.Build();

                ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusHostName, new DefaultAzureCredential(), new ServiceBusClientOptions()
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                    RetryOptions = new ServiceBusRetryOptions() { MaxRetries = 3 }

                });

                ServiceBusProcessor processor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
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
                tracingService.Trace($"{ex.GetType()}: {ex}");
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
            catch (Exception ex)
            {
                tracingService.Trace($"{ex.GetType()}: {ex}");
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
            tracingService.Trace($"{args.Exception.GetType()}: {args.Exception}");
            return Task.CompletedTask;
        }
    }
}
