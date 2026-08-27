using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiEmail : ApiActivityPointer
    {
        public const string EntityLogicalName = "email";

        public ApiEmail() : base(EntityLogicalName)
        {
        }

        private bool? directionCode;

        [CrmEntityMapper("directioncode", CrmPropertyType.Bool)]
        public bool? DirectionCode
        {
            get
            {
                return this.directionCode;
            }
            set
            {
                this.SetProperty(value);
                this.directionCode = value;
            }
        }

        private DateTime? sentOn;
        [CrmEntityMapper("senton", CrmPropertyType.DateTime)]
        public DateTime? SentOn
        {
            get
            {
                return this.sentOn;
            }
            set
            {
                this.SetProperty(value);
                this.sentOn = value;
            }
        }

        private int? templateCodeInt;
        [CrmEntityMapper("alt_templatecodeint", CrmPropertyType.Int)]
        public int? TemplateCodeInt
        {
            get
            {
                return templateCodeInt;
            }
            set
            {
                this.SetProperty(value);
                this.templateCodeInt = value;
            }
        }

        private bool? isAutomaticBit;
        [CrmEntityMapper("alt_isautomaticbit", CrmPropertyType.Bool)]
        public bool? IsAutomaticBit
        {
            get
            {
                return isAutomaticBit;
            }
            set
            {
                this.SetProperty(value);
                this.isAutomaticBit = value;
            }
        }

        private List<ApiActivityParty> related;

        [CrmEntityMapper("related", CrmPropertyType.ActivityParty, true, false)]
        public List<ApiActivityParty> Related
        {
            get
            {
                return related;
            }
            set
            {
                this.SetProperty(value);
                related = value;
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

        private ApiEntity emailTemplateId;
        [CrmEntityMapper("alt_emailtemplateid", CrmPropertyType.EntityReference)]
        public ApiEntity EmailTemplateId
        {
            get => emailTemplateId;
            set
            {
                this.SetProperty(value);
                emailTemplateId = value;
            }
        }
    }
}
