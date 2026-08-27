using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiSms : ApiActivityPointer
    {
        public const string EntityLogicalName = "alt_sms";

        public ApiSms() : base(EntityLogicalName)
        {
        }

        private string mobilePhone;
        [CrmEntityMapper("alt_mobilephone", CrmPropertyType.String)]
        public string MobilePhone
        {
            get
            {
                return mobilePhone;
            }
            set
            {
                this.SetProperty(value);
                this.mobilePhone = value;
            }
        }

        private ApiSmsTemplate smsTemplate;
        [CrmEntityMapper("alt_smstemplateid", CrmPropertyType.EntityReference)]
        public ApiSmsTemplate SmsTemplate
        {
            get
            {
                return smsTemplate;
            }
            set
            {
                this.SetProperty(value);
                this.smsTemplate = value;
            }
        }

        private string sendResult;
        [CrmEntityMapper("alt_sendresult", CrmPropertyType.String)]
        public string SendResult
        {
            get
            {
                return sendResult;
            }
            set
            {
                this.SetProperty(value);
                this.sendResult = value;
            }
        }

        private bool? isAutomatic;
        [CrmEntityMapper("alt_isautomaticbit", CrmPropertyType.Bool)]
        public bool? IsAutomatic
        {
            get
            {
                return isAutomatic;
            }
            set
            {
                this.SetProperty(value);
                this.isAutomatic = value;
            }
        }

        private int? templateCode;
        [CrmEntityMapper("alt_templatecodeint", CrmPropertyType.Int)]
        public int? TemplateCode
        {
            get
            {
                return templateCode;
            }
            set
            {
                this.SetProperty(value);
                this.templateCode = value;
            }
        }

        private ApiContact contactId;
        [CrmEntityMapper("alt_contactid", CrmPropertyType.EntityReference)]
        public ApiContact ContactId
        {
            get
            {
                return contactId;
            }
            set
            {
                this.SetProperty(value);
                contactId = value;
            }
        }

        private string parserCustomEntryPoint;
        [CrmEntityMapper("alt_parsercustomentrypoint", CrmPropertyType.String)]
        public string ParserCustomEntryPoint
        {
            get => parserCustomEntryPoint;
            set
            {
                this.SetProperty(value);
                parserCustomEntryPoint = value;
            }
        }
    }
}
