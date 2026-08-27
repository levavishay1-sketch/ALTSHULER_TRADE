using Alt.BusinessLogicLayer.Crm.External.Reports;
using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using System;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class ScheduledOperationBL : ExternalBLBase, ICrmOutgoing<ApiScheduledOperation>
    {
        public ScheduledOperationBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiScheduledOperation> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ScheduledOperationDAL scheduledOperationDal = new ScheduledOperationDAL(this.GlobalContext);
            ApiScheduledOperation retrievedScheduledOperation = scheduledOperationDal.GetScheduledOperationDetails(apiContext.Target.Id.Value);

            switch (retrievedScheduledOperation.StatusCode.Value)
            {
                case (int)ScheduledOperationStatusCode.Run:
                    {
                        scheduledOperationDal.Update(new ApiScheduledOperation()
                        {
                            Id = retrievedScheduledOperation.Id,
                            StatusCode = (int)ScheduledOperationStatusCode.Running,
                            StateCode = (int)CustomStateCode.Active
                        });
                        actionResult = this.RunScheduledOperation(retrievedScheduledOperation);
                        break;
                    }
                case (int)ScheduledOperationStatusCode.Running:
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.ScheduledOperationAlreadyRunningError,
                            new string[] { Enum.GetName(typeof(ScheduledOperationStatusCode), retrievedScheduledOperation.StatusCode.Value) });
                        break;
                    }
                default:
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForRunningScheduledOperation,
                            new string[] { Enum.GetName(typeof(ScheduledOperationStatusCode), retrievedScheduledOperation.StatusCode.Value) });
                        break;
                    }
            }
            return actionResult;
        }

        private ActionResult RunScheduledOperation(ApiScheduledOperation scheduledOperation)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            string executionResult = null;
            var retrievedSchedulerSetup = this.GetSchedulerSetup(scheduledOperation.SchedulerSetupCode.Value);
            try
            {
                actionResult = this.ExecuteBySchedulerSetupCode(scheduledOperation, retrievedSchedulerSetup);
                executionResult = actionResult.IsSuccess ?
                    actionResult.ReturnObject?.ToString()
                    : $"{actionResult.Error?.ToString()}{actionResult.ReturnObject?.ToString()}";
            }
            catch (Exception ex)
            {
                this.GlobalContext.Log.Critical(ex);
                actionResult.SetToFailedActionResult(ex.ToString());
                executionResult = $"{actionResult.Error?.ToString()}{actionResult.ReturnObject?.ToString()}";
            }
            finally
            {
                const int executionResultColumnMaxLength = 500000;

                ScheduledOperationDAL scheduledOperationDal = new ScheduledOperationDAL(this.GlobalContext);
                var schedulerOperationToUpdate = new ApiScheduledOperation()
                {
                    Id = scheduledOperation.Id,
                    StatusCode = actionResult.IsSuccess ?
                    (int)ScheduledOperationStatusCode.FinishedSuccessfully : (int)ScheduledOperationStatusCode.Failed,
                    ExecutionResult = executionResult
                };

                string tempExecutionResult = executionResult;
                if (executionResult?.Length > executionResultColumnMaxLength)
                {
                    schedulerOperationToUpdate.ExecutionResult = null;
                }

                scheduledOperationDal.Update(schedulerOperationToUpdate);
                schedulerOperationToUpdate.ExecutionResult = tempExecutionResult;

                SendEmailWithExecutionResult(schedulerOperationToUpdate, retrievedSchedulerSetup);
            }
            return actionResult;
        }

        private ActionResult ExecuteBySchedulerSetupCode(ApiScheduledOperation scheduledOperation, ApiSchedulerSetup retrievedSchedulerSetup)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            switch (scheduledOperation.SchedulerSetupCode.Value)
            {
                case (int)ScheduledOperationSetupType.ErrorLogsReport:
                    {
                        ErrorLogsReportBL reportBl = new ErrorLogsReportBL(this.GlobalContext);
                        actionResult = reportBl.HandleErrorLogsReportScheduledOperation(scheduledOperation, retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.TradeImportDataFromFiles:
                    {
                        TradeImportDataBL tradeImportDataBl = new TradeImportDataBL(this.GlobalContext);
                        actionResult = tradeImportDataBl.HandleImportFromFiles(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.DepositsImportData:
                case (int)ScheduledOperationSetupType.ShenhavImportData:
                    {
                        TradeImportDataBL tradeImportDataBl = new TradeImportDataBL(this.GlobalContext);
                        actionResult = tradeImportDataBl.HandleSSISLogResult(scheduledOperation, retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.JoiningProcessAbandonmentAlerts:
                    {
                        MailingBL mailingBl = new MailingBL(this.GlobalContext);
                        actionResult = mailingBl.HandleAutomaticMailingOnAbandonedJoiningProcess(scheduledOperation);
                        break;
                    }
                case (int)ScheduledOperationSetupType.DigitalFormVerificationDocumentSearch:
                    {
                        DigitalFormVerificationBL digitalFormVerificationBL = new DigitalFormVerificationBL(this.GlobalContext);
                        actionResult = digitalFormVerificationBL.HandleDocumentSearchForDigitalFormVerifications(scheduledOperation);
                        break;
                    }
                case (int)ScheduledOperationSetupType.DynamicsExportData:
                    {
                        DynamicsExportDataBL dynamicsExportDataBL = new DynamicsExportDataBL(this.GlobalContext);
                        actionResult = dynamicsExportDataBL.HandleDynamicsDataExport();
                        break;
                    }
                case (int)ScheduledOperationSetupType.DailyFetchXMLReport:
                case (int)ScheduledOperationSetupType.AdditionalFetchXMLReport:
                    {
                        FetchXMLReportBL reportBl = new FetchXMLReportBL(this.GlobalContext);
                        actionResult = reportBl.HandleFetchXMLReportScheduledOperation(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.BankSynchronization:
                    {
                        BankBL bankBl = new BankBL(this.GlobalContext);
                        actionResult = bankBl.HandleBankSynchronization(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.BranchesSynchronization:
                    {
                        BranchBL branchBL = new BranchBL(this.GlobalContext);
                        actionResult = branchBL.HandleBranchesSynchronization(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.CountriesSynchronization:
                    {
                        CountryBL countryBl = new CountryBL(this.GlobalContext);
                        actionResult = countryBl.HandleCountriesSynchronization(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.CitiesSynchronization:
                    {
                        CityBL cityBl = new CityBL(this.GlobalContext);
                        actionResult = cityBl.HandleCitiesSynchronization(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.StreetsSynchronization:
                    {
                        StreetBL streetBl = new StreetBL(this.GlobalContext);
                        actionResult = streetBl.HandleStreetsSynchronization(retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.MainAccountHoldersManualMailing:
                    {
                        DigitalFormVerificationBL digitalFormVerificationBL = new DigitalFormVerificationBL(this.GlobalContext);
                        actionResult = digitalFormVerificationBL.HandleMainAccountHoldersManualMailing(scheduledOperation, retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.DailyDepositsReport:
                    {
                        DepositsReportBL depositsReportBL = new DepositsReportBL(this.GlobalContext);
                        actionResult = depositsReportBL.HandleDailyDepositsReport(scheduledOperation, retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.LeadsSynchronizationToIVR:
                    {
                        LeadBL leadBL = new LeadBL(this.GlobalContext);
                        actionResult = leadBL.HandleLeadsSynchronizationToIVR(scheduledOperation, retrievedSchedulerSetup);
                        break;
                    }
                case (int)ScheduledOperationSetupType.ClearTotalMissedPhoneCallsTodayFromLeads:
                    {
                        LeadBL leadBL = new LeadBL(this.GlobalContext);
                        actionResult = leadBL.HandleClearTotalMissedPhoneCallsTodayFromLeads(scheduledOperation, retrievedSchedulerSetup);
                        break;
                    }
                default:
                    break;
            }
            return actionResult;
        }

        private ApiSchedulerSetup GetSchedulerSetup(int schedulerSetupCode)
        {
            this.GlobalContext.LogEntry();

            SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
            return schedulerSetupDal.GetFirstOrDefaultByAttribute("alt_codeint", schedulerSetupCode, null);
        }

        private void SendEmailWithExecutionResult(ApiScheduledOperation apiScheduledOperation, ApiSchedulerSetup apiSchedulerSetup)
        {
            this.GlobalContext.LogEntry();
            EmailSettings emailSettings;
            if (apiSchedulerSetup.SendEmailWithExecutionResultBit != null
                 && apiSchedulerSetup.SendEmailWithExecutionResultBit.Value
                 && apiSchedulerSetup.TryGetSettingsItemValue<EmailSettings>(nameof(emailSettings), out emailSettings))
            {
                if (!string.IsNullOrWhiteSpace(apiScheduledOperation.ExecutionResult)
                    || (emailSettings.SendOnEmptyResult.HasValue
                        && emailSettings.SendOnEmptyResult.Value))
                {
                    emailSettings.Regarding = apiScheduledOperation;
                    emailSettings.Description = apiScheduledOperation.ExecutionResult;

                    EmailBL emailBL = new EmailBL(this.GlobalContext);
                    emailBL.CreateEmailByEmailSettings(emailSettings);
                }
            }
        }
    }
}
