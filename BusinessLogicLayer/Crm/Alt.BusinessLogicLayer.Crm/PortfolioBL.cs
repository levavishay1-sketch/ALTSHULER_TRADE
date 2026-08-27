using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class PortfolioBL : CrmBaseBL
    {
        private string[] fieldsFromDigitalFormVerification = new string[]
        {
            alt_DigitalFormVerification.Fields.alt_AutomaticLaunchedShenhavPortfolioBit,
            alt_DigitalFormVerification.Fields.alt_EncouragingDepositSystemUserId
        };

        public PortfolioBL(GlobalContext globalContext) : base(globalContext) { }

        public void LinkAccountHoldersToPortfolio(alt_Portfolio targetPortfolio)
        {
            this.GlobalContext.LogEntry();
            if (targetPortfolio.AttributeHasValue<DateTime?>(alt_Portfolio.Fields.alt_LatestOperationalSystemUpdateDate)
                && targetPortfolio.AttributeHasValue<string>(alt_Portfolio.Fields.alt_JoiningProcessNumber))
            {
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                var retrievedDigitalFormVerification = digitalFormVerificationDal.GetByAttribute(alt_DigitalFormVerification.Fields.alt_DigitalFormNumber, targetPortfolio.alt_JoiningProcessNumber, new string[] { alt_DigitalFormVerification.Fields.StateCode })?.FirstOrDefault();
                if (retrievedDigitalFormVerification != null)
                {
                    AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                    List<alt_AccountHolder> accountHolders = accountHolderDal.GetActiveByAttribute(alt_AccountHolder.Fields.alt_DigitalFormVerificationId, retrievedDigitalFormVerification.Id, new[] { alt_AccountHolder.Fields.alt_PortfolioId });
                    foreach (alt_AccountHolder accountHolder in accountHolders)
                    {
                        if (accountHolder.alt_PortfolioId == null)
                        {
                            alt_AccountHolder accountHolderToUpdate = new alt_AccountHolder
                            {
                                Id = accountHolder.Id,
                                StatusCode = new OptionSetValue((int)AccountHolderStatusCode.Active),
                                alt_PortfolioId = targetPortfolio.ToEntityReference()
                            };
                            accountHolderDal.Update(accountHolderToUpdate);
                        }
                        else
                        {
                            this.GlobalContext.Log.Warning($"AccountHolder with Id ({accountHolder.Id}) already linked to portfolio Id ({accountHolder.alt_PortfolioId.Id})");
                        }
                    }
                }
            }
        }

        public void SetFieldsByRelatedDigitalFormVerification(alt_Portfolio targetPortfolio)
        {
            this.GlobalContext.LogEntry();
            if (targetPortfolio.AttributeHasValue<string>(alt_Portfolio.Fields.alt_JoiningProcessNumber))
            {
                DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(this.GlobalContext);
                alt_DigitalFormVerification retrievedDigitalFormVerification = digitalFormVerificationDAL.GetFirstOrDefaultByAttribute(
                    alt_DigitalFormVerification.Fields.alt_DigitalFormNumber,
                    targetPortfolio.alt_JoiningProcessNumber,
                    fieldsFromDigitalFormVerification
                );

                if (retrievedDigitalFormVerification != null)
                {
                    targetPortfolio.alt_AutomaticLaunchedShenhavPortfolioBit = retrievedDigitalFormVerification.alt_AutomaticLaunchedShenhavPortfolioBit;
                    targetPortfolio.alt_EncouragingDepositSystemUserId = retrievedDigitalFormVerification.alt_EncouragingDepositSystemUserId;
                }
            }
        }

        public void SetConversionTimeInDaysByLeadCreatedOn(alt_Portfolio targetPortfolio)
        {
            GlobalContext.LogEntry();
            if (targetPortfolio.AttributeHasValue<string>(alt_Portfolio.Fields.alt_JoiningProcessNumber))
            {
                LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                Lead retrievedLead = leadDal.GetFirstOrDefaultByAttribute(Lead.Fields.alt_LeadIdentityNumber,
                    targetPortfolio.alt_JoiningProcessNumber, new string[] { Lead.Fields.CreatedOn });

                if (retrievedLead != null)
                {
                    targetPortfolio.alt_ConversionTimeInDaysInt = (DateTime.UtcNow - retrievedLead.CreatedOn.Value).Days;
                }
            }
        }

        public void HandleDocuments(alt_Portfolio targetPortfolio)
        {
            this.GlobalContext.LogEntry();

            if (targetPortfolio.AttributeHasValue<DateTime?>(alt_Portfolio.Fields.alt_OpenDate))
            {
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                var retrievedDigitalFormVerification = digitalFormVerificationDal.GetByAttribute(alt_DigitalFormVerification.Fields.alt_PortfolioId, targetPortfolio.Id, new string[] { alt_DigitalFormVerification.Fields.StateCode })?.FirstOrDefault();
                if (retrievedDigitalFormVerification != null)
                {
                    LinkArchiveDocumentsToPortfolio(targetPortfolio, retrievedDigitalFormVerification.ToEntityReference());
                }
            }
        }

        public void CompleteJoiningProcessOnCreateViaSSIS(alt_Portfolio targetPortfolio)
        {
            this.GlobalContext.LogEntry();
            if (targetPortfolio.AttributeHasValue<DateTime?>(alt_Portfolio.Fields.alt_LatestOperationalSystemUpdateDate)
                && targetPortfolio.AttributeHasValue<string>(alt_Portfolio.Fields.alt_JoiningProcessNumber))
            {
                string joiningProcessNumber = targetPortfolio.alt_JoiningProcessNumber;
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                var retrievedDigitalFormVerification = digitalFormVerificationDal.GetByAttribute(alt_DigitalFormVerification.Fields.alt_DigitalFormNumber, joiningProcessNumber, new string[] { alt_DigitalFormVerification.Fields.alt_PortfolioId })?.FirstOrDefault();

                if (retrievedDigitalFormVerification != null)
                {
                    if (retrievedDigitalFormVerification.alt_PortfolioId == null)
                    {
                        digitalFormVerificationDal.Update(new alt_DigitalFormVerification
                        {
                            Id = retrievedDigitalFormVerification.Id,
                            alt_PortfolioId = targetPortfolio.ToEntityReference()
                        });
                        this.LinkArchiveDocumentsToPortfolio(targetPortfolio, retrievedDigitalFormVerification.ToEntityReference());
                    }
                    else
                    {
                        this.GlobalContext.Log.Warning($"Digital Form Verification with Id ({retrievedDigitalFormVerification.Id}) already linked to portfolio Id ({retrievedDigitalFormVerification.alt_PortfolioId.Id})");
                    }
                }
                else
                {
                    OpportunityDAL opportunityDal = new OpportunityDAL(this.GlobalContext);
                    var retrievedOpportunity = opportunityDal.GetByAttribute(Opportunity.Fields.alt_OpportunityIdentityNumber, joiningProcessNumber, new string[] { Opportunity.Fields.StateCode })?.FirstOrDefault();
                    if (retrievedOpportunity != null)
                    {
                        if (retrievedOpportunity.StateCode == OpportunityState.Open)
                        {
                            opportunityDal.CloseOpportunityAsWon(retrievedOpportunity.ToEntityReference(), new OptionSetValue((int)OpportunityStatusCode.Winning));
                        }
                    }
                    else
                    {
                        LeadDAL leadDal = new LeadDAL(this.GlobalContext);
                        var retrievedLead = leadDal.GetByAttribute(Lead.Fields.alt_LeadIdentityNumber, joiningProcessNumber, new string[] { Lead.Fields.StateCode })?.FirstOrDefault();
                        if (retrievedLead != null && retrievedLead.StateCode == LeadState.Open)
                        {
                            var qualifyLeadResponse = leadDal.QualifyLead(retrievedLead.ToEntityReference(), new OptionSetValue((int)LeadStatusCode.Qualified), true);
                            var opportunity = qualifyLeadResponse.CreatedEntities
                                .Where(e => e.LogicalName == Opportunity.EntityLogicalName)?.ToArray()?.FirstOrDefault();
                            if (opportunity != null)
                            {
                                opportunityDal.CloseOpportunityAsWon(opportunity, new OptionSetValue((int)OpportunityStatusCode.Winning));
                            }
                        }
                    }
                }
            }
        }

        private void LinkArchiveDocumentsToPortfolio(alt_Portfolio targetPortfolio, EntityReference digitalFormVerificationEntityReference)
        {
            this.GlobalContext.LogEntry();

            DocumentBL documentBl = new DocumentBL(this.GlobalContext);
            documentBl.ReplaceDocumentsRegardingId(digitalFormVerificationEntityReference, targetPortfolio.ToEntityReference());
        }
    }
}
