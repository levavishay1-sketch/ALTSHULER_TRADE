using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Alt.BusinessLogicLayer.Crm.External.Reports
{
    public class ErrorLogsReportBL : ExternalBLBase
    {
        DailyErrorLogsReport dailyReport;
        List<ApiSystemLog> logs;
        List<ApiError> errorKeys;
        List<ApiScheduledOperation> scheduledOperations;
        List<ApiAsyncOperation> failedJobs;

        public ErrorLogsReportBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult HandleErrorLogsReportScheduledOperation(ApiScheduledOperation apiScheduledOperation, ApiSchedulerSetup apiSchedulerSetup)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ErrorLogsReportConfigurations reportConfiguration = base.GetDeserializedContent<ErrorLogsReportConfigurations>(apiSchedulerSetup.DevelopmentSettings);
            DateTime date = apiScheduledOperation.ModifiedOn.Value.Date.AddDays(-reportConfiguration.XDaysBefore.Value);
            string contentAsHtml = this.GetDailyErrorsReportAsHtml(date, reportConfiguration.Description);
            string dateString = date.ToString("dd/MM/yyyy");
            string reportDetails = string.IsNullOrEmpty(contentAsHtml) ?
                     $"{reportConfiguration.EmptyResultMessage} {dateString}" : contentAsHtml;

            actionResult.ReturnObject += $"{Environment.NewLine}{reportDetails}";
            return actionResult;
        }

        public string GetDailyErrorsReportAsHtml(DateTime date, string reportName)
        {
            this.GlobalContext.LogEntry();
            string htmlString = null;

            SystemLogDAL systemLogDal = new SystemLogDAL(this.GlobalContext);
            this.logs = systemLogDal.GetErrorLogsByDate(date);
            if (logs != null && logs.Count > 0)
            {
                CreateErrorLogsReport();
                this.ExecuteDefaultChecks(date);
                this.CommentCommonIssues();

                string reportHeader = $"{reportName} ב{date.ToString("yyyy-MM-dd")}";
                htmlString = this.dailyReport.ToHtml(reportHeader);
            }
            return htmlString;
        }

        private void ExecuteDefaultChecks(DateTime date)
        {
            this.GlobalContext.LogEntry();

            this.CheckForMailsWithEmptyDocument(date);
            this.CheckCanceledSchedulerOperations(date);
            this.CheckForTwoMinutesLimitFailedJobs(date);
        }

        private void CommentCommonIssues()
        {
            this.GlobalContext.LogEntry();
            this.CommentCustomersWithoutPhoneNumberIssue();
            this.CommentFailedRequestsForCreatePortfolioInShenhav();
        }

        private void CommentCustomersWithoutPhoneNumberIssue()
        {
            this.GlobalContext.LogEntry();
            string logName = "Alt.Crm.Plugins.SMS.PreValidationCreateSms";
            string errorKey = "השדה טלפון נייד הוא שדה חובה";
            List<ApiSystemLog> selectedLogs = this.logs.Where(l => l.Name == logName && l.MessageBlock.IndexOf(errorKey) > 0)?.ToList();
            if (selectedLogs != null && selectedLogs.Count > 0)
            {
                dailyReport.AddComment(Issues.CustomersWithoutPhoneNumber);
            }
        }

        private void CommentFailedRequestsForCreatePortfolioInShenhav()
        {
            this.GlobalContext.LogEntry();
            string logName = "Alt.External.Jobs.CrmOutgoingApi.Program - alt_digitalformverification-Update";

            List<ApiSystemLog> selectedLogs = this.logs.Where(l => l.Name == logName)?.ToList();
            if (selectedLogs != null && selectedLogs.Count > 0)
            {
                foreach (var log in selectedLogs)
                {
                    List<string> content = new List<string>();
                    content.Add(this.CreateRecordUrl(log.TargetLogicalName, log.TargetId));

                    this.dailyReport.AddComment(Issues.FailedRequestForCreatePortfolioInShenhav, content);
                }
            }
        }

        private void CreateErrorLogsReport()
        {
            this.GlobalContext.LogEntry();
            this.dailyReport = new DailyErrorLogsReport();

            ErrorDAL errorKeyDAL = new ErrorDAL(this.GlobalContext);
            this.errorKeys = errorKeyDAL.GetSystemLogErrorKeys();

            foreach (var log in logs)
            {
                ErrorLogModel errorModel = CreateErrorLogModelFromLog(log, errorKeys);
                if (errorModel != null)
                {
                    dailyReport.KnownErrors.Add(errorModel);
                }
                else
                {
                    errorModel = CreateErrorLogModelFromLog(log, String.Empty);
                    dailyReport.UnknownErrors.Add(errorModel);
                }
            }
        }

        private ErrorLogModel CreateErrorLogModelFromLog(ApiSystemLog log, List<ApiError> errorKeys)
        {
            ErrorLogModel errorLogModel = null;
            foreach (var errorKey in errorKeys)
            {
                if (log.MessageBlock.Contains(errorKey.ErrorKey))
                {
                    string message = HandleLogCustomDescriptionCreate(log, errorKey);
                    errorLogModel = CreateErrorLogModelFromLog(log, message);
                    errorLogModel.Message = errorKey.ErrorMessage;
                    return errorLogModel;
                }
            }
            return errorLogModel;
        }

        private ErrorLogModel CreateErrorLogModelFromLog(ApiSystemLog log, string errorDescription)
        {
            ErrorLogModel errorLogModel = new ErrorLogModel()
            {
                Name = log.Name,
                Url = this.CreateRecordUrl(log.LogicalName, log.Id.ToString()),
                Description = errorDescription
            };

            errorLogModel.MessageLevel = ((MessageLevel)log.MessageLevelCode.Value).GetDescriptionAttribute();
            errorLogModel.Source = ((EntryPointTypeCode)log.EntryPointTypeCode.Value).GetDescriptionAttribute();

            return errorLogModel;
        }

        private string HandleLogCustomDescriptionCreate(ApiSystemLog log, ApiError customError)
        {
            string message = customError.Description;
            switch (customError.ErrorKey)
            {
                case ".JsonReaderException":
                case ".JsonSerializationException":
                    {
                       // message += this.GetRequiredFields(log);
                        break;
                    }
                case "Error code:-100000003":
                    {
                       // message = this.GetRequiredFields(log);
                        break;
                    }
                default:
                    break;
            }
            return message;
        }

        private void CheckForMailsWithEmptyDocument(DateTime date)
        {
            this.GlobalContext.LogEntry();

            EmailDAL emailDal = new EmailDAL(this.GlobalContext);
            List<ApiEmail> emails = emailDal.GetEmailsWithEmptyAttachmentsByDate(date);

            if (emails != null && emails.Count > 0)
            {
                List<string> emailsUrls = new List<string>();
                foreach (var entity in emails)
                {
                    emailsUrls.Add(CreateRecordUrl(entity.LogicalName, entity.Id.ToString()));
                }
                this.dailyReport.AddComment(Issues.EmptyDocument, emailsUrls);
            }
        }

        private void CheckForTwoMinutesLimitFailedJobs(DateTime date)
        {
            this.GlobalContext.LogEntry();
            List<ApiAsyncOperation> twoMinuteLimitFailedJobs = this.GetFailedJobs(date).Where(j => j.FriendlyMessage.Contains("plug-in within the 2-minute limit."))?.ToList();
            if (twoMinuteLimitFailedJobs != null && twoMinuteLimitFailedJobs.Count > 0)
            {
                foreach (ApiAsyncOperation job in twoMinuteLimitFailedJobs)
                {
                    List<string> content = new List<string>();

                    if (job.RegardingObject != null)
                    {
                        content.Add(job.StartedOn.Value.ToString());
                        content.Add(job.Name);
                        content.Add(this.CreateRecordUrl(job.RegardingObject.LogicalName, job.RegardingObject.Id.ToString()));
                    }
                    this.dailyReport.AddComment(Issues.TwoMinuteLimitFailedJobs, content);
                }
            }
        }

        private void CheckCanceledSchedulerOperations(DateTime date)
        {
            this.GlobalContext.LogEntry();
            List<ApiScheduledOperation> cancledOperations = this.GetScheduledOperations(date)
                .Where(o => o.StatusCode == (int)ScheduledOperationStatusCode.Canceled)?.ToList();

            if (cancledOperations != null && cancledOperations.Count > 0)
            {
                foreach (ApiScheduledOperation operation in cancledOperations)
                {
                    List<string> content = new List<string>
                    {
                        operation.Name,
                        this.CreateRecordUrl(operation.LogicalName, operation.Id.ToString())
                    };
                    this.dailyReport.AddComment(Issues.CanceledSchedulerOperations, content);
                }
            }
        }

        private List<ApiScheduledOperation> GetScheduledOperations(DateTime date)
        {
            this.GlobalContext.LogEntry();
            if (this.scheduledOperations == null)
            {
                ScheduledOperationDAL scheduledOperationDAL = new ScheduledOperationDAL(this.GlobalContext);
                List<ApiScheduledOperation> apiScheduledOperations = scheduledOperationDAL.GetScheduledOperationsByDate(date);
                this.scheduledOperations = apiScheduledOperations != null ? apiScheduledOperations : new List<ApiScheduledOperation>();
            }
            return this.scheduledOperations;
        }

        private List<ApiAsyncOperation> GetFailedJobs(DateTime date)
        {
            this.GlobalContext.LogEntry();
            if (this.failedJobs == null)
            {
                AsyncOperationDAL asyncOperationDal = new AsyncOperationDAL(this.GlobalContext);
                List<ApiAsyncOperation> apiAsyncOperations = asyncOperationDal.GetFailedJobsByDate(date);
                this.failedJobs = apiAsyncOperations != null ? apiAsyncOperations : new List<ApiAsyncOperation>();
            }
            return this.failedJobs;
        }

        public string SubstringProperty(ApiSystemLog apiSystemLog, string propertyName, string to)
        {
            string stringToReturn = String.Empty;
            int indexOfProperty = apiSystemLog.MessageBlock.IndexOf(propertyName);
            if (indexOfProperty > 0)
            {
                string str = apiSystemLog.MessageBlock.Substring(indexOfProperty);
                int index = str.IndexOf(to);
                stringToReturn += apiSystemLog.MessageBlock.Substring(indexOfProperty, index + 1);
            }
            return stringToReturn;
        }

        public CustomEntityReference ConvertToLookupProperty(ApiSystemLog systemLog, string propertyName)
        {
            try
            {
                string lookupProperty = this.SubstringProperty(systemLog, propertyName, "}");
                int index = lookupProperty.IndexOf(':') + 1;
                var obj = lookupProperty.Substring(index, lookupProperty.Length - index).Replace("{", "").Replace("}", "");
                var str = obj.Split(',');
                Guid id = new Guid(str[0].Split(':')[1].Trim());
                string logicalName = str[1].Split(':')[1].Trim();
                return new CustomEntityReference()
                {
                    LogicalName = logicalName,
                    Id = id,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public CustomEntityReference ConvertTargetToLookupProperty(ApiSystemLog systemLog)
        {
            CustomEntityReference lookup = null;
            int indexOf = systemLog.MessageBlock.IndexOf("TargetEntity : ");

            if (indexOf > 0)
            {
                string property = this.SubstringProperty(systemLog, "(Entity:", ",");

                if (!string.IsNullOrEmpty(property))
                {
                    int lastIndexOf = property.IndexOf("PrimaryEntityId");
                    string entityName = property.Substring(8, lastIndexOf - 8).Trim();
                    string entityId = property.Split(':')[2].Replace(',', ' ').Trim();
                    lookup = new CustomEntityReference() { Id = new Guid(entityId), LogicalName = entityName };
                }
            }
            return lookup;
        }

        public string GetEmailDitails(ApiSystemLog log)
        {
            string from = this.SubstringProperty(log, "\"sender", ",");
            string to = this.SubstringProperty(log, "torecipients", ",");

            return $" - {from.Replace('"', ' ')} {to.Replace('"', ' ')}";
        }

        public string GetRequiredFields(ApiSystemLog log)
        {
            string message = log.MessageBlock;
            int startIndex = 0;
            int indexOf = 0;
            string str = String.Empty;

            do
            {
                indexOf = message.IndexOf("השדה", startIndex);
                if (indexOf > 0)
                {
                    int lastIndexOf = message.IndexOf("שדה חובה");
                    if (lastIndexOf >= 0)
                    {
                        string subStr = message.Substring(indexOf, lastIndexOf + 9 - indexOf);
                        str += subStr;
                        startIndex = lastIndexOf + 9;
                    }
                }

            } while (indexOf > 0);

            return str;
        }

        public string CreateLookupPropertyUrl(ApiSystemLog log, string organizationUrl, string propertyName)
        {
            string property = this.SubstringProperty(log, propertyName, "}");
            int index = property.IndexOf(':') + 1;
            var obj = property.Substring(index, property.Length - index).Replace("{", "").Replace("}", "");
            var str = obj.Split(',');
            string entityName = str[1].Split(':')[1].Trim();
            string entityId = str[0].Split(':')[1].Trim();
            return $"{organizationUrl}/main.aspx?etn={entityName}&id={entityId}&newWindow=true&pagetype=entityrecord";
        }

        private string CreateRecordUrl(string entityName, string id)
        {
            return $"{this.GlobalContext.OrganizationUrl}/main.aspx?etn={entityName}&id={id}&newWindow=true&pagetype=entityrecord";
        }
    }
}
