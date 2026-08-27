using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Alt.DataAccessLayer.Crm.External
{
    public class SmsDAL : CrmExternalBaseDAL<ApiSms>
    {
        string[] attributes =
        {
            "description",
            "alt_mobilephone",
            "alt_smstemplateid",
            "statuscode"
        };
        public SmsDAL(GlobalContext globalContext) : base(globalContext, ApiSms.EntityLogicalName) { }

        public ApiSms GetSmsDetails(Guid smsId)
        {
            this.GlobalContext.LogEntry();

            var query = new QueryExpression(ApiSms.EntityLogicalName);
            query.ColumnSet.AddColumns(attributes);
            query.Criteria.AddCondition("activityid", ConditionOperator.Equal, smsId);

            var smsTemplateLinkEntity = query.AddLink(ApiSmsTemplate.EntityLogicalName, "alt_smstemplateid", "alt_smstemplateid",JoinOperator.LeftOuter);
            smsTemplateLinkEntity.EntityAlias = "smsTemplate";
            smsTemplateLinkEntity.Columns.AddColumns("alt_sendby");

            var sms = this.GetMultipleAsEntity(query).Entities.FirstOrDefault();
            return sms != null ? this.MappToApiSms(sms) : null;
        }

        private ApiSms MappToApiSms(Microsoft.Xrm.Sdk.Entity sms)
        {
            ApiSms apiSms = base.MappCrmEntityToApiEntity(sms);
            if (apiSms.SmsTemplate != null)
            {
                apiSms.SmsTemplate.SendBy = sms.GetAliasedAttributeValue<string>("smsTemplate", "alt_sendby");
            }
            return apiSms;
        }
    }
}
