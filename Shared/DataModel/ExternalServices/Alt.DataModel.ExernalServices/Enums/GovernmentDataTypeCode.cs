using System.ComponentModel;

namespace Alt.DataModel.ExernalServices.Enums
{
    public enum GovernmentDataTypeCode
    {
        [Description("streets")]
        Streets,

        [Description("cities")]
        Cities,

        [Description("banks")]
        Banks,

        [Description("bank branches")]
        Branches,

        [Description("competitors")]
        Competitors,

        [Description("countries")]
        Countries
    }
}
