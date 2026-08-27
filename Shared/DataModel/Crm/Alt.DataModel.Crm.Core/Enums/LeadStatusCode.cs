
namespace Alt.DataModel.Crm.Core.Enums
{
    public enum LeadStatusCode
    {
        New = 1,
        InProgress = 2,
        Qualified = 3,
        Other = 4,
        NoResponseMultipleAttempts = 5,
        RequestedBenefitsNotApproved = 6,
        CanceledAutomaticallyInOutSystem = 7,
        DoubleLead = 100000000,
        RepresentativeInitiativeAffiliateAccount = 100000001,
        RepresentativeInitiativeCorporateAccount = 100000002,
        NotInterestedInOpeningAnAccount = 100000003,
        DoesNotMeetMinimumDeposit = 100000004,
        ExistingCustomerTransferredToCustomerRelations = 100000005,
        InterestedInManagedPortfolio = 100000006,
        UnderAgeEighteen = 100000007,
        InformationProvidedAndCurrentlyNotRelevant = 100000008,
        OpenedByMistake = 100000009,
        OpenedAccountWithAnotherStockExchangeMember = 100000010,
        UnitedStateResident = 100000011,
        Foreigner = 100000012,
        Disqualified = 157350001,
        RepresentativeInitiativeUnder18 = 157350002,
        RepresentativeInitiativeForeignCountryTaxResidency = 157350003,
        RepresentativeInitiativeKosherPhone = 157350004,
        RepresentativeInitiativeUSPerson = 157350005,
        RepresentativeInitiativeNoValidIdentification = 157350006,
        RepresentativeInitiativeInvalidPhoneNumber = 157350007,
        RepresentativeInitiativeNotGettingAlongWithTheProcess = 157350008,
        CorporationAccount = 157350009,
        AffiliateAccount = 157350010
    }
}
