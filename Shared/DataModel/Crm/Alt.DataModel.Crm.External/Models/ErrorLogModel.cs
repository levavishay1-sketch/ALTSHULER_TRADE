
using System.ComponentModel;

namespace Alt.DataModel.Crm.External.Models
{
   public class ErrorLogModel
    {
        [Description("הודעת שגיאה")]
        public string Message { get; set; }
        [Description("מקור")]
        public string Source { get; set; }
        [Description("שם לוג")]
        public string Name { get; set; }
        [Description("סוג הודעה")]
        public string MessageLevel { get; set; }
        [Description("כמות")]
        public int Count { get; set; }
        [Description("לינק")]
        public string Url { get; set; }
        [Description("")]
        public string TargetEntityUrl { get; set; }
        [Description("תיאור")]
        public string Description { get; set; }
    }
}
