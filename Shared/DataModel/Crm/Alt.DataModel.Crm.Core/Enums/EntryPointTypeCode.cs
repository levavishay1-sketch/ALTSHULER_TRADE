
using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum EntryPointTypeCode
    {
        [Description("פלגין")]
        Plugin = 1,
        [Description("תהליך")]
        Workfolw = 2,
        [Description("צד שלישי")]
        ThirdParty = 3,
        [Description("צד לקוח")]
        ClientSide = 4
    }
}
