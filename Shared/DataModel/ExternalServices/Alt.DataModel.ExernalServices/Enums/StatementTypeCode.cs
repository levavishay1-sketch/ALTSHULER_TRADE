
using System.ComponentModel;

namespace Alt.DataModel.ExernalServices.Enums
{
    public enum StatementTypeCode
    {
        [Description("Balances")]
        Balances = 1,
        [Description("Positions")]
        Positions = 2,
        [Description("StornoOperations")]
        StornoOperations = 3
    }
}
