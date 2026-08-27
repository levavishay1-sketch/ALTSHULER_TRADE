using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;

namespace Alt.Test.CrmApi
{
    [TestClass]
    public class DigitalFormApiUnitTest : BaseUnitTest
    {
        string controllerName = "DigitalForms";
        string routePath = "api/digitalforms";

        [TestMethod]
        [TestCategory("Integration")]
        public void CreateDigitalForm_Success()
        {
            ApiDigitalForm digitalFormToCreate = new ApiDigitalForm
            {
                DigitalFormLink = "https://digital-orgtest.as-invest.co.il/Auth?AppName=Trade_Onboarding&FormToken=991449f0-4ae0-4758-b305-4314a0d43238&UserToken=", // replace
                DigitalFormIdentityNumber = "42779",
                DigitalFormStatus = new ApiDigitalFormStatus
                {
                    Code = "01"
                },
                DigitalFormType = 1
            };

            var digitalFormController = new DigitalFormsController();
            HandleControllerSetup(digitalFormController, HttpMethod.Post);

            var result = digitalFormController.Post(digitalFormToCreate);
            var returnedValue = ((ObjectContent)((ResponseMessageResult)result).Response.Content).Value;

            Assert.AreEqual(((ResponseMessageResult)result).Response.StatusCode, HttpStatusCode.Created);
            Assert.IsInstanceOfType(returnedValue, typeof(Guid));
        }

        public void DigitalForm_SentToVerification()
        {
            ApiDigitalForm digitalFormToUpdate = new ApiDigitalForm
            {
                Id = new Guid("a50fdb71-d2f4-ee11-a1fd-000d3ad94186"),
                DigitalFormStatus = new ApiDigitalFormStatus
                {
                    Code = "10"
                },
                DigitalFormType = 1,
                JoiningForm = new ApiDigitalFormVerification
                {
                    CreditRequestExistsCode = 2,
                    ShortSaleRequestApprovaIExistsCode = 2,
                    OptionExerciseRequestApprovalExistsCode = 1,
                    VotingDocumentsCode = 1,
                    AccountVerificationCode = "6054",
                    QuarterlyReportsSendingCode = 2,
                    Bank = new ApiBank
                    {
                        Code = "31"
                    },
                    Branch = new ApiBranch
                    {
                        Code = "31-315"
                    },
                    BankAccountNumber = "3150315",
                    BankAccountName = "הבינלאומי - 315 - 3150315",
                    PortfolioOwners = new List<ApiAccountHolder>
                    {
                        new ApiAccountHolder
                        {
                            IdentificationNumber = "123456789",
                            IdentificationTypeCode = 1,
                            IDIssueDate = DateTime.Parse("2022-07-11T05:00:00Z"),
                            FirstName = "אחד",
                            LastName = "שתיים",
                            FirstNameEng = "One",
                            LastNameEng = "Two",
                            BirthDate = DateTime.Parse("1900-01-01T05:00:00Z"),
                            GenderCode = 1,
                            Email = "testingtest@gmail.com",
                            MobilePhone = "05012515342",
                            HouseNumber = "4",
                            FlatNumber = "",
                            AccountHolderTypeCode = 1,
                            UserCharacteristicCode = 2,
                            PerformVerificationCode = 1,
                            PerformVerificationDate = DateTime.Parse("2024-04-07T14:45:45.417Z"),
                            City = new ApiCity
                            {
                                Code = "1200"
                            },
                            Street = new ApiStreet
                            {
                                Code = "1200-423"
                            },
                            BirthCountry = new ApiCountry
                            {
                                Code = "900"
                            },
                            Country = new ApiCountry
                            {
                                Code = "900"
                            },
                            IdentificationIssuingCountry = new ApiCountry
                            {
                                Code = "900"
                            },
                            AllowMarketingContent = false,
                            DigitalVisualRecognitionCode = 2,
                            BeneficiarySigningDeclarationCode = 1,
                            MainAccountHolder = true,
                            BeneficiaryDeclarationCode = 1,
                            IsraeliResidency = true,
                            USPersonDeclaration = true,
                            ChangeIsraeliResidency = false,
                            ChangeUSPersonDeclaration = false,
                            ChangeForeignTaxResidency = false,
                            ForeignTaxResidency = true,
                            CheckTerrorOrganizationCode = 2,
                            AU10tixSessionID = "31632B2E848248F0BDA3EA75C553C377",
                            KYC = new ApiKYC
                            {
                                FundsDepositFrequencyForecastCode = 1,
                                FundsSourceCode = new List<int>
                                {
                                    1
                                },
                                TotalDepositForecastPerYearCode = 1,
                                TotalWithdrawalOrTransferForecastCode = 5,
                                YearlyTotalWithdrawalTransferForecastCode = 1,
                                TransactionsToFromThirdParty = false,
                                EmploymentTypeCode = 4,
                                TradeRelationRiskTerritory = false,
                                BankServiceDenial = false,
                                MonthlyIncomeLevelNISCode = 1,
                                PublicPerson = false,
                                AdditionalAccountExistsAtAltshuler = false,
                            }
                        }
                    }
                }
            };

            var digitalFormController = new DigitalFormsController();
            HandleControllerSetup(digitalFormController, HttpMethod.Put);

            var result = digitalFormController.Post(digitalFormToUpdate);

            Assert.AreEqual(((ResponseMessageResult)result).Response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DigitalForm_IdentificationSucceeded()
        {
            ApiDigitalForm digitalFormToUpdate = new ApiDigitalForm
            {
                Id = new Guid("a50fdb71-d2f4-ee11-a1fd-000d3ad94186"),
                DigitalFormStatus = new ApiDigitalFormStatus
                {
                    Code = "08"
                },
                DigitalFormType = 1
            };

            var digitalFormController = new DigitalFormsController();
            HandleControllerSetup(digitalFormController, HttpMethod.Put);

            var result = digitalFormController.Post(digitalFormToUpdate);

            Assert.AreEqual(((ResponseMessageResult)result).Response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DigitalForm_IdentificationFailed()
        {
            ApiDigitalForm digitalFormToUpdate = new ApiDigitalForm
            {
                Id = new Guid("a50fdb71-d2f4-ee11-a1fd-000d3ad94186"),
                DigitalFormStatus = new ApiDigitalFormStatus
                {
                    Code = "09"
                },
                DigitalFormType = 1
            };

            var digitalFormController = new DigitalFormsController();
            HandleControllerSetup(digitalFormController, HttpMethod.Put);

            var result = digitalFormController.Post(digitalFormToUpdate);

            Assert.AreEqual(((ResponseMessageResult)result).Response.StatusCode, HttpStatusCode.OK);
        }

        public void DigitalForm_AbandonedProcess()
        {
            ApiDigitalForm digitalFormToUpdate = new ApiDigitalForm
            {
                Id = new Guid("a50fdb71-d2f4-ee11-a1fd-000d3ad94186"),
                AbandonedProcessBit = true,
                AbandonmentPage = "מסך הכנה לאותנטיקס"
            };

            var digitalFormController = new DigitalFormsController();
            HandleControllerSetup(digitalFormController, HttpMethod.Put);

            var result = digitalFormController.Post(digitalFormToUpdate);

            Assert.AreEqual(((ResponseMessageResult)result).Response.StatusCode, HttpStatusCode.OK);
        }
    }
}
