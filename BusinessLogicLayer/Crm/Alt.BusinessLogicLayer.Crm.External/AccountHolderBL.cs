using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ANVIL;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.DataModel.ExternalServices.ESB;
using Alt.Framework;
using Alt.Framework.Extensions;
using Alt.Framework.External.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class AccountHolderBL : ExternalBLBase
    {
        Dictionary<string, ApiAccountHolder> accountHoldersToCreate;

        public AccountHolderBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult UpdateAccountHolder(ApiAccountHolder apiAccountHolder)
        {
            this.GlobalContext.LogEntry();
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            accountHolderDal.Update(apiAccountHolder);

            return new ActionResult();
        }

        public ActionResult CreateAccountHolders(List<ApiAccountHolder> apiAccountHolders, string joiningProcessNumber = null)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);

            this.accountHoldersToCreate = apiAccountHolders.ToDictionary(a => a.IdentificationNumber, a => a);
            for (int i = 0; i < apiAccountHolders.Count; i++)
            {
                Guid? accountHolderId = null;
                string apiAccountHolderToCreateIdentityNumber = apiAccountHolders[i].IdentificationNumber;

                if (accountHoldersToCreate[apiAccountHolderToCreateIdentityNumber].Id == null)
                {
                    if (!accountHolderDal.IsAccountHolderExist(apiAccountHolders[i], out accountHolderId))
                    {
                        if (apiAccountHolders[i].SpouseAccountHolder != null)
                        {
                            apiAccountHolders[i].SpouseAccountHolder.Id = this.HandleLinkedAccountHolder(apiAccountHolders[i].SpouseAccountHolder.IdentificationNumber, joiningProcessNumber);
                        }
                        if (apiAccountHolders[i].BeneficiarySpouseAccountHolder != null)
                        {
                            apiAccountHolders[i].BeneficiarySpouseAccountHolder.Id = this.HandleLinkedAccountHolder(apiAccountHolders[i].BeneficiarySpouseAccountHolder.IdentificationNumber, joiningProcessNumber);
                        }

                        var accountHolder = this.CreateAccountHolder(apiAccountHolders[i], joiningProcessNumber);
                        apiAccountHolders[i] = accountHolder;
                        accountHoldersToCreate[apiAccountHolderToCreateIdentityNumber] = accountHolder;
                    }
                    else
                    {
                        apiAccountHolders[i].Id = accountHolderId.Value;
                        accountHoldersToCreate[apiAccountHolderToCreateIdentityNumber] = apiAccountHolders[i];
                    }
                }

                if (apiAccountHolders[i].KYC != null && apiAccountHolders[i].KYC.Id == null)
                {
                    apiAccountHolders[i].KYC.Id = this.HandleAccountHolderKyc(apiAccountHolders[i]);
                }

            }
            return actionResult;
        }

        private Guid? HandleLinkedAccountHolder(string identificationNumber, string joiningProcessNumber = null)
        {
            this.GlobalContext.LogEntry($"Identification number: {identificationNumber}");
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);

            var linkedAccountHolder = accountHoldersToCreate[identificationNumber];
            Guid? accountHolderId;
            if (linkedAccountHolder.Id != null)
            {
                accountHolderId = linkedAccountHolder.Id;
            }
            else if (accountHolderDal.IsAccountHolderExist(linkedAccountHolder, out accountHolderId))
            {
                accountHoldersToCreate[identificationNumber].Id = accountHolderId;
            }
            else
            {
                if (linkedAccountHolder.BeneficiarySpouseAccountHolder != null)
                {
                    linkedAccountHolder.BeneficiarySpouseAccountHolder.Id = this.HandleLinkedAccountHolder(linkedAccountHolder.BeneficiarySpouseAccountHolder.IdentificationNumber, joiningProcessNumber);
                }
                linkedAccountHolder.SpouseAccountHolder = null;
                var createdAccountHolder = this.CreateAccountHolder(linkedAccountHolder, joiningProcessNumber);
                accountHolderId = createdAccountHolder.Id;
                accountHoldersToCreate[identificationNumber] = createdAccountHolder;
            }
            return accountHolderId;
        }

        public ApiAccountHolder CreateAccountHolder(ApiAccountHolder apiAccountHolder, string joiningProcessNumber = null)
        {
            this.GlobalContext.LogEntry();
            AccountHolderDAL accountHolderDAL = new AccountHolderDAL(this.GlobalContext);

            this.PerformVerificationSystemUser(apiAccountHolder);
            this.HandleCustomer(apiAccountHolder);
            apiAccountHolder.Id = accountHolderDAL.Create(apiAccountHolder);

            this.HanldePopulationRegisterCustomerVerification(apiAccountHolder, joiningProcessNumber);

            return apiAccountHolder;
        }

        private void PerformVerificationSystemUser(ApiAccountHolder apiAccountHolder)
        {
            this.GlobalContext.LogEntry();

            if (apiAccountHolder.PerformVerificationSystemUser != null)
            {
                if (apiAccountHolder.PerformVerificationSystemUser.Id == null
                    && !string.IsNullOrWhiteSpace(apiAccountHolder.PerformVerificationSystemUser.DomainName))
                {
                    SystemUserDAL systemUserDal = new SystemUserDAL(this.GlobalContext);
                    ApiSystemUser apiSystemUser = systemUserDal.GetActiveByAttribute("domainname", apiAccountHolder.PerformVerificationSystemUser.DomainName, new string[] { "systemuserid" }).FirstOrDefault();
                    if (apiSystemUser != null)
                    {
                        apiAccountHolder.PerformVerificationSystemUser.Id = apiSystemUser.Id;
                    }
                    else
                    {
                        this.GlobalContext.Log.Warning($"System User with DomainName {apiAccountHolder.PerformVerificationSystemUser.DomainName} Not Found.");
                    }
                }
            }
        }

        private void HandleCustomer(ApiAccountHolder apiAccountHolder)
        {
            this.GlobalContext.LogEntry($"Identification Number ({apiAccountHolder.IdentificationNumber})");

            IdentificationTypeCode identificationTypeCode = (IdentificationTypeCode)apiAccountHolder.IdentificationTypeCode.Value;
            switch (identificationTypeCode)
            {
                case IdentificationTypeCode.GovernmentId:
                case IdentificationTypeCode.Passport:
                case IdentificationTypeCode.DrivingLicense:
                    {
                        ContactDAL contacDal = new ContactDAL(this.GlobalContext);
                        apiAccountHolder.CustomerId = contacDal.GetByGovernmentId(apiAccountHolder.IdentificationNumber);
                        if (apiAccountHolder.CustomerId == null)
                        {
                            apiAccountHolder.CustomerId = new ApiContact()
                            {
                                Id = contacDal.Create(new ApiContact()
                                {
                                    FirstName = apiAccountHolder.FirstName,
                                    LastName = apiAccountHolder.LastName,
                                    GovernmentId = apiAccountHolder.IdentificationNumber

                                })
                            };
                        }
                        break;
                    }

                default:
                    {
                        AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                        apiAccountHolder.CustomerId = accountDal.GetByAccountNumber(apiAccountHolder.IdentificationNumber);
                        if (apiAccountHolder.CustomerId == null)
                        {
                            apiAccountHolder.CustomerId = new ApiAccount()
                            {
                                Id = accountDal.Create(new ApiAccount()
                                {
                                    Name = apiAccountHolder.FirstName,
                                    AccountNumber = apiAccountHolder.IdentificationNumber
                                })
                            };
                        }
                        break;
                    }
            }
        }

        private Guid? HandleAccountHolderKyc(ApiAccountHolder apiAccountHolder)
        {
            this.GlobalContext.LogEntry($"Identification Number: {apiAccountHolder.IdentificationNumber}");

            Guid? kycId;
            if (!this.IsKycExistByAccountHolder(apiAccountHolder.Id.Value, out kycId))
            {
                ApiKYC apiKYCToCreate = apiAccountHolder.KYC;
                apiKYCToCreate.AccountHolder = new ApiAccountHolder() { Id = apiAccountHolder.Id };
                apiKYCToCreate.DigitalFormVerification = apiAccountHolder.DigitalFormVerification;

                KycDAL kycDal = new KycDAL(this.GlobalContext);
                kycDal.Create(apiKYCToCreate);
            }
            return kycId;
        }

        private bool IsKycExistByAccountHolder(Guid accountHolderId, out Guid? kycId)
        {
            this.GlobalContext.LogEntry();
            kycId = null;
            KycDAL kycDAL = new KycDAL(this.GlobalContext);
            ApiKYC retrievedKYC = kycDAL.GetByAttribute("alt_accountholderid", accountHolderId, new string[] { "alt_kycid" }).FirstOrDefault();
            if (retrievedKYC != null)
            {
                kycId = retrievedKYC.Id;
            }
            return retrievedKYC != null;
        }

        private void HanldePopulationRegisterCustomerVerification(ApiAccountHolder apiAccountHolder, string joiningProcessNumber)
        {
            this.GlobalContext.LogEntry();

            if (apiAccountHolder.AccountHolderTypeCode != null
                && apiAccountHolder.AccountHolderTypeCode.Value == (int)AccountHolderTypeCode.Owner
                && apiAccountHolder.IdentificationTypeCode.Value != (int)IdentificationTypeCode.DrivingLicense)
            {

                ApiPopulationRegistryCustomerVerification populationRegistryVerificationToCreate = new ApiPopulationRegistryCustomerVerification()
                {
                    IdentityNumber = apiAccountHolder.IdentificationNumber,
                    BirthDate = apiAccountHolder.BirthDate,
                    IDIssuanceDate = apiAccountHolder.IDIssueDate,
                    ContactId = apiAccountHolder.CustomerId as ApiContact,
                    RelatedRecordId = apiAccountHolder,
                    Owner = apiAccountHolder.Owner,
                    JoiningProcessNumber = joiningProcessNumber,
                    CompanyCodeInt = (int)CompanyCode.StockExchangeMember
                };
                PopulationRegistryCustomerVerificationDAL populationRegistryCustomerVerificationDal = new PopulationRegistryCustomerVerificationDAL(this.GlobalContext);
                try
                {
                    populationRegistryCustomerVerificationDal.Create(populationRegistryVerificationToCreate);
                }
                catch (Exception ex)
                {
                    this.GlobalContext.Log.Warning($"Create Population Register Verification for Account Holder Id ({apiAccountHolder.Id} Failed). Error message: {ex.Message}");
                }
            }
        }

        internal ActionResult HandleCustomerOperationRequest(ApiContext<ApiCustomerOperationRequest> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ApiAccountHolder apiAccountHolder = new ApiAccountHolder { Id = apiContext.MergedTarget.RelatedRecordId.Id };
            ApiCustomerOperationRequest apiCustomerOperationRequest = apiContext.MergedTarget;

            CustomerOperationTemplateDAL customerOperationTemplateDal = new CustomerOperationTemplateDAL(this.GlobalContext);
            ApiCustomerOperationTemplate apiCustomerOperationTemplate = customerOperationTemplateDal.Get(apiCustomerOperationRequest.CustomerOperationTemplateId.Id.Value, null);
            switch (apiContext.MergedTarget.CustomerOperationTemplateCode)
            {
                case (int)CustomerOperationTemplateCode.SendJoiningBenefit:
                    {
                        actionResult = this.SendJoiningBenefit(apiAccountHolder, apiCustomerOperationTemplate.ApiConfigurationId);
                        break;
                    }
                case (int)CustomerOperationTemplateCode.OpenTradeOneUser:
                    {
                        actionResult = this.SendToOpenTradeOneUser(apiAccountHolder, apiCustomerOperationTemplate.ApiConfigurationId);
                        break;
                    }
                case (int)CustomerOperationTemplateCode.SendCustomerAgreement:
                    {
                        actionResult = this.SendCustomerAgreement(apiAccountHolder, apiCustomerOperationRequest, apiCustomerOperationTemplate);
                        break;
                    }
                case (int)CustomerOperationTemplateCode.CheckEligibilityBenefit:
                    {
                        actionResult = this.CheckClubMembershipEligibility(apiAccountHolder, apiCustomerOperationTemplate.ApiConfigurationId);
                        break;
                    }
                case (int)CustomerOperationTemplateCode.FundsTransferActionMotivation:
                case (int)CustomerOperationTemplateCode.CompletionDigitalJoiningProcess:
                case (int)CustomerOperationTemplateCode.MainAccountHoldersManualMailing:
                    {
                        actionResult = this.HandleMailingWithFeezbackLink(apiAccountHolder, apiCustomerOperationTemplate);
                        break;
                    }
                default:
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.NotImplementedLogicForCustomerOperationTemplate, new string[] { this.ApiConfiguration.Code?.ToString() });
                        break;
                    }
            }

            return actionResult;
        }

        private ActionResult HandleMailingWithFeezbackLink(ApiAccountHolder apiAccountHolder, ApiCustomerOperationTemplate apiCustomerOperationTemplate)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (apiCustomerOperationTemplate.ApiConfigurationId != null)
            {
                AccountHolderDAL accountHolderDAL = new AccountHolderDAL(this.GlobalContext);
                DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(this.GlobalContext);

                ApiConfiguration retrievedApiConfiguration = apiConfigurationDal.GetApiConfigurationById(apiCustomerOperationTemplate.ApiConfigurationId.Id.Value);
                ApiAccountHolder retrievedApiAccountHolder = accountHolderDAL.GetAccountHolderDetails(apiAccountHolder.Id.Value);

                ApiDigitalFormVerification relatedApiDigitalFormVerification =
                    digitalFormVerificationDAL.GetDigitalFormVerificationDetails(retrievedApiAccountHolder.DigitalFormVerification.Id.Value);

                retrievedApiAccountHolder.DigitalFormVerification = relatedApiDigitalFormVerification;
                ESBFeezbackLinkRequestDAL eSBFeezbackLinkRequestDAL = new ESBFeezbackLinkRequestDAL(this.GlobalContext, retrievedApiConfiguration);
                actionResult = eSBFeezbackLinkRequestDAL.ExecuteRequest(retrievedApiAccountHolder);

                string linkPrefix = string.Empty;
                if (retrievedApiConfiguration.TryGetSettingsItemValue("Constants", out Dictionary<string, string> settings))
                {
                    linkPrefix = settings["FeezbackLinkPrefix"];
                }

                this.HandleFeezbackLinkResponse(retrievedApiAccountHolder, actionResult, linkPrefix);

                string parserCustomEntryPoint = base.Serialize(new CustomEntityReference { Id = apiAccountHolder.Id.Value, LogicalName = ApiAccountHolder.EntityLogicalName });
                if (apiCustomerOperationTemplate.EmailTemplateId != null)
                {

                    EmailSettings emailSettings = new EmailSettings
                    {
                        EmailTemplateId = apiCustomerOperationTemplate.EmailTemplateId,
                        ParserCustomEntryPoint = parserCustomEntryPoint,
                        Regarding = relatedApiDigitalFormVerification,
                    };
                    this.SendEmail(retrievedApiAccountHolder, emailSettings);
                }
                if (apiCustomerOperationTemplate.SmsTemplateId != null)
                {
                    this.SendSmsWithFeezbackLink(retrievedApiAccountHolder, parserCustomEntryPoint, apiCustomerOperationTemplate.SmsTemplateId);
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }

            return actionResult;
        }

        private void HandleFeezbackLinkResponse(ApiAccountHolder retrievedApiAccountHolder, ActionResult actionResult, string linkPrefix)
        {
            this.GlobalContext.LogEntry();

            string feezbackLink = null;
            if (actionResult.IsSuccess)
            {
                var feezbackLinkResponse = base.DeserializeSpecial<ESBResponse<ESBFeezbackLinkResponse>>(actionResult.ReturnObject.ToString());
                if (feezbackLinkResponse.ErrorCode == (int)ESBResultStatusCode.Success)
                {
                    feezbackLink = linkPrefix + feezbackLinkResponse.ResponseData.DepositPageId;
                    actionResult.ReturnObject = feezbackLink;
                }
                else
                {
                    actionResult.SetToFailedActionResult(feezbackLinkResponse.ErrorMessage);
                }
            }
            ApiAccountHolder accountHolderToUpdate = new ApiAccountHolder()
            {
                Id = retrievedApiAccountHolder.Id,
                FeezbackLink = feezbackLink
            };
            AccountHolderDAL accountHolderDAL = new AccountHolderDAL(this.GlobalContext);
            accountHolderDAL.Update(accountHolderToUpdate);
        }

        private ActionResult CheckClubMembershipEligibility(ApiAccountHolder apiAccountHolder, ApiConfiguration apiConfiguration)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (apiConfiguration != null)
            {
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                var retrievedAccountHolder = accountHolderDal.Get(apiAccountHolder.Id.Value, new string[] { "alt_identificationnumber", "alt_digitalformverificationid" });

                ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(this.GlobalContext);
                ApiConfiguration retrievedApiConfiguration = apiConfigurationDal.GetApiConfigurationById(apiConfiguration.Id.Value);

                ESBClubMembershipEligibilityDAL eSBClubMembershipEligibilityDal = new ESBClubMembershipEligibilityDAL(this.GlobalContext, retrievedApiConfiguration);
                actionResult = eSBClubMembershipEligibilityDal.ExecuteRequest(retrievedAccountHolder);

                if (actionResult.IsSuccess)
                {
                    this.HandleClubMembershipEligibilityResponse(retrievedAccountHolder, actionResult);
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }
            return actionResult;
        }

        private void HandleClubMembershipEligibilityResponse(ApiAccountHolder apiAccountHolder, ActionResult actionResult)
        {
            var response = base.GetDeserializedContent<ESBClubMembershipEligibilityResponse>(actionResult.ReturnObject?.ToString());
            if (!string.IsNullOrWhiteSpace(response.VerificationResultCode)
                && int.TryParse(response.VerificationResultCode, out int result))
            {
                if (Enum.IsDefined(typeof(ClubMembershipEligibilityCode), result))
                {
                    AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                    accountHolderDal.Update(new ApiAccountHolder
                    {
                        Id = apiAccountHolder.Id,
                        ClubMembershipEligibilityCode = result
                    });
                    this.SetDigitalFormVerificationLoyaltyProgram(apiAccountHolder, response.VerificationResultCode);
                }
                actionResult.ReturnObject = null;
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { response.ToString() });
            }
        }

        private void SetDigitalFormVerificationLoyaltyProgram(ApiAccountHolder apiAccountHolder, string verificationResultCode)
        {
            this.GlobalContext.LogEntry();

            var mappingSettings = this.GlobalContext.CacheManager.GetGlobalParameter<Dictionary<string, int>>("ClubMembershipEligibilityMappingSettings");
            if (mappingSettings.ContainsKey(verificationResultCode))
            {
                int loyaltyProgramCode = mappingSettings[verificationResultCode];
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                digitalFormVerificationDal.Update(new ApiDigitalFormVerification
                {
                    Id = apiAccountHolder.DigitalFormVerification.Id,
                    LoyaltyProgramId = new ApiLoyaltyProgram
                    {
                        Code = loyaltyProgramCode
                    }
                });
            }
        }

        public ActionResult SendToOpenTradeOneUser(ApiAccountHolder apiAccountHolder, ApiConfiguration apiConfiguration)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (apiConfiguration != null)
            {
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                var retrievedAccountHolder = accountHolderDal.GetAccountHolderDetails(apiAccountHolder.Id.Value);

                ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(this.GlobalContext);
                ApiConfiguration retrievedApiConfiguration = apiConfigurationDal.GetApiConfigurationById(apiConfiguration.Id.Value);

                ESBTradeOneUserDAL esbTradeOneUserDal = new ESBTradeOneUserDAL(this.GlobalContext, retrievedApiConfiguration);
                actionResult = esbTradeOneUserDal.ExecuteRequest(retrievedAccountHolder);

                if (actionResult.IsSuccess)
                {
                    var response = base.GetDeserializedContent<ESBResponse<ESBTradeOneUserResponse>>(actionResult.ReturnObject?.ToString());
                    if (response.ResultStatusCode == null)
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { response.ErrorCode?.ToString() });
                    }
                    else
                    {
                        if (response.ResultStatusCode == ESBResultStatusCode.Success)
                        {
                            accountHolderDal.Update(new ApiAccountHolder
                            {
                                Id = apiAccountHolder.Id,
                                UserNameTrade = response.ResponseData?.UserName
                            });
                        }
                        else
                        {
                            actionResult.SetToFailedActionResult(response.ErrorMessage);
                        }
                    }
                    actionResult.ReturnObject = null;
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }
            return actionResult;
        }

        internal ActionResult SendJoiningBenefit(ApiAccountHolder apiAccountHolder, ApiConfiguration apiConfiguration)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            if (apiConfiguration != null)
            {
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                var retrievedAccountHolder = accountHolderDal.Get(apiAccountHolder.Id.Value, new string[] { "alt_name", "alt_mobilephone", "alt_email" });

                ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(this.GlobalContext);
                ApiConfiguration retrievedApiConfiguration = apiConfigurationDal.GetApiConfigurationById(apiConfiguration.Id.Value);

                ESBJoiningBenefitRequestDAL eSBTradeFreeCourseDal = new ESBJoiningBenefitRequestDAL(this.GlobalContext, retrievedApiConfiguration);
                actionResult = eSBTradeFreeCourseDal.ExecuteRequest(retrievedAccountHolder);

                if (actionResult.IsSuccess)
                {
                    var response = base.GetDeserializedContent<ESBResponse<object>>(actionResult.ReturnObject?.ToString());
                    if (response.ResultStatusCode == null)
                    {
                        actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { response.ErrorCode?.ToString() });
                    }
                    else if (response.ResultStatusCode != ESBResultStatusCode.Success)
                    {
                        actionResult.SetToFailedActionResult(response.ErrorMessage);
                    }
                    actionResult.ReturnObject = null;
                }
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.ApiConfigurationNotFound);
            }
            return actionResult;
        }

        internal ActionResult SendCustomerAgreement(ApiAccountHolder apiAccountHolder, ApiCustomerOperationRequest apiCustomerOperationRequest, ApiCustomerOperationTemplate apiCustomerOperationTemplate)
        {
            this.GlobalContext.LogEntry();

            var retrievedAccountHolder = this.GetAccountHolderDetails(apiAccountHolder.Id.Value);
            var pdfProductionTemplate = this.GetPdfProductionTemplate(retrievedAccountHolder, apiCustomerOperationRequest);

            ActionResult actionResult = this.GetPDF(retrievedAccountHolder, pdfProductionTemplate);
            if (actionResult.IsSuccess)
            {
                CommonDAL commonDal = new CommonDAL(this.GlobalContext, string.Empty);
                DocumentDetails documentDetails = new DocumentDetails
                {
                    FileBody = actionResult.ReturnObject.ToString(),
                    FileName = commonDal.GetParsedMessage(pdfProductionTemplate.FileName, apiAccountHolder),
                    MimeType = "application/pdf"
                };
                actionResult.ReturnObject = null;
                this.CreateCustomerAgreementDocument(retrievedAccountHolder, documentDetails);
                if (apiCustomerOperationTemplate.EmailTemplateId != null)
                {
                    actionResult = this.HandleMainlingOnSendCustomerAgreement(retrievedAccountHolder, documentDetails, apiCustomerOperationTemplate);
                }
            }
            return actionResult;
        }

        private ApiPDFProductionTemplate GetPdfProductionTemplate(ApiAccountHolder apiAccountHolder, ApiCustomerOperationRequest apiCustomerOperationRequest)
        {
            this.GlobalContext.LogEntry();
            int? pdfProductionTemplateCode;
            if (apiCustomerOperationRequest.PDFProductionTemplateCode != null)
            {
                pdfProductionTemplateCode = apiCustomerOperationRequest.PDFProductionTemplateCode;
            }
            else
            {
                pdfProductionTemplateCode = this.GetPdfProductionTemplateCode(apiAccountHolder);
                CustomerOperationRequestDAL customerOperationRequestDal = new CustomerOperationRequestDAL(this.GlobalContext);
                customerOperationRequestDal.Update(new ApiCustomerOperationRequest { Id = apiCustomerOperationRequest.Id, PDFProductionTemplateCode = pdfProductionTemplateCode });
            }
            PDFProductionTemplateDAL pDFProductionTemplateDal = new PDFProductionTemplateDAL(this.GlobalContext);
            return pDFProductionTemplateDal.GetActiveByAttribute("alt_codeint", pdfProductionTemplateCode.Value, null)
                .FirstOrDefault();
        }

        private ActionResult HandleMainlingOnSendCustomerAgreement(ApiAccountHolder apiAccountHolder, DocumentDetails documentDetails, ApiCustomerOperationTemplate apiCustomerOperationTemplate)
        {
            this.GlobalContext.LogEntry();
            string parserCustomEntryPoint = base.Serialize(new CustomEntityReference { Id = apiAccountHolder.Id.Value, LogicalName = ApiAccountHolder.EntityLogicalName });

            EmailSettings emailSettings = new EmailSettings
            {
                EmailTemplateId = apiCustomerOperationTemplate.EmailTemplateId,
                Attachments = new List<DocumentDetails> { documentDetails },
                ParserCustomEntryPoint = parserCustomEntryPoint,
                Regarding = apiAccountHolder.Portfolio,

            };
            string base64 = emailSettings.Attachments[0].FileBody;
            emailSettings.Attachments[0].FileBody = this.GetProtectedPdf(base64, apiAccountHolder.IdentificationNumber.GetLast(4));

            ActionResult actionResult = this.SendEmail(apiAccountHolder, emailSettings);
            new AccountHolderDAL(this.GlobalContext).Update(new ApiAccountHolder()
            {
                Id = apiAccountHolder.Id,
                SentCustomerAgreementBit = actionResult.IsSuccess
            });
            if (actionResult.IsSuccess)
            {
                this.SendSmsForCustomerAgreement(apiAccountHolder, parserCustomEntryPoint, apiCustomerOperationTemplate.SmsTemplateId);
            }

            return actionResult;
        }

        private void CreateCustomerAgreementDocument(ApiAccountHolder apiAccountHolder, DocumentDetails documentDetails)
        {
            this.GlobalContext.LogEntry();

            DocumentDAL documentDal = new DocumentDAL(this.GlobalContext);
            ApiDocument apiDocument = new ApiDocument
            {
                Name = documentDetails.FileName,
                Regarding = apiAccountHolder.Portfolio,
                CustomerID = new ApiCustomer(apiAccountHolder.CustomerId.LogicalName) { Id = apiAccountHolder.CustomerId.Id.Value },
                DocumentTypeCode = (int)DocumentTypeCode.AccountOpeningAgreement,
                ProductTypeCode = (int)DocumentProductTypeCode.Trade
            };

            Guid id = documentDal.Create(apiDocument);

            DocumentBL documentBl = new DocumentBL(this.GlobalContext);
            documentBl.PopulateFileFieldInDocument(id, documentDetails.FileBody, documentDetails.FileName, documentDetails.MimeType);
            documentDal.Update(new ApiDocument { Id = id, ArchiveTransferStatusCode = (int)TransferStatusCode.Send });
        }

        private ActionResult SendEmail(ApiAccountHolder apiAccountHolder, EmailSettings emailSettings)
        {
            this.GlobalContext.LogEntry();

            emailSettings.Related = new List<ApiActivityParty> { new ApiActivityParty { Id = apiAccountHolder.CustomerId.Id, LogicalName = apiAccountHolder.CustomerId.LogicalName } };
            emailSettings.Recipients = new List<ApiActivityParty> { new ApiActivityParty { AddressUsed = apiAccountHolder.Email } };

            EmailBL emailBL = new EmailBL(this.GlobalContext);
            return emailBL.CreateEmailByEmailSettings(emailSettings);
        }

        private void SendSmsForCustomerAgreement(ApiAccountHolder apiAccountHolder, string parserCustomEntryPoint, ApiSmsTemplate smsTemplate)
        {
            this.GlobalContext.LogEntry();

            this.SendSms(apiAccountHolder.Portfolio, apiAccountHolder, parserCustomEntryPoint, smsTemplate);
        }

        private void SendSmsWithFeezbackLink(ApiAccountHolder apiAccountHolder, string parserCustomEntryPoint, ApiSmsTemplate smsTemplate)
        {
            this.GlobalContext.LogEntry();

            this.SendSms(apiAccountHolder.DigitalFormVerification, apiAccountHolder, parserCustomEntryPoint, smsTemplate);
        }

        private void SendSms(ApiEntity regarding, ApiAccountHolder apiAccountHolder, string parserCustomEntryPoint, ApiSmsTemplate smsTemplate)
        {
            this.GlobalContext.LogEntry();

            if (smsTemplate != null)
            {
                ApiContact contact = null;
                if (apiAccountHolder.CustomerId.LogicalName == ApiContact.EntityLogicalName)
                {
                    contact = new ApiContact()
                    {
                        Id = apiAccountHolder.CustomerId.Id
                    };
                }

                SmsDAL smsDAL = new SmsDAL(this.GlobalContext);
                ApiSms smsToCreate = new ApiSms
                {
                    IsAutomatic = true,
                    Owner = apiAccountHolder.Owner,
                    StatusCode = (int)SmsStatusCode.Send,
                    RegardingObject = regarding,
                    MobilePhone = apiAccountHolder.MobilePhone,
                    SmsTemplate = smsTemplate,
                    ContactId = contact,
                    ParserCustomEntryPoint = parserCustomEntryPoint
                };
                smsDAL.Create(smsToCreate);
            }
            else
            {
                this.GlobalContext.Log.Warning($"Sms Template Code Not Defined");
            }
        }

        private ActionResult GetPDF(ApiAccountHolder accountHolder, ApiPDFProductionTemplate pdfProductionTemplate)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            string parsedPdfData = null;
            if (!string.IsNullOrWhiteSpace(pdfProductionTemplate.JsonData))
            {
                if (pdfProductionTemplate.UseValueParserBit.Value)
                {
                    CommonDAL commonDal = new CommonDAL(this.GlobalContext, string.Empty);
                    parsedPdfData = commonDal.GetParsedPDFMessage(pdfProductionTemplate.JsonData, accountHolder);
                    this.GlobalContext.Log.Info($"Parsed Pdf Data:{Environment.NewLine}{parsedPdfData}");
                }
                ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(this.GlobalContext);
                var apiConfiguration = apiConfigurationDal.GetApiConfigurationById(pdfProductionTemplate.ApiConfigurationId.Id);

                AnvilTradeCustomerAgreementDAL anvilTradeCustomerAgreementDal = new AnvilTradeCustomerAgreementDAL(this.GlobalContext, apiConfiguration, pdfProductionTemplate, parsedPdfData);
                actionResult = anvilTradeCustomerAgreementDal.ExecuteRequest(accountHolder);
            }
            else
            {
                actionResult.SetToFailedActionResult($"Json not Defined");
            }
            return actionResult;
        }

        private string GetProtectedPdf(string base64, string userPassword)
        {
            this.GlobalContext.LogEntry();

            var bytes = FileUtils.ProtectFile(base64, userPassword);
            return Convert.ToBase64String(bytes);
        }

        private ApiAccountHolder GetAccountHolderDetails(Guid id)
        {
            this.GlobalContext.LogEntry();
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            var accountHolder = accountHolderDal.Get(id, null);

            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
            accountHolder.DigitalFormVerification = digitalFormVerificationDal.Get(accountHolder.DigitalFormVerification.Id.Value, null);

            AuthorizationManagementDAL authorizationManagementDal = new AuthorizationManagementDAL(this.GlobalContext);
            accountHolder.DigitalFormVerification.AuthorizationManagements = authorizationManagementDal.GetByAttribute("alt_digitalformverificationid", accountHolder.DigitalFormVerification.Id.Value, null);

            return accountHolder;
        }

        private int GetPdfProductionTemplateCode(ApiAccountHolder apiEntity)
        {
            ApiDigitalFormVerification apiDigitalFormVerification = apiEntity.DigitalFormVerification;
            ApiAuthorizationManagement apiAuthorizationManagement = apiDigitalFormVerification.AuthorizationManagements?
                .OrderByDescending(a => a.CreatedOn)?.FirstOrDefault();

            OptionExerciseRequestApprovalExistsCode? optionExerciseRequestApprovalExistsCode = (OptionExerciseRequestApprovalExistsCode?)apiDigitalFormVerification?.OptionExerciseRequestApprovalExistsCode;
            ShortSaleRequestApprovaIExistsCode? shortSaleRequestApprovaIExistsCode = (ShortSaleRequestApprovaIExistsCode?)apiDigitalFormVerification?.ShortSaleRequestApprovaIExistsCode;
            CreditRequestExistsCode? creditRequestExistsCode = (CreditRequestExistsCode?)apiDigitalFormVerification?.CreditRequestExistsCode;
            OptinExerciseRequestApprovalCode? authorizationManagementOptionExerciseRequestApprovalExistsCode = apiAuthorizationManagement?.OptinExerciseRequestApprovalCode != null ?
                (OptinExerciseRequestApprovalCode?)apiAuthorizationManagement.OptinExerciseRequestApprovalCode : null;
            CreditRequestCode? creditRequestCode = apiAuthorizationManagement?.CreditRequestCode != null ?
                (CreditRequestCode?)apiAuthorizationManagement?.CreditRequestCode : null;
            bool? shortSaleRequestApprovalBit = apiAuthorizationManagement?.ShortSaleRequestApprovalBit;

            if (((optionExerciseRequestApprovalExistsCode == OptionExerciseRequestApprovalExistsCode.IncludOptions
                        || optionExerciseRequestApprovalExistsCode == OptionExerciseRequestApprovalExistsCode.OnlyBuySell)
                    && shortSaleRequestApprovaIExistsCode == ShortSaleRequestApprovaIExistsCode.Yes)
                || ((authorizationManagementOptionExerciseRequestApprovalExistsCode == OptinExerciseRequestApprovalCode.IncludeWriteOptions
                        || authorizationManagementOptionExerciseRequestApprovalExistsCode == OptinExerciseRequestApprovalCode.SellAndBayOnly)
                    && shortSaleRequestApprovalBit != null && shortSaleRequestApprovalBit.Value))
            {
                return 6;
            }
            else if (optionExerciseRequestApprovalExistsCode == OptionExerciseRequestApprovalExistsCode.IncludOptions
                || authorizationManagementOptionExerciseRequestApprovalExistsCode == OptinExerciseRequestApprovalCode.IncludeWriteOptions)
            {
                return 7;
            }
            else if (shortSaleRequestApprovaIExistsCode == ShortSaleRequestApprovaIExistsCode.Yes
                || shortSaleRequestApprovalBit != null && shortSaleRequestApprovalBit.Value)
            {
                return 4;
            }
            else if (optionExerciseRequestApprovalExistsCode == OptionExerciseRequestApprovalExistsCode.OnlyBuySell
                || authorizationManagementOptionExerciseRequestApprovalExistsCode == OptinExerciseRequestApprovalCode.SellAndBayOnly)
            {
                return 5;
            }
            else if (creditRequestExistsCode == CreditRequestExistsCode.Yes
                || creditRequestCode == CreditRequestCode.Yes)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
    }
}