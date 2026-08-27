using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum AccountTypeTaxCode
    {
        [Description("IsraeliResident")]
        IsraeliResident = 1,
        [Description("IsraeliCorporation")]
        IsraeliCorporation = 2,
        [Description("IndividualForeignResident")]
        IndividualForeignResident = 3,
        [Description("CorporateForeignResident")]
        CorporateForeignResident = 4,
        [Description("ReturningResident")]
        ReturningResident = 5,
        [Description("NewResident/SeniorReturningResident")]
        NewResidentOrSeniorReturningResident = 6,
        [Description("Non-Profitable/ProvidentFund")]
        NonProfitableOrProvidentFund = 7
    }
}
