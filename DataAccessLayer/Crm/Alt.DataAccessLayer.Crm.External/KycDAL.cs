using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class KycDAL : CrmExternalBaseDAL<ApiKYC>
    {
        const string countryEntityAlias = "country";
        const string primaryAttributeName = "alt_name";
        const string countryCodeAttributeName = "alt_code";
        const string occupationEntityAlias = "occupation";
        const string occupationCodeAttributeName = "alt_codeint";

        public KycDAL(GlobalContext globalContext) : base(globalContext, ApiKYC.EntityLogicalName) { }

        public ApiKYC GetKycDetailsByAccountHolder(Guid accountHolderId)
        {
            this.GlobalContext.LogEntry(entityLogicalName);

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiKYC.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("alt_accountholderid", ConditionOperator.Equal, accountHolderId)
                    }
                },
                NoLock = true
            };
            query.AddOrder("createdon", OrderType.Descending);

            var countryLinkEntity = query.AddLink(ApiCountry.EntityLogicalName, "alt_traderelationriskcountryid", "alt_countryid", JoinOperator.LeftOuter);
            countryLinkEntity.EntityAlias = countryEntityAlias;
            countryLinkEntity.Columns.AddColumns(primaryAttributeName, countryCodeAttributeName);

            var occupationLinkEntity = query.AddLink(ApiOccupation.EntityLogicalName, "alt_employmentcategoryoccupationid", "alt_occupationid", JoinOperator.LeftOuter);
            occupationLinkEntity.EntityAlias = occupationEntityAlias;
            occupationLinkEntity.Columns.AddColumns(primaryAttributeName, occupationCodeAttributeName);

            var kyc = base.GetMultipleAsEntity(query).Entities.FirstOrDefault();
            return kyc != null ? this.MappToApiEntity(kyc) : null;
        }

        private ApiKYC MappToApiEntity(Entity entity)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            ApiKYC apiKYC = null;
            if (entity != null)
            {
                apiKYC = base.MappCrmEntityToApiEntity(entity);

                if (apiKYC.TradeRelationRiskCountryId != null)
                {
                    apiKYC.TradeRelationRiskCountryId.Name = entity.GetAliasedAttributeValue<string>(countryEntityAlias, primaryAttributeName);
                    apiKYC.TradeRelationRiskCountryId.Code = entity.GetAliasedAttributeValue<string>(countryEntityAlias, countryCodeAttributeName);
                }
                if (apiKYC.EmploymentCategoryOccupation != null)
                {
                    apiKYC.EmploymentCategoryOccupation.Name = entity.GetAliasedAttributeValue<string>(occupationEntityAlias, primaryAttributeName);
                    apiKYC.EmploymentCategoryOccupation.Code = entity.GetAliasedAttributeValue<int?>(occupationEntityAlias, occupationCodeAttributeName);
                }
            }
            return apiKYC;
        }
    }
}
