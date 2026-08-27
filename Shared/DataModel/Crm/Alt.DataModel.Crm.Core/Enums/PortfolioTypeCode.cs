
using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum PortfolioTypeCode
    {
        [Description("P")]
        Private = 1,

        [Description("C")]
        Corporation = 2,

        [Description("N")]
        Nostro = 3,

        [Description("I")]
        Institutional = 4,

        [Description("M")]
        Managed = 5,

        [Description("O")]
        Other = 6
    }
}
