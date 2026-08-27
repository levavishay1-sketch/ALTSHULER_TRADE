using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class PopulationRegistryCustomerVerificationBL : CrmBaseBL
    {
        static string configurationsParameterName = "PopulationRegistryVerificationConfigurations";
        static string firstNameKey = "FirstName";
        static string lastNameKey = "LastName";

        public PopulationRegistryCustomerVerificationBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void Validate(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            if (!targetPopulationRegistryCustomerVerification.AttributeHasValue<string>(alt_PopulationRegistryCustomerVerification.Fields.alt_IdentityNumber))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "מספר זהות"));
            }
            if (!targetPopulationRegistryCustomerVerification.AttributeHasValue<DateTime?>(alt_PopulationRegistryCustomerVerification.Fields.alt_BirthDate))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "תאריך לידה"));
            }
            if (!targetPopulationRegistryCustomerVerification.AttributeHasValue<int?>(alt_PopulationRegistryCustomerVerification.Fields.alt_CompanyCodeInt))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "קוד חברה"));
            }
        }

        public void SetName(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            targetPopulationRegistryCustomerVerification.alt_Name = targetPopulationRegistryCustomerVerification.alt_IdentityNumber;
        }

        public void SetCompareDataBit(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetPopulationRegistryCustomerVerification.alt_RelatedRecordId != null)
            {
                Configuration configuration = this.GetConfigurationByCompanyCode(targetPopulationRegistryCustomerVerification.alt_CompanyCodeInt.Value);
                string relatedEntityLogicalName = targetPopulationRegistryCustomerVerification.alt_RelatedRecordId.LogicalName;
                var definitionsByEntity = configuration?.DefinitionsByEntity?
                    .Where(d => d.LogicalName == relatedEntityLogicalName).FirstOrDefault();
                if (definitionsByEntity?.AttributesToCompare != null)
                {
                    targetPopulationRegistryCustomerVerification.alt_CompareDataBit = true;
                }
            }
        }

        public void HandlePopulationRegisterCustomerVerificationAsyncCreate(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            this.AppendToRelatedRecord(targetPopulationRegistryCustomerVerification);
        }

        public void HandlePopulationRegisterCustomerVerificationAsyncUpdate(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification, alt_PopulationRegistryCustomerVerification prePopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();

            var mergedPopulationRegisterCustomerVerification = targetPopulationRegistryCustomerVerification.Merge(prePopulationRegistryCustomerVerification);

            this.HandlePopulationRegistryValidationResponse(targetPopulationRegistryCustomerVerification, mergedPopulationRegisterCustomerVerification);
            this.HanleContactIdDetailsUpdate(targetPopulationRegistryCustomerVerification, mergedPopulationRegisterCustomerVerification);
        }

        public void SetContactByIdentityNumber(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetPopulationRegistryCustomerVerification.AttributeHasValue<string>(alt_PopulationRegistryCustomerVerification.Fields.alt_IdentityNumber)
                && targetPopulationRegistryCustomerVerification.alt_ContactId == null)
            {
                ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                Contact retrievedContact = contactDal.GetByGovernmentId(targetPopulationRegistryCustomerVerification.alt_IdentityNumber);
                if (retrievedContact != null)
                {
                    targetPopulationRegistryCustomerVerification.alt_ContactId = retrievedContact.ToEntityReference();
                }
                else
                {
                    var configuration = this.GetConfigurationByCompanyCode(targetPopulationRegistryCustomerVerification.alt_CompanyCodeInt);
                    if (configuration != null
                        && configuration.CreateContactIfNotExist.HasValue
                        && configuration.CreateContactIfNotExist.Value)
                    {
                        Guid contactId = contactDal.Create(new Contact() { GovernmentId = targetPopulationRegistryCustomerVerification.alt_IdentityNumber });
                        targetPopulationRegistryCustomerVerification.alt_ContactId = new EntityReference(Contact.EntityLogicalName, contactId);
                    }
                }
            }
        }

        public void AppendToRelatedRecord(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();

            if (targetPopulationRegistryCustomerVerification.AttributeHasValue<EntityReference>(alt_PopulationRegistryCustomerVerification.Fields.alt_RelatedRecordId))
            {
                switch (targetPopulationRegistryCustomerVerification.alt_RelatedRecordId.LogicalName)
                {
                    case alt_AccountHolder.EntityLogicalName:
                        {
                            AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                            accountHolderDal.Update(new alt_AccountHolder()
                            {
                                Id = targetPopulationRegistryCustomerVerification.alt_RelatedRecordId.Id,
                                alt_PopulationRegisterCustomerVerificationId = targetPopulationRegistryCustomerVerification.ToEntityReference()
                            });
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        public void HandleTransferStatusCode(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetPopulationRegistryCustomerVerification.AttributeHasValue<OptionSetValue>(alt_PopulationRegistryCustomerVerification.Fields.alt_TransferStatusCode)
                && targetPopulationRegistryCustomerVerification.alt_TransferStatusCode.Value == (int)TransferStatusCode.Send)
            {
                targetPopulationRegistryCustomerVerification.alt_TransferStatusCode = new OptionSetValue((int)TransferStatusCode.Sending);
                targetPopulationRegistryCustomerVerification.alt_ErrorMessageDetails = null;
            }
        }

        private void HandlePopulationRegistryValidationResponse(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification, alt_PopulationRegistryCustomerVerification mergedPopulationRegisterCustomerVerification)
        {
            this.GlobalContext.LogEntry();

            if (targetPopulationRegistryCustomerVerification.AttributeHasValue<OptionSetValue>(alt_PopulationRegistryCustomerVerification.Fields.alt_TransferStatusCode)
                && (targetPopulationRegistryCustomerVerification.alt_TransferStatusCode.Value == (int)TransferStatusCode.Sent
                    || targetPopulationRegistryCustomerVerification.alt_TransferStatusCode.Value == (int)TransferStatusCode.Failed))
            {
                switch (mergedPopulationRegisterCustomerVerification.alt_RelatedRecordId?.LogicalName)
                {
                    case alt_AccountHolder.EntityLogicalName:
                        {
                            this.HandleManagerVerificationRequired(mergedPopulationRegisterCustomerVerification);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void HanleContactIdDetailsUpdate(alt_PopulationRegistryCustomerVerification targetPopulationRegistryCustomerVerification, alt_PopulationRegistryCustomerVerification mergedPopulationRegisterCustomerVerification)
        {
            this.GlobalContext.LogEntry();
            if (targetPopulationRegistryCustomerVerification.AttributeHasValue<OptionSetValue>(alt_PopulationRegistryCustomerVerification.Fields.alt_TransferStatusCode)
                && targetPopulationRegistryCustomerVerification.alt_TransferStatusCode.Value == (int)TransferStatusCode.Sent
                && mergedPopulationRegisterCustomerVerification.alt_ContactId != null
                && !string.IsNullOrWhiteSpace(mergedPopulationRegisterCustomerVerification.alt_ResponseDetails))
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(mergedPopulationRegisterCustomerVerification.alt_ResponseDetails);
                if (responseData.ContainsKey(firstNameKey) || responseData.ContainsKey(lastNameKey))
                {
                    var configuration = this.GetConfigurationByCompanyCode(mergedPopulationRegisterCustomerVerification.alt_CompanyCodeInt);
                    if (configuration != null
                        && configuration.UpdateContactIfVerified.HasValue
                        && configuration.UpdateContactIfVerified.Value)
                    {
                        ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                        Contact retrievedContact = contactDal.Get(mergedPopulationRegisterCustomerVerification.alt_ContactId.Id, new string[]
                            {
                                Contact.Fields.FirstName,
                                Contact.Fields.LastName
                            });
                        if (string.IsNullOrWhiteSpace(retrievedContact.FirstName)
                            || string.IsNullOrWhiteSpace(retrievedContact.LastName))
                        {
                            Contact contactToUpdate = new Contact();
                            if (string.IsNullOrWhiteSpace(retrievedContact.FirstName)
                                && responseData.ContainsKey(firstNameKey)
                                && !string.IsNullOrWhiteSpace(responseData[firstNameKey]?.ToString()))
                            {
                                contactToUpdate.Attributes.Add(Contact.Fields.FirstName, responseData[firstNameKey].ToString());
                            }
                            if (string.IsNullOrWhiteSpace(retrievedContact.LastName)
                                && responseData.ContainsKey(lastNameKey)
                                && !string.IsNullOrWhiteSpace(responseData[lastNameKey]?.ToString()))
                            {
                                contactToUpdate.Attributes.Add(Contact.Fields.LastName, responseData[lastNameKey].ToString());
                            }
                            if (contactToUpdate.Attributes.Count > 0)
                            {
                                contactToUpdate.Id = mergedPopulationRegisterCustomerVerification.alt_ContactId.Id;
                                contactDal.Update(contactToUpdate);
                            }
                        }
                    }
                }
            }
        }

        private void HandleManagerVerificationRequired(alt_PopulationRegistryCustomerVerification mergedPopulationRegisterCustomerVerification)
        {
            this.GlobalContext.LogEntry();

            if (mergedPopulationRegisterCustomerVerification.alt_VerificationResultCode == null
                      || mergedPopulationRegisterCustomerVerification.alt_VerificationResultCode.Value == (int)PopulateReqisterVerificationCode.NotVerified
                      || mergedPopulationRegisterCustomerVerification.alt_IDIssuanceDateVerificationResultCode == null
                      || mergedPopulationRegisterCustomerVerification.alt_IDIssuanceDateVerificationResultCode.Value == (int)PopulateReqisterVerificationCode.NotVerified
                      || (mergedPopulationRegisterCustomerVerification.alt_CompareDataBit.Value
                          && (mergedPopulationRegisterCustomerVerification.alt_DataComparisonStatusCode == null
                          || mergedPopulationRegisterCustomerVerification.alt_DataComparisonStatusCode.Value == (int)DataComparisonStatusCode.NotMatch)))
            {
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                var retrievedAccountHolder = accountHolderDal.Get(mergedPopulationRegisterCustomerVerification.alt_RelatedRecordId.Id, new string[]
                                            {
                                                alt_AccountHolder.Fields.alt_DigitalFormVerificationId,
                                                alt_AccountHolder.Fields.StatusCode
                                            });
                if (retrievedAccountHolder.alt_DigitalFormVerificationId != null
                    && retrievedAccountHolder.StatusCode.Value == (int)AccountHolderStatusCode.InProcessing)
                {
                    DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                    var retrievedDigitalFromVerification = digitalFormVerificationDal.Get(retrievedAccountHolder.alt_DigitalFormVerificationId.Id, new string[] { alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode });
                    if (retrievedDigitalFromVerification.alt_ManagerVerificationRequiredCode == null
                        || retrievedDigitalFromVerification.alt_ManagerVerificationRequiredCode.Value != (int)ManagerVerificationRequiredCode.Yes)
                    {
                        digitalFormVerificationDal.Update(new alt_DigitalFormVerification()
                        {
                            Id = retrievedAccountHolder.alt_DigitalFormVerificationId.Id,
                            alt_ManagerVerificationRequiredCode = new OptionSetValue((int)ManagerVerificationRequiredCode.Yes)
                        });
                    }
                }
            }
        }

        private Configuration GetConfigurationByCompanyCode(int? companyCode)
        {
            this.GlobalContext.LogEntry();
            Configuration configuration = null;
            if (companyCode != null)
            {
                string globalParameter = this.GlobalContext.CacheManager.GetGlobalParameter<string>(configurationsParameterName);
                var populationRegistryVerificationSettings = JsonSerializer.Deserialize<PopulationRegistryVerificationSettings>(globalParameter);
                configuration = populationRegistryVerificationSettings?.configurations?
                    .Where(c => c.CompanyCode == companyCode).FirstOrDefault();
            }
            return configuration;
        }
    }
}
