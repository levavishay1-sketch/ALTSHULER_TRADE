using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum PortfolioTypeTaxCode
    {
        [Description("Israeli Resident")]
        IsraeliResident, 
        IsraeliCorporation, 
        IndividualForeignResident, 
        CorporateForeignResident,
        ReturningResident, 
        NewResident_SeniorReturningResident,
        NonProfitable_ProvidentFund
    }
}
