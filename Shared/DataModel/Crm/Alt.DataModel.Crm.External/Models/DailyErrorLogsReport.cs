using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.External.Models
{
    public class DailyErrorLogsReport
    {
        private readonly string commentsTitle = "שים לב!";
        public List<ErrorLogModel> KnownErrors { get; set; } = new List<ErrorLogModel>();
        public List<ErrorLogModel> UnknownErrors { get; set; } = new List<ErrorLogModel>();

        private List<ErrorLogModel> errorsToWrite;
        public List<ErrorLogModel> ErrorsToWrite
        {
            get
            {
                if (KnownErrors != null && KnownErrors.Count > 0)
                {
                    errorsToWrite = this.GroupKnownErrorsLogs();
                }
                if (UnknownErrors != null && UnknownErrors.Count > 0)
                {
                    if (errorsToWrite != null)
                    {
                        errorsToWrite.AddRange(UnknownErrors);
                    }
                    else
                    {
                        errorsToWrite = UnknownErrors;
                    }
                }
                return errorsToWrite;
            }
            protected set { errorsToWrite = value; }
        }

        Dictionary<string, List<Comment>> commentsToWrite = new Dictionary<string, List<Comment>>();

        public string ToHtml(string reportHeader)
        {
            HtmlBuilder htmlBuilder = new HtmlBuilder();

            string header = htmlBuilder.CreateHeader(reportHeader);
            string errorsTable = htmlBuilder.CreateErrorLogsTable(this.ErrorsToWrite);
            string commentsList = htmlBuilder.CreateErrorLogsCommentsList(this.commentsToWrite, commentsTitle);

            return $"{header}{commentsList}{errorsTable}";
        }

        public void AddComment(string key, string value)
        {
            this.AddComment(new Comment()
            {
                Title = key,
                Content = !string.IsNullOrEmpty(value) ? new List<string>() { value } : null
            });
        }

        public void AddComment(string key)
        {
            this.AddComment(key, string.Empty);
        }

        public void AddComment(string key, List<string> values)
        {
            this.AddComment(new Comment()
            {
                Title = key,
                Content = values
            });
        }

        public void AddComment(Comment comment)
        {
            if (!this.commentsToWrite.ContainsKey(comment.Title))
            {
                var value = comment.Content != null ? new List<Comment>() { comment } : null;
                commentsToWrite.Add(comment.Title, value);
            }
            else if (comment.Content != null)
            {
                if (commentsToWrite[comment.Title] != null)
                {
                    if (!commentsToWrite[comment.Title].Contains(comment))
                    {
                        commentsToWrite[comment.Title].Add(comment);
                    }
                }
                else
                {
                    commentsToWrite.Add(comment.Title, new List<Comment>() { comment });
                }
            }
        }

        public bool ContainsComment(string key)
        {
            return this.commentsToWrite.ContainsKey(key);
        }

        private List<ErrorLogModel> GroupKnownErrorsLogs()
        {
            List<ErrorLogModel> logsToWrite = new List<ErrorLogModel>();
            var groupByName = this.KnownErrors.GroupBy(l => l.Name, l => l, (key, value) => new
            {
                Key = key,
                Logs = value
            });
            foreach (var nameGroup in groupByName)
            {
                var groupByMessage = nameGroup.Logs.GroupBy(l => l.Message, l => l, (key, value) => new
                {
                    Key = key,
                    Logs = value
                });
                foreach (var messageGroup in groupByMessage)
                {
                    ErrorLogModel logToWrite = messageGroup.Logs.FirstOrDefault();
                    logToWrite.Count = messageGroup.Logs.Count();
                    logsToWrite.Add(logToWrite);
                }
            }
            return logsToWrite;
        }
    }
}
