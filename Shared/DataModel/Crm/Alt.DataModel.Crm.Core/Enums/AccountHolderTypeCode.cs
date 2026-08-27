using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum AccountHolderTypeCode
    {
        [Description("AccountOwner")]
        Owner = 1,
        [Description("AccountRelated")]
        PowerOfAttorney = 2,
        [Description("Beneficiary")]
        Beneficiary = 3,
        [Description("Shareholder")]
        ShareHolder = 4,
        [Description("AppointedbyOrder")]
        AppointedByOrder = 5,
        [Description("BeneficiaryShareholder")]
        BeneficiaryShareHolder = 6,
        [Description("RelatedCorporationShareholder")]
        RelatedCorporationShareHolder = 7,
        [Description("Custodian")]
        Custodian = 8,
        [Description("AllowInfo")]
        AuthorizedToReceiveInformation = 9,
        [Description("AllowAction")]
        AuthorizedToOperation = 10,
        [Description("Guardian")]
        Guardian = 11
    }
}
