using Alt.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Alt.DataModel.Crm.External.Models
{
    public class ETLLogMessageBlock
    {
        public List<ETLCounter> counters { get; set; }
        public List<ETLWarning> warnings { get; set; }
        [Description("שגיאת כישלון של ממשק")]
        public string exception { get; set; }
        public int? errorCode { get; set; }
        public string description { get; set; }

        public string htmlWithoutWarnings { get; set; }
        public string html { get; set; }

        public void ParseToHtml(out bool isExceededLength)
        {
            string exception = string.Empty;
            string description = string.Empty;
            string countersTableHeader = string.Empty;
            string countersTable = string.Empty;
            string warningsTableHeader = string.Empty;
            string warningsTable = string.Empty;

            HtmlBuilder htmlBuilder = new HtmlBuilder();

            if (counters != null && counters.Count > 0)
            {
                countersTableHeader = htmlBuilder.CreateHeader("מוני זרימות הנתונים");
                countersTable = htmlBuilder.CreateTable<ETLCounter>(this.counters);
            }
            if (warnings != null && warnings.Count > 0)
            {
                warningsTableHeader = htmlBuilder.CreateHeader("אזהרות בריצת הממשק");
                warningsTable = htmlBuilder.CreateTable<ETLWarning>(this.warnings);
            }
            if (!string.IsNullOrWhiteSpace(this.description))
            {
                description = htmlBuilder.CreateHeader(this.description);
            }
            if (!string.IsNullOrWhiteSpace(this.exception))
            {
                exception = htmlBuilder.CreateParagraph(this.exception);
            }

            this.html = $"{exception}{description}{countersTableHeader}{countersTable}{warningsTableHeader}{warningsTable}";
            if (this.html.Length > 500000)
            {
                this.htmlWithoutWarnings = $"{exception}{description}{countersTableHeader}{countersTable}";
                isExceededLength = true;
            }
            else
            {
                isExceededLength = false;
            }
        }
    }
    public class ETLCounter
    {
        [Description("שם זרימת נתונים")]
        public string DataFlowName { get; set; }
        [Description("יצירות")]
        public int? Created { get; set; }
        [Description("עדכונים")]
        public int? Updated { get; set; }
        [Description("ללא שינויים")]
        public int? NoNeededChanges { get; set; }
        [Description("שגיאות")]
        public int? Errors { get; set; }
        [Description("סה'כ")]
        public int? Pipeline { get; set; }
    }

    public class ETLWarning
    {
        [Description("שם זרימת נתונים")]
        public string DataFlowName { get; set; }
        [Description("מפתח רשומה")]
        public string RecordKey { get; set; }
        [Description("הודעת אזהרה")]
        public string WarningMessage { get; set; }
        [Description("רמת אזהרה")]
        public int? WarningLevel { get; set; }
        [Description("קוד שגיאה")]
        public int? ErrorCode { get; set; }
        [Description("קישור לרשומה")]
        public string RecordUrl { get; set; }
    }
}
