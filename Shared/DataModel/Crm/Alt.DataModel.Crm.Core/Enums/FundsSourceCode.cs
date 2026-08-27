using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum FundsSourceCode
    {
        [Description("Salary")]
        Salary = 1,

        [Description("Savingsfromwork")]
        SavingsFromWork =2,

        [Description("Businessincome")]
        IncomeSourceBusiness = 3,

        [Description("Pensionsavings")]
        PensionSavings = 4,

        [Description("Benefitplansrenting")]
        BenefitPlansrenting = 5,

        [Description("Rent/Salerealestate")]
        RentingSellingRealEstate = 6,

        [Description("Inheritance")]
        Inheritance = 7,

        [Description("Gift")]
        Gift = 8,

        [Description("Tradingactivity")]
        TradingActivity = 9,

        [Description("Dividend")]
        Dividend = 10,

        [Description("Foreignterritory")]
        ForeignTerritory = 11,

        [Description("VirtualAssets")]
        VirtualAssets = 12,

        [Description("Other")]
        Else = 13
    }
}