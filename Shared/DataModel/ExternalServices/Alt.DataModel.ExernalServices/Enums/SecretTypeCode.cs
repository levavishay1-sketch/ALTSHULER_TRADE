using System.ComponentModel;

namespace Alt.DataModel.ExernalServices.Enums
{
    public enum SecretTypeCode
    {
        [Description("password")]
        Password = 1,
        [Description("key")]
        Key = 2
    }
}
