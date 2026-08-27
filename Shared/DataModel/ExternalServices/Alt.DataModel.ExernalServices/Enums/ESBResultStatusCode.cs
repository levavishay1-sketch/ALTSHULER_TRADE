using System.ComponentModel;

namespace Alt.DataModel.ExernalServices.Enums
{
    public enum ESBResultStatusCode
    {
        [Description("OK")]
        Success = 0,
        [Description("ERROR")]
        Error = 1,
        [Description("Reject")]
        Reject = 2,
        NotFound = 3
    }
}
