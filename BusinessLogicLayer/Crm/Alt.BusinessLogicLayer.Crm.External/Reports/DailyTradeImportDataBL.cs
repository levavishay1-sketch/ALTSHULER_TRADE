using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm.External.Reports
{
    public class DailyTradeImportDataBL : ExternalBLBase
    {
        public DailyTradeImportDataBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult HandleSSISLogResult(ApiScheduledOperation apiScheduledOperation)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            SchedulerSetupDAL schedulerSetupDal = new SchedulerSetupDAL(this.GlobalContext);
            var retrievedSchedulerSetup = schedulerSetupDal.GetFirstOrDefaultByAttribute("alt_codeint", apiScheduledOperation.SchedulerSetupCode.Value, new[]
            {
                "alt_developmentsettings",
                "alt_sendemailwithexecutionresultbit"
            });
            ETLLogMessageBlock eTLLogMessageBlock = null;
            bool isExceedded = false;
            string filteredExecutionResult = new string(apiScheduledOperation.ExecutionResult?
                .Where(c => !char.IsControl(c)).ToArray());

            if (!string.IsNullOrWhiteSpace(filteredExecutionResult))
            {
                eTLLogMessageBlock = base.GetDeserializedContent<ETLLogMessageBlock>(filteredExecutionResult);
                eTLLogMessageBlock.ParseToHtml(out isExceedded);
                actionResult.ReturnObject = isExceedded ? eTLLogMessageBlock.htmlWithoutWarnings : eTLLogMessageBlock.html;

                if (!string.IsNullOrWhiteSpace(eTLLogMessageBlock.exception))
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.PackageExecutionCompletedWithError);
                }
                else if (eTLLogMessageBlock.warnings != null
                    && eTLLogMessageBlock.warnings.Count > 0)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.DataReceptionCompletedWithWarnings);
                }
            }
            if ((retrievedSchedulerSetup.SendEmailWithExecutionResultBit.HasValue 
                    && retrievedSchedulerSetup.SendEmailWithExecutionResultBit.Value)
                || isExceedded)
            {
                this.SendExecutionResultByEmail(apiScheduledOperation, retrievedSchedulerSetup, eTLLogMessageBlock.html);
            }
            return actionResult;
        }

        private void SendExecutionResultByEmail(ApiScheduledOperation apiScheduledOperation, ApiSchedulerSetup schedulerSetup, string html)
        {
            this.GlobalContext.LogEntry();

            ErrorLogsReportConfigurations reportConfiguration = base.GetDeserializedContent<ErrorLogsReportConfigurations>(schedulerSetup.DevelopmentSettings);
            reportConfiguration.emailSettings.Regarding = apiScheduledOperation;
            reportConfiguration.emailSettings.Subject = reportConfiguration.Name;
            reportConfiguration.emailSettings.Description = html;

            EmailBL emailBL = new EmailBL(this.GlobalContext);
            emailBL.CreateEmailByEmailSettings(reportConfiguration.emailSettings);
        }
    }
}
