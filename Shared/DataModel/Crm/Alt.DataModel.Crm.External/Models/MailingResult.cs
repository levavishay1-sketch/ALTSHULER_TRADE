using Alt.DataModel.Crm.Core.Enums;
using System;
using System.ComponentModel;

namespace Alt.DataModel.Crm.External.Models
{
    public class MailingResult
    {
        [Description("מזהה מקור")]
        public Guid SourceId { get; set; }
        [Description("יעד")]
        public string Target { get; set; }
        [Description("סוג")]
        public TemplateType? TemplateType { get; set; }
        [Description("תוצאת הצלחה")]
        public string SuccessResult { get; set; }
        [Description("תוצאת כישלון")]
        public string FailedResult { get; set; }
    }
}
