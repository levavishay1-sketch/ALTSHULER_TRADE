using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class AccountHolderDAL : CrmExternalBaseDAL<ApiAccountHolder>
    {
        string streetEntityAlias = "street";
        string countryEntityAlias = "country";
        string birthCountryEntityAlias = "birthCountry";
        string portfolioEntityAlias = "portfolio";
        string digitalFormVerificationEntityAlias = "digitalFormVerification";
        string commissionClientTypeEntityAlias = "commissionClientType";
        string identificationIssuingCountryEntityAlias = "identificationIssuingCountry";
        string secIdentificationIssuingCountryEntityAlias = "secIdentificationIssuingCountry";
        string cityEntityAlias = "city";
        string primaryAttributeName = "alt_name";
        string countryCodeAttributeName = "alt_code";

        string[] attributesToRetrieve = new string[]
            {
                "alt_email",
                "alt_firstnameeng",
                "alt_lastnameeng",
                "alt_mobilephone",
                "alt_identificationnumber",
                "alt_portfolioid",
                "alt_usercharacteristiccode",
                "alt_firstname",
                "alt_lastname",
                "alt_group",
                "alt_name",
                "alt_digitalformverificationid",
                "alt_customerid"
            };

        public AccountHolderDAL(GlobalContext globalContext) : base(globalContext, ApiAccountHolder.EntityLogicalName) { }

        public List<ApiAccountHolder> GetActiveAccountHoldersByDigitalFormVerification(Guid id)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiAccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("alt_digitalformverificationid", ConditionOperator.Equal, id),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                },
                NoLock = true
            };

            var cityLinkEntity = query.AddLink(ApiCity.EntityLogicalName, "alt_cityid", "alt_cityid", JoinOperator.LeftOuter);
            cityLinkEntity.EntityAlias = cityEntityAlias;
            cityLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_code");

            var countryLinkEntity = query.AddLink(ApiCountry.EntityLogicalName, "alt_countryid", "alt_countryid", JoinOperator.LeftOuter);
            countryLinkEntity.EntityAlias = countryEntityAlias;
            countryLinkEntity.Columns.AddColumns(primaryAttributeName, countryCodeAttributeName);

            var birthCountryLinkEntity = query.AddLink(ApiCountry.EntityLogicalName, "alt_birthcountryid", "alt_countryid", JoinOperator.LeftOuter);
            birthCountryLinkEntity.EntityAlias = birthCountryEntityAlias;
            birthCountryLinkEntity.Columns.AddColumns(primaryAttributeName, countryCodeAttributeName);

            var identificationIssuingCountryLinkEntity = query.AddLink(ApiCountry.EntityLogicalName, "alt_identificationissuingcountryid", "alt_countryid", JoinOperator.LeftOuter);
            identificationIssuingCountryLinkEntity.EntityAlias = identificationIssuingCountryEntityAlias;
            identificationIssuingCountryLinkEntity.Columns.AddColumns(primaryAttributeName, countryCodeAttributeName);

            var streetLinkEntity = query.AddLink(ApiStreet.EntityLogicalName, "alt_streetid", "alt_streetid", JoinOperator.LeftOuter);
            streetLinkEntity.EntityAlias = streetEntityAlias;
            streetLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_streetcode");

            var issuingCountryEntity = query.AddLink(ApiCountry.EntityLogicalName, "alt_secondaryidentificationissuingcountryid", "alt_countryid", JoinOperator.LeftOuter);
            issuingCountryEntity.EntityAlias = secIdentificationIssuingCountryEntityAlias;
            issuingCountryEntity.Columns.AddColumns(primaryAttributeName, countryCodeAttributeName);

            var accountHolders = base.GetMultipleAsEntity(query);
            return this.MappToApiEntity(accountHolders);
        }

        public ApiAccountHolder GetAccountHolderDetails(Guid id)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiAccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(attributesToRetrieve),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("alt_accountholderid", ConditionOperator.Equal, id)
                    }
                },
                NoLock = true
            };

            var portfolioLinkEntity = query.AddLink(ApiPortfolio.EntityLogicalName, "alt_portfolioid", "alt_portfolioid", JoinOperator.LeftOuter);
            portfolioLinkEntity.EntityAlias = portfolioEntityAlias;
            portfolioLinkEntity.Columns.AddColumns(primaryAttributeName, "alt_shenhavaccountnumber");

            var digitalFormVerificationLinkEntity = query.AddLink(ApiDigitalFormVerification.EntityLogicalName, "alt_digitalformverificationid", "alt_digitalformverificationid", JoinOperator.LeftOuter);
            digitalFormVerificationLinkEntity.EntityAlias = digitalFormVerificationEntityAlias;
            digitalFormVerificationLinkEntity.Columns.AddColumns("alt_commissionclienttypeid");

            var commissionClientTypeLinkEntity = digitalFormVerificationLinkEntity.AddLink(ApiCommissionClientType.EntityLogicalName, "alt_commissionclienttypeid", "alt_commissionclienttypeid", JoinOperator.LeftOuter);
            commissionClientTypeLinkEntity.EntityAlias = commissionClientTypeEntityAlias;
            commissionClientTypeLinkEntity.Columns.AddColumns("alt_tradeonegroup");

            var accountHolder = base.GetMultipleAsEntity(query).Entities.FirstOrDefault();
            return accountHolder != null ? this.MappToApiEntity(accountHolder) : null;
        }

        public bool IsAccountHolderExist(ApiAccountHolder apiAccountHolder, out Guid? accountHolderId)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            accountHolderId = null;
            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiAccountHolder.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("alt_digitalformverificationid", ConditionOperator.Equal, apiAccountHolder.DigitalFormVerification.Id),
                        new ConditionExpression("alt_identificationnumber", ConditionOperator.Equal, apiAccountHolder.IdentificationNumber)
                    }
                },
                NoLock = true
            };

            var accountHolder = OrganizationService.RetrieveMultiple(query).Entities?.FirstOrDefault();
            if (accountHolder != null)
            {
                accountHolderId = accountHolder.Id;
            }
            return accountHolder != null;
        }

        private List<ApiAccountHolder> MappToApiEntity(EntityCollection accountHolders)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            List<ApiAccountHolder> apiAccountHolders = null;
            if (accountHolders.Entities.Count > 0)
            {
                apiAccountHolders = new List<ApiAccountHolder>();

                foreach (var entity in accountHolders.Entities)
                {
                    apiAccountHolders.Add(this.MappToApiEntity(entity));
                }
            }
            return apiAccountHolders;
        }

        private ApiAccountHolder MappToApiEntity(Entity entity)
        {
            this.GlobalContext.LogEntry();

            ApiAccountHolder apiAccountHolder = base.MappCrmEntityToApiEntity(entity);
            this.MappAliasedAttributes(apiAccountHolder, entity);
            this.MappDigitalFormVerification(apiAccountHolder, entity);

            return apiAccountHolder;
        }

        private void MappDigitalFormVerification(ApiAccountHolder apiAccountHolder, Entity entity)
        {
            this.GlobalContext.LogEntry();

            if (apiAccountHolder.DigitalFormVerification != null)
            {
                var commissionClientType = entity.GetAliasedAttributeValue<EntityReference>(digitalFormVerificationEntityAlias, "alt_commissionclienttypeid");
                if (commissionClientType != null)
                {
                    ApiCommissionClientType apiCommissionClientType = new ApiCommissionClientType { Id = commissionClientType.Id };
                    apiCommissionClientType.TradeOneGroup = entity.GetAliasedAttributeValue<string>(commissionClientTypeEntityAlias, "alt_tradeonegroup");
                    apiAccountHolder.DigitalFormVerification.CommissionClientType = apiCommissionClientType;
                }
            }
        }

        private void MappAliasedAttributes(ApiAccountHolder apiAccountHolder, Entity entity)
        {
            this.GlobalContext.LogEntry();
            if (apiAccountHolder.IdentificationIssuingCountry != null)
            {
                apiAccountHolder.IdentificationIssuingCountry.Name = entity.GetAliasedAttributeValue<string>(identificationIssuingCountryEntityAlias, primaryAttributeName);
                apiAccountHolder.IdentificationIssuingCountry.Code = entity.GetAliasedAttributeValue<string>(identificationIssuingCountryEntityAlias, countryCodeAttributeName);
            }
            if (apiAccountHolder.SecondaryIdentificationIssuingCountry != null)
            {
                apiAccountHolder.SecondaryIdentificationIssuingCountry.Name = entity.GetAliasedAttributeValue<string>(secIdentificationIssuingCountryEntityAlias, primaryAttributeName);
                apiAccountHolder.SecondaryIdentificationIssuingCountry.Code = entity.GetAliasedAttributeValue<string>(secIdentificationIssuingCountryEntityAlias, countryCodeAttributeName);
            }
            if (apiAccountHolder.Country != null)
            {
                apiAccountHolder.Country.Name = entity.GetAliasedAttributeValue<string>(countryEntityAlias, primaryAttributeName);
                apiAccountHolder.Country.Code = entity.GetAliasedAttributeValue<string>(countryEntityAlias, countryCodeAttributeName);
            }
            if (apiAccountHolder.City != null)
            {
                apiAccountHolder.City.Name = entity.GetAliasedAttributeValue<string>(cityEntityAlias, primaryAttributeName);
                apiAccountHolder.City.Code = entity.GetAliasedAttributeValue<string>(cityEntityAlias, "alt_code");
            }
            if (apiAccountHolder.BirthCountry != null)
            {
                apiAccountHolder.BirthCountry.Name = entity.GetAliasedAttributeValue<string>(birthCountryEntityAlias, primaryAttributeName);
                apiAccountHolder.BirthCountry.Code = entity.GetAliasedAttributeValue<string>(birthCountryEntityAlias, countryCodeAttributeName);
            }
            if (apiAccountHolder.Street != null)
            {
                apiAccountHolder.Street.Name = entity.GetAliasedAttributeValue<string>(streetEntityAlias, primaryAttributeName);
                apiAccountHolder.Street.StreetCode = entity.GetAliasedAttributeValue<string>(streetEntityAlias, "alt_streetcode");
            }
            if (apiAccountHolder.Portfolio != null)
            {
                apiAccountHolder.Portfolio.Name = entity.GetAliasedAttributeValue<string>(portfolioEntityAlias, primaryAttributeName);
                apiAccountHolder.Portfolio.ShenhavAccountNumber = entity.GetAliasedAttributeValue<string>(portfolioEntityAlias, "alt_shenhavaccountnumber");
            }
        }
    }
}
