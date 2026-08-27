using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum IdentificationTypeCode
    {
        [Description("ID")]
        GovernmentId = 1,
        [Description("Passport")]
        Passport = 2,
        [Description("DriverLicense")]
        DrivingLicense = 4,
        [Description("CorporateID")]
        AccountNumber = 3,
        [Description("OtherPrivate")]
        OtherDetails = 5,
        [Description("Other ForeignCorporation")]
        OtherCorporateNumber = 6,
        [Description("ForeignCorporation")]
        ForeignCorporation = 7
    }
}
