using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum MessageLevel
    {
        [Description("מידע")]
        Information = 1,

        [Description("אזהרה")]
        Warning = 2,

        [Description("שגיאה")]
        Error = 3,

        [Description("שגיאה קריטית")]
        Critical = 4
    }
}
