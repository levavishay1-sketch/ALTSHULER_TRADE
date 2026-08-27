using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;


namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiCustomerOperationTemplate : ApiEntity
    {
        public const string EntityLogicalName = "alt_customeroperationtemplate";
        public ApiCustomerOperationTemplate() : base(EntityLogicalName)
        {
        }

        private string name;
        /// <summary>
        ///שם 
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_name", CrmPropertyType.String)]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.SetProperty(value);
                this.name = value;
            }
        }

        private int? code;
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get
            {
                return code;
            }
            set
            {
                this.SetProperty(value);
                this.code = value;
            }
        }

        /// <summary>
        /// תבנית מסרון
        /// </summary>
        private ApiSmsTemplate smsTemplateId;
        [CrmEntityMapper("alt_smstemplateid", CrmPropertyType.EntityReference)]
        public ApiSmsTemplate SmsTemplateId
        {
            get
            {
                return smsTemplateId;
            }
            set
            {
                this.SetProperty(value);
                this.smsTemplateId = value;
            }
        }

        /// <summary>
        /// הגדרת API
        /// </summary>
        private ApiConfiguration apiConfigurationId;
        [CrmEntityMapper("alt_apiconfigurationid", CrmPropertyType.EntityReference)]
        public ApiConfiguration ApiConfigurationId
        {
            get
            {
                return apiConfigurationId;
            }
            set
            {
                this.SetProperty(value);
                this.apiConfigurationId = value;
            }
        }

        /// <summary>
        /// תבנית דואר אלקטרוני
        /// </summary>
        private ApiEmailTemplate emailTemplateId;

        [CrmEntityMapper("alt_emailtemplateid", CrmPropertyType.EntityReference)]
        public ApiEmailTemplate EmailTemplateId
        {
            get
            {
                return emailTemplateId;
            }
            set
            {
                this.SetProperty(value);
                this.emailTemplateId = value;
            }
        }
    }
}
