using System.Collections.Generic;
using System.Dynamic;

namespace Alt.DataModel.ExernalServices.ANVIL
{
    public class AnvilTradeAgreement
    {
        public dynamic ToDynamic()
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var propertyInfo in this.GetType().GetProperties())
            {
                var currentValue = propertyInfo.GetValue(this);
                expando.Add(propertyInfo.Name, currentValue);
            }
           return expando as ExpandoObject;
        }
    }

    /// <summary>
    /// הסכם כללי ונהנים - code 1
    /// </summary>
    public class AnvilGeneralAgreementAppendixAB : AnvilTradeAgreement
    {
        public string ShenhavAccountNumber { get; set; }
        public string CreatedOn { get; set; }
        public string AccountHolder1Name { get; set; }
        public string AccountHolder2Name { get; set; }
        public string AccountHolder1ID { get; set; }
        public string AccountHolder2ID { get; set; }
        public string AccountHolder1DateOfBirth { get; set; }
        public string AccountHolder2DateOfBirth { get; set; }
        public bool AccountHolder1Male { get; set; }
        public bool AccountHolder1Female { get; set; }
        public bool AccountHolder2Male { get; set; }
        public bool AccountHolder2Female { get; set; }
        public string AccountHolder1MobilePhone { get; set; }
        public string AccountHolder2MobilePhone { get; set; }
        public string AccountHolder1CityID { get; set; }
        public string AccountHolder2CityID { get; set; }
        public string AccountHolder1Address { get; set; }
        public string AccountHolder2Address { get; set; }
        public string AccountHolder1PostalCode { get; set; }
        public string AccountHolder2PostalCode { get; set; }
        public string AccountHolder1Email { get; set; }
        public string AccountHolder2Email { get; set; }
        public bool WithMail { get; set; }
        public bool WithIsraelPost { get; set; }
        public string CityID { get; set; }
        public string StreetID { get; set; }
        public string PostalCode { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
        public string BankAccountNumber { get; set; }
        public string AccountVerificationCode { get; set; }
        public bool AllowMarketingContentBit1 { get; set; }
        public bool AllowMarketingContentBit0 { get; set; }
        public bool VotingDocumentsCode1 { get; set; }
        public bool VotingDocumentsCode2 { get; set; }
        public bool VotingDocumentsCode3 { get; set; }
        public bool VotingDocumentsCode4 { get; set; }
        public bool VotingDocumentsCode5 { get; set; }
        public bool ForeignTaxResidencyBit1 { get; set; }
        public string ForeignTaxResidencyBit1Name { get; set; }
        public bool ForeignTaxResidencyBit2 { get; set; }
        public string ForeignTaxResidencyBit2Name { get; set; }
        public bool UspersonDeclarationBit1 { get; set; }
        public string UspersonDeclarationBit1Name { get; set; }
        public bool UspersonDeclarationBit2 { get; set; }
        public string UspersonDeclarationBit2Name { get; set; }
        public string ApprovedBy1 { get; set; }
        public string ApprovedByID1 { get; set; }
        public string ApprovedBy2 { get; set; }
        public string ApprovedByID2 { get; set; }
        public string SignatureDate { get; set; }
        public bool BeneficiaryDeclarationCode1 { get; set; }
        public bool BeneficiaryDeclarationCode7 { get; set; }
        public bool BeneficiaryDeclarationCode6 { get; set; }
        public bool BeneficiaryDeclarationCode5 { get; set; }
        public bool BeneficiaryDeclarationCode4 { get; set; }
        public bool BeneficiaryDeclarationCode3 { get; set; }
        public bool BeneficiaryDeclarationCode2 { get; set; }
        public bool BeneficiaryInAccount { get; set; }
        public string Beneficiary1Name { get; set; }
        public string Beneficiary2Name { get; set; }
        public string Beneficiary1ID { get; set; }
        public string Beneficiary2ID { get; set; }
        public bool NoControllingOwner { get; set; }
        public bool ControllingOwner { get; set; }
        public string ControllingOwner1Name { get; set; }
        public string ControllingOwner2Name { get; set; }
        public string ControllingOwner1ID { get; set; }
        public string ControllingOwner2ID { get; set; }
        public string ControllingOwner1DateOfBirth { get; set; }
        public string ControllingOwner2DateOfBirth { get; set; }
        public bool ControllingOwner1Male { get; set; }
        public bool ControllingOwner2Male { get; set; }
        public bool ControllingOwner2Female { get; set; }
        public bool ControllingOwner1Female { get; set; }
        public bool ChkDetails { get; set; }
        public string CommissionName1 { get; set; }
        public string CommissionName6 { get; set; }
        public string CommissionName5 { get; set; }
        public string CommissionName4 { get; set; }
        public string CommissionName3 { get; set; }
        public string CommissionName2 { get; set; }
        public string CommissionName8 { get; set; }
        public string CommissionName9 { get; set; }
        public string CommissionName10 { get; set; }
        public string CommissionName11 { get; set; }
        public string CommissionName12 { get; set; }
        public string CommissionName7 { get; set; }
        public string CommissionAmount1 { get; set; }
        public string CommissionAmount2 { get; set; }
        public string CommissionAmount3 { get; set; }
        public string CommissionAmount4 { get; set; }
        public string CommissionAmount5 { get; set; }
        public string CommissionAmount6 { get; set; }
        public string CommissionAmount7 { get; set; }
        public string CommissionAmount8 { get; set; }
        public string CommissionAmount9 { get; set; }
        public string CommissionAmount10 { get; set; }
        public string CommissionAmount11 { get; set; }
        public string CommissionAmount12 { get; set; }
        public string Details1 { get; set; }
        public string Details2 { get; set; }
        public string Details3 { get; set; }
        public string Details4 { get; set; }
        public string Details5 { get; set; }
        public string Details6 { get; set; }
        public string Details7 { get; set; }
        public string Details8 { get; set; }
        public string Details9 { get; set; }
        public string Details10 { get; set; }
        public string Details11 { get; set; }
        public string Details12 { get; set; }
        public string Israel { get; set; }

    }

    /// <summary>
    /// אלטשולר שחם טרייד - הסכם כללי + נספחים א' וב + נהנים + נספח ג - code 2
    /// </summary>
    public class AnvilAgreementAppendixABC : AnvilGeneralAgreementAppendixAB
    {
        public string LineCreditLimitRequestMNY { get; set; }
        public string CreditAmountNISRequestMNY { get; set; }
        public string LineWriteOptionsRequestMNY { get; set; }
        public string LineStockShortRequestMNY { get; set; }
        public string LineAggregateCreditLimitMNY { get; set; }
        public string CreditAmountNISMNY { get; set; }
        public string LineWriteOptionsMNY { get; set; }
        public string LineStockShortMNY { get; set; }
        public string OpenPositionLimit { get; set; }
    }

    /// <summary>
    /// אלטשולר שחם טרייד - הסכם כללי + נספחים א' וב + נהנים + נספח ג + נספח ד - code 4
    /// </summary>
    public class AnvilAgreementAppendixABCD : AnvilAgreementAppendixABC
    {
        public string Approvedby1 { get; set; }
        public string Approvedby2 { get; set; }
        public string shenhavaccountnumber { get; set; }
        public string createdon { get; set; }
        public string AccountHolderName { get; set; }
    }

    /// <summary>
    /// אלטשולר שחם טרייד - הסכם כללי + נספחים א' וב + נהנים + נספח ה - code 5
    /// </summary>
    public class AnvilAgreementAppendixABE : AnvilGeneralAgreementAppendixAB
    {
        public bool chkDetails { get; set; }
    }

    /// <summary>
    /// אלטשולר שחם טרייד - הסכם כללי + נספחים א' וב + נהנים + נספח ג + נספח ד + נספח ה - code 6
    /// </summary>
    public class AnvilAgreementAppendixABCDE : AnvilAgreementAppendixABCD
    {
        public bool chkDetails { get; set; }
    }

    /// <summary>
    /// אלטשולר שחם טרייד - הסכם כללי + נספחים א' וב + נהנים + נספח ג + נספח ה - code 7
    /// </summary>
    public class AnvilAgreementAppendixABCE : AnvilAgreementAppendixABC
    {
        public bool chkDetails { get; set; }
    }

    /// <summary>
    /// "אלטשולר שחם טרייד - הסכם כללי + נספחים א' וב + נהנים + נספח ד + נספח ה - code 8
    /// </summary>
    public class AnvilAgreementAppendixABDE : AnvilGeneralAgreementAppendixAB
    {
        public string Approvedby1 { get; set; }
        public string Approvedby2 { get; set; }
        public string shenhavaccountnumber { get; set; }
        public string createdon { get; set; }
        public string AccountHolderName { get; set; }
        public bool chkDetails { get; set; }
    }
}

