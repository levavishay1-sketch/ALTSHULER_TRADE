using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum PerformVerificationCode
    {
        [Description("Online")]
        Digital = 1,
        [Description("Video")]
        Visual = 2,
        [Description("Physical")]
        FaceToFace = 3,
        [Description("Other")]
        Other = 4
    }
}
