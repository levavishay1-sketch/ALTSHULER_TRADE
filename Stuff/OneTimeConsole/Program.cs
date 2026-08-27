using Alt.Framework.EntryPoints.External;
using Alt.Framework.External.WebJobs;
using OneTimeConsole.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace OneTimeConsole
{
    public class Program
    {
        static readonly string Username = ConfigurationManager.AppSettings[nameof(Username)];
        static readonly string Password = ConfigurationManager.AppSettings[nameof(Password)];
        static readonly string CrmURL = ConfigurationManager.AppSettings[nameof(CrmURL)];
        static readonly string AppId = ConfigurationManager.AppSettings[nameof(AppId)];
        static readonly string RedurectUri = ConfigurationManager.AppSettings[nameof(RedurectUri)];
        static readonly string CRMConnectionString = $@"
                    AuthType=OAuth;
                    Username={Username};
                    Password={Password};
                    Url={CrmURL};
                    AppId={AppId};
                    RedirectUri={RedurectUri};
                    LoginPrompt=Auto
                    ";
        //TokenCacheStorePath=C:\Users\adma\source\Credit\Alt_CRM\Main\Altshuler\Staff\OneTimeConsole\MyTokenCache;

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

            List<string> values = Enum.GetValues(typeof(OperationCode)).Cast<Enum>()
                                      .Select(e => $"({Convert.ToInt32(e)}) {e}").ToList();
            string operationOptions = string.Join("\n", values);

            while (true)
            {
                ThirdPartyBase thirdPartyBase = null;
                OperationBase operationBase = null;
                Console.WriteLine($"Enter the number of the action you want to perform.\n{operationOptions}");
                string operation = Console.ReadLine();
                if (int.TryParse(operation, out int result))
                {
                    OperationCode operationCode = (OperationCode)result;
                    thirdPartyBase = ExternalEntryPointManager.Connect(typeof(Program), new Guid(), operationCode.ToString(), null, null);
                    switch (operationCode)
                    {
                        case OperationCode.DigitalFormVerificationMainAccountHolder:
                        case OperationCode.DigitalFormVerificationEncouragingDepositSystemUser:
                            {
                                operationBase = new DigitalFormVerificationDataConversion(thirdPartyBase.GlobalContext, operationCode);
                                break;
                            }
                        case OperationCode.InactiveLeadsClosedOnDate:
                            {
                                operationBase = new LeadDataConversion(thirdPartyBase.GlobalContext, operationCode);
                                break;
                            }
                        case OperationCode.PortfoliosConversionTime:
                            {
                                operationBase = new PortfolioDataConversion(thirdPartyBase.GlobalContext, operationCode);
                                break;
                            }
                        default:
                            Console.WriteLine($"Not implemented OperationCode: {operationCode}");
                            continue;
                    }

                    Stopwatch watch = Stopwatch.StartNew();
                    operationBase?.Run();
                    watch.Stop();
                    Console.WriteLine($"\nRuntime duration {watch.Elapsed.Hours}:{watch.Elapsed.Minutes}:{watch.Elapsed.Seconds}");

                    thirdPartyBase?.Dispose();
                }
            }
        }
    }
}
