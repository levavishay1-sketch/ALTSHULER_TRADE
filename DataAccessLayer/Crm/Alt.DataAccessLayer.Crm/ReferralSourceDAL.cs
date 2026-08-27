using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class ReferralSourceDAL :  CrmBaseDAL<alt_ReferralSource>
    {
        public ReferralSourceDAL(GlobalContext globalContext) : base(globalContext, alt_ReferralSource.EntityLogicalName)
        {
        }

        public int? GetReferralSourceCodeById(EntityReference referralSource)
        {
            var result = GlobalContext.CacheManager.GetCachedItem(nameof(alt_ReferralSource),
                () => this.GetAll(),
                5);
            var filteredSources = result?.Where(r => r.Id == referralSource.Id).FirstOrDefault();
            return filteredSources != null ?
                filteredSources.alt_CodeInt : null;
        }

        public List<alt_ReferralSource> GetAll()
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = alt_ReferralSource.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
   
                            new ConditionExpression(alt_AccountHolder.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    }
            };
            return this.GetMultiple(query);
        }

        public bool IsReferralSourceMivtza(EntityReference referralSource)
        {
            var marketingSources = GlobalContext.CacheManager.GetGlobalParameter<string>("ClubMembershipEligibilityMarketingSources");
            var sources = marketingSources?.Split(',').Select(s => int.Parse(s.Trim())).ToList();

            ReferralSourceDAL referralSourceDal = new ReferralSourceDAL(this.GlobalContext);
            var referralSourceCode = referralSourceDal.GetReferralSourceCodeById(referralSource);

            return referralSourceCode.HasValue && sources.Contains(referralSourceCode.Value);
        }
    }
}
