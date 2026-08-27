using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.External.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm.External.DigitalJoiningForms
{
    public class TradeDigitalJoiningFormBL : DigitalJoiningFormBaseBL
    {
        const string tradeDigitalFormComplitedStatusParameterName = "TradeDigitalFormComplitedStatusCode";
        const string tradeDefaultCommissionClientTypeParameterName = "TradeDefaultCommissionClientTypeCode";
        const string tradeDefaultOwnerKey = "tradeDefaultOwner";

        public TradeDigitalJoiningFormBL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
            this.DigitalFormComplitedStatus = this.GlobalContext.CacheManager.GetGlobalParameter<string>(tradeDigitalFormComplitedStatusParameterName);
        }

        internal override void HandleDefaultOwner(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            base.HandleDefaultOwner<ApiTeam>(apiDigitalForm, tradeDefaultOwnerKey);
        }

        internal override void SetJoiningForm(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            this.JoiningForm = apiDigitalForm.JoiningForm;
        }

        internal override ActionResult ValidateJoiningForm()
        {
            ApiDigitalFormVerification joiningForm = this.JoiningForm as ApiDigitalFormVerification;
            return this.Validate(joiningForm);
        }

        internal override ActionResult ConstractData(ApiDigitalForm apiDigitalForm, string joiningProcessNumber = null)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            var digitalFormToUpdate = new ApiDigitalForm() { Id = apiDigitalForm.Id };
            DataReceptionStatusCode dataReceptionStatus = DataReceptionStatusCode.Success;
            try
            {
                this.HandleDataUnderConstraction(apiDigitalForm, joiningProcessNumber);
                digitalFormToUpdate.Customers = this.GetDigitalFormCustomers(apiDigitalForm.JoiningForm.AccountHolders);
            }
            catch (Exception ex)
            {
                dataReceptionStatus = DataReceptionStatusCode.Failed;
                this.GlobalContext.Log.Critical(ex.ToString());
                actionResult.SetToFailedActionResult(ex.Message);
            }
            finally
            {
                DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
                digitalFormToUpdate.DataReceptionStatusCode = (int)dataReceptionStatus;
                digitalFormToUpdate.DigitalFormDetails = apiDigitalForm.ToString();
                digitalFormDal.Update(digitalFormToUpdate);
                if (dataReceptionStatus == DataReceptionStatusCode.Success)
                {
                    this.CheckRelatedOpportunity(apiDigitalForm, apiDigitalForm.DigitalFormIdentityNumber);
                }
            }
            return actionResult;
        }

        private void CheckRelatedOpportunity(ApiDigitalForm apiDigitalForm, string joiningProcessNumber)
        {
            this.GlobalContext.LogEntry();
            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
            ApiDigitalFormVerification retrievedDigitalFormVerification = digitalFormVerificationDal.Get(apiDigitalForm.JoiningForm.Id.Value, new string[] { "alt_opportunityid" });
            if (retrievedDigitalFormVerification.OpportunityId == null)
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                ApiLead retrievedLead = leadDal.GetByAttribute("alt_leadidentitynumber", joiningProcessNumber, new string[] { "qualifyingopportunityid" }).FirstOrDefault();
                if (retrievedLead != null && retrievedLead.QualifyingOpportunityId != null)
                {
                    retrievedDigitalFormVerification.OpportunityId = retrievedLead.QualifyingOpportunityId;
                    digitalFormVerificationDal.Update(retrievedDigitalFormVerification);
                }
                else
                {
                    this.GlobalContext.Log.Warning($"Digital Form Verification Id {retrievedDigitalFormVerification.Id} without related Opportunity");
                }
            }
        }

        internal override void HandleRegardingObject(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();

            var retrievedDigitalForm = this.GetDigitalFormByIdOrIdentityNumber(apiDigitalForm);
            ApiLead apiLead = null;
            if (retrievedDigitalForm.RegardingObject == null)
            {
                ApiDigitalForm apiDigitalFormToUpdate = new ApiDigitalForm { Id = apiDigitalForm.Id };
                if (retrievedDigitalForm.DigitalFormIdentityNumber != null)
                {
                    apiLead = this.GetRelatedLeadDetails(apiDigitalForm.DigitalFormIdentityNumber);
                    if (apiLead != null)
                    {
                        apiDigitalFormToUpdate.RegardingObject = apiLead;
                        this.HandleRelatedLeadDetails(apiDigitalForm, apiLead);
                    }
                    else
                    {
                        string leadIdentityNumber = this.CreateRegardingObject(apiDigitalForm);
                        apiDigitalFormToUpdate.RegardingObject = new ApiLead() { LeadIdentityNumber = leadIdentityNumber };
                    }
                }
                else
                {
                    string leadIdentityNumber = this.CreateRegardingObject(apiDigitalForm);
                    apiDigitalFormToUpdate.DigitalFormIdentityNumber = leadIdentityNumber;
                    apiDigitalFormToUpdate.RegardingObject = new ApiLead() { LeadIdentityNumber = leadIdentityNumber };
                }

                apiDigitalFormToUpdate.Subject = this.GenerateSubject(apiDigitalForm);

                DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
                digitalFormDal.Update(apiDigitalFormToUpdate);
            }
            else
            {
                if (apiLead == null)
                {
                    apiLead = this.GetRelatedLeadDetails(apiDigitalForm.DigitalFormIdentityNumber);
                }
                this.HandleRelatedLeadDetails(apiDigitalForm, apiLead);
            }
        }

        private string GenerateSubject(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            string mobilePhone = apiDigitalForm.JoiningForm.PortfolioOwners
                .Where(p => p.MainAccountHolder != null && p.MainAccountHolder.Value)?
                .FirstOrDefault()?.MobilePhone;
            return $"{apiDigitalForm.DigitalFormIdentityNumber} - {mobilePhone}";
        }

        private void HandleRelatedLeadDetails(ApiDigitalForm apiDigitalForm, ApiLead apiLead)
        {
            this.GlobalContext.LogEntry();
            if (apiLead.StateCode == 0)
            {
                if (string.IsNullOrWhiteSpace(apiLead.IdentityNumber)
                  || string.IsNullOrWhiteSpace(apiLead.FirstName)
                  || string.IsNullOrWhiteSpace(apiLead.LastName)
                  || string.IsNullOrWhiteSpace(apiLead.DigitalFormLink))
                {
                    var primaryOwner = apiDigitalForm.JoiningForm.PortfolioOwners.Where(p => p.MainAccountHolder != null
                                        && p.MainAccountHolder.Value)?.FirstOrDefault();

                    ApiLead leadToUpdate = new ApiLead { Id = apiLead.Id };
                    if (string.IsNullOrWhiteSpace(apiLead.IdentityNumber))
                    {
                        leadToUpdate.IdentityNumber = primaryOwner.IdentificationNumber;
                    }
                    if (string.IsNullOrWhiteSpace(apiLead.FirstName))
                    {
                        leadToUpdate.FirstName = primaryOwner.FirstName;
                    }
                    if (string.IsNullOrWhiteSpace(apiLead.LastName))
                    {
                        leadToUpdate.LastName = primaryOwner.LastName;
                    }
                    if (string.IsNullOrWhiteSpace(apiLead.DigitalFormLink))
                    {
                        leadToUpdate.DigitalFormLink = apiDigitalForm.DigitalFormLink;
                    }

                    LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                    leadDal.Update(leadToUpdate);
                }
            }
        }

        private ApiLead GetRelatedLeadDetails(string digitalFormIdentityNumber)
        {
            this.GlobalContext.LogEntry();
            ApiLead apiLead = null;
            if (!string.IsNullOrWhiteSpace(digitalFormIdentityNumber))
            {
                string[] attributesToRetrieve = { "leadid", "statecode", "firstname", "lastname", "alt_identitynumber", "alt_digitalformlink" };

                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                apiLead = leadDal.GetByAttribute("alt_leadidentitynumber", digitalFormIdentityNumber, attributesToRetrieve).FirstOrDefault();
            }
            return apiLead;
        }

        private ApiDigitalForm GetDigitalFormByIdOrIdentityNumber(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();

            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            string[] attributesToRetrieve = { "regardingobjectid", "alt_digitalformidentitynumber" };
            return apiDigitalForm.Id != null ?
                digitalFormDal.Get(apiDigitalForm.Id.Value, attributesToRetrieve)
                : digitalFormDal.GetByAttribute("alt_digitalformidentitynumber", apiDigitalForm.DigitalFormIdentityNumber, attributesToRetrieve).FirstOrDefault();
        }

        private string CreateRegardingObject(ApiDigitalForm apiDigitalForm)
        {
            this.GlobalContext.LogEntry();
            var primaryOwner = apiDigitalForm.JoiningForm?.PortfolioOwners.Where(p => p.MainAccountHolder != null
                && p.MainAccountHolder.Value)?.FirstOrDefault();

            LeadBL leadBL = new LeadBL(this.GlobalContext);
            ActionResult actionResult = leadBL.HandleCreateLead(new ApiLead
            {
                LeadSourceCode = (int)LeadSourceCode.DigitalForm,
                FirstName = primaryOwner?.FirstName,
                LastName = primaryOwner?.LastName,
                IdentityNumber = primaryOwner?.IdentificationNumber,
                MobilePhone = primaryOwner?.MobilePhone,
                EmailAddress1 = primaryOwner?.Email,
                Owner = apiDigitalForm.Owner,
                DigitalFormLink = apiDigitalForm.DigitalFormLink,
               // ApiConfigurationCode = apiDigitalForm.ApiConfigurationCode
            });
            ApiLead apiLead = (ApiLead)actionResult.ReturnObject;
            return apiLead.IdentityNumber ?? apiLead.Id.ToString();
        }

        private void HandleDataUnderConstraction(ApiDigitalForm apiDigitalForm, string joiningProcessNumber = null)
        {
            this.GlobalContext.LogEntry();
            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
            base.HandleDefaultOwner<ApiTeam>(apiDigitalForm.JoiningForm, tradeDefaultOwnerKey);

            if (apiDigitalForm.JoiningForm.Id == null)
            {
                ApiDigitalFormVerification retrievedDigitalFormVerification = digitalFormVerificationDAL.GetActiveByAttribute("alt_digitalformid", apiDigitalForm.Id.Value, new[] { "alt_digitalformverificationid" }).FirstOrDefault();
                if (retrievedDigitalFormVerification != null)
                {
                    apiDigitalForm.JoiningForm.Id = retrievedDigitalFormVerification.Id;
                }
                else if (apiDigitalForm.JoiningForm.DigitalForm == null
                    || apiDigitalForm.JoiningForm.DigitalForm.Id == null
                    || retrievedDigitalFormVerification == null)
                {
                    apiDigitalForm.JoiningForm.DigitalForm = new ApiDigitalForm() { Id = apiDigitalForm.Id };
                    apiDigitalForm.JoiningForm.Name = this.GenerateDigitalFormVerificationName(apiDigitalForm.JoiningForm);
                    apiDigitalForm.JoiningForm.CommissionClientType = new ApiCommissionClientType() { Code = this.GlobalContext.CacheManager.GetGlobalParameter<string>(tradeDefaultCommissionClientTypeParameterName) };
                    apiDigitalForm.JoiningForm.Id = digitalFormVerificationDAL.Create(apiDigitalForm.JoiningForm);
                }
            }
            apiDigitalForm.JoiningForm.AccountHolders
                .Where(h => h.Id == null || h.DigitalFormVerification == null || h.DigitalFormVerification.Id == null)
                .Select(h => { h.DigitalFormVerification = new ApiDigitalFormVerification() { Id = apiDigitalForm.JoiningForm.Id }; return h; })
                .ToList();
            AccountHolderBL accountHolderBl = new AccountHolderBL(this.GlobalContext);
            accountHolderBl.CreateAccountHolders(apiDigitalForm.JoiningForm.AccountHolders, apiDigitalForm.DigitalFormIdentityNumber);
        }

        public ActionResult Validate(ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();
            try
            {
                if (apiDigitalFormVerification == null)
                {
                    throw new CustomException(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.EmptyDigitalFormVerification), CustomErrorCodes.EmptyDigitalFormVerification);
                }

                var primaryOwners = apiDigitalFormVerification.PortfolioOwners.Where(p => p.MainAccountHolder != null && p.MainAccountHolder.Value)?.ToList();
                if (primaryOwners == null || primaryOwners.Count != 1)
                {
                    throw new CustomException(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.MainAccountHolderError), CustomErrorCodes.MainAccountHolderError);
                }
                if ((BeneficiaryDeclarationCode)primaryOwners.First().BeneficiaryDeclarationCode.Value == BeneficiaryDeclarationCode.NoExeptSpouse
                     && (apiDigitalFormVerification.PortfolioBeneficiaries == null
                        || apiDigitalFormVerification.PortfolioBeneficiaries.Count == 0))
                {
                    throw new CustomException(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.BeneficiarySigningDeclarationError), CustomErrorCodes.BeneficiarySigningDeclarationError);
                }
                foreach (var accountHolder in apiDigitalFormVerification.AccountHolders)
                {
                    if (this.IsDuplicatedAccountHolder(apiDigitalFormVerification.AccountHolders, accountHolder.IdentificationNumber))
                    {
                        throw new CustomException(string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.DuplicatePortfolioOwnerError), accountHolder.IdentificationNumber), CustomErrorCodes.DuplicatePortfolioOwnerError);

                    }
                    if (accountHolder.SpouseAccountHolder != null
                         && !this.IsAccountHolderExist(apiDigitalFormVerification.PortfolioOwners, accountHolder.SpouseAccountHolder.IdentificationNumber))
                    {
                        throw new CustomException(string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.SpousePortfolioOwnerError), accountHolder.SpouseAccountHolder.IdentificationNumber), CustomErrorCodes.SpousePortfolioOwnerError);
                    }
                    if (accountHolder.BeneficiarySpouseAccountHolder != null
                        && !this.IsAccountHolderExist(apiDigitalFormVerification.PortfolioBeneficiaries, accountHolder.BeneficiarySpouseAccountHolder.IdentificationNumber))
                    {
                        throw new CustomException(string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.BeneficiarySpousePortfolioOwnerError), accountHolder.BeneficiarySpouseAccountHolder.IdentificationNumber), CustomErrorCodes.BeneficiarySpousePortfolioOwnerError);
                    }
                }
            }
            catch (CustomException ex)
            {
                actionResult.SetToFailedActionResult(ex.HResult, null, ex.Message);
            }
            return actionResult;
        }

        private bool IsDuplicatedAccountHolder(List<ApiAccountHolder> apiAccountHolders, string identificationNumber)
        {
            this.GlobalContext.LogEntry();

            var accountHolders = apiAccountHolders.Where(a => a.IdentificationNumber == identificationNumber).ToList();
            return accountHolders.Count > 1;
        }

        private bool IsAccountHolderExist(List<ApiAccountHolder> apiAccountHolders, string identificationNumber)
        {
            this.GlobalContext.LogEntry();

            var accountHolders = apiAccountHolders.Where(a => a.IdentificationNumber == identificationNumber).ToList();
            return accountHolders != null && accountHolders.Count > 0;
        }

        private string GenerateDigitalFormVerificationName(ApiDigitalFormVerification apiDigitalFormVerification)
        {
            this.GlobalContext.LogEntry();
            DigitalFormDAL digitalFormDal = new DigitalFormDAL(this.GlobalContext);
            ApiDigitalForm retrievedDigitalForm = digitalFormDal.Get(apiDigitalFormVerification.DigitalForm.Id.Value, new[] { "alt_digitalformidentitynumber" });
            List<string> nameParts = new List<string>() { retrievedDigitalForm.DigitalFormIdentityNumber };
            nameParts.AddRange(apiDigitalFormVerification.AccountHolders
                .Where(a => a.AccountHolderTypeCode == (int)AccountHolderTypeCode.Owner)
                .Select(a => $"{a.FirstName} {a.LastName}").ToList());
            return string.Join(" - ", nameParts);
        }

        private List<ApiActivityParty> GetDigitalFormCustomers(List<ApiAccountHolder> accountHolders)
        {
            this.GlobalContext.LogEntry();
            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
            foreach (var accountHolder in accountHolders)
            {
                if (accountHolder.CustomerId == null)
                {
                    accountHolder.CustomerId = accountHolderDal.Get(accountHolder.Id.Value, new string[] { "alt_customerid" }).CustomerId;
                }
            }
            return accountHolders.Select(a => new ApiActivityParty(a.CustomerId.LogicalName) { Id = a.CustomerId.Id }).ToList();
        }
    }
}
