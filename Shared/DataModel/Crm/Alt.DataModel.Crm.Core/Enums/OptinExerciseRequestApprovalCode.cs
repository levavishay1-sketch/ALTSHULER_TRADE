using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum OptinExerciseRequestApprovalCode
    {
        [Description("N")]
        No = 1,
        [Description("Y")]
        SellAndBayOnly = 2,
        [Description("Y")]
        IncludeWriteOptions = 3
    }
}
