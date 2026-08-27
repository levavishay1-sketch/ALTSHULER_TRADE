using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiIncident : ApiEntity
    {
        public const string EntityLogicalName = "incident";
        public ApiIncident() : base(EntityLogicalName)
        {
        }

        private ApiCustomer customer;
        [CrmEntityMapper("customerid", CrmPropertyType.EntityReference)]
        public ApiCustomer Customer
        {
            get
            {
                return this.customer;
            }
            set
            {
                this.SetProperty(value);
                this.customer = value;
            }
        }

        private ApiSubject2 subject2;
        [CrmEntityMapper("alt_subject2id", CrmPropertyType.EntityReference)]
        public ApiSubject2 Subject2
        {
            get
            {
                return this.subject2;
            }
            set
            {
                this.SetProperty(value);
                this.subject2 = value;
            }
        }

        private ApiSubject1 subject1;
        [CrmEntityMapper("alt_subject1id", CrmPropertyType.EntityReference)]
        public ApiSubject1 Subject1
        {
            get
            {
                return this.subject1;
            }
            set
            {
                this.SetProperty(value);
                this.subject1 = value;
            }
        }

        private ApiIncidentStatus incidentStatus;
        [CrmEntityMapper("alt_incidentstatusid", CrmPropertyType.EntityReference)]
        public ApiIncidentStatus IncidentStatus
        {
            get
            {
                return this.incidentStatus;
            }
            set
            {
                this.SetProperty(value);
                this.incidentStatus = value;
            }
        }

        private string description;
        [CrmEntityMapper("description", CrmPropertyType.String)]
        [StringLength(2000)]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.SetProperty(value);
                this.description = value;
            }
        }

        private int? sourceSystemCode;
        [CrmEntityMapper("alt_sourcesystemcode", CrmPropertyType.OptionSet)]
        public int? SourceSystemCode
        {
            get
            {
                return this.sourceSystemCode;
            }
            set
            {
                this.SetProperty(value);
                this.sourceSystemCode = value;
            }
        }

        private ApiIncident parentCase;
        [CrmEntityMapper("parentcaseid", CrmPropertyType.EntityReference)]
        public ApiIncident ParentCase
        {
            get
            {
                return this.parentCase;
            }
            set
            {
                this.SetProperty(value);
                this.parentCase = value;
            }
        }

        private ApiSystemUser responsibleSystemUser;
        [CrmEntityMapper("alt_responsiblesystemuserid", CrmPropertyType.EntityReference)]
        public ApiSystemUser ResponsibleSystemUser
        {
            get
            {
                return this.responsibleSystemUser;
            }
            set
            {
                this.SetProperty(value);
                this.responsibleSystemUser = value;
            }
        }

        private string title;
        [CrmEntityMapper("title", CrmPropertyType.String)]
        [StringLength(200)]
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.SetProperty(value);
                this.title = value;
            }
        }

        private int? caseOriginCode;
        [CrmEntityMapper("caseorigincode", CrmPropertyType.OptionSet)]
        public int? CaseOriginCode
        {
            get
            {
                return this.caseOriginCode;
            }
            set
            {
                this.SetProperty(value);
                this.caseOriginCode = value;
            }
        }

        private int? incidentTypeCode;
        [CrmEntityMapper("alt_incidenttypecode", CrmPropertyType.OptionSet)]
        public int? IncidentTypeCode
        {
            get
            {
                return this.incidentTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.incidentTypeCode = value;
            }
        }

        private string automaticIncidentTemplateKey;
        [CrmEntityMapper("alt_automaticincidenttemplatekey", CrmPropertyType.String)]
        [StringLength(100)]
        public string AutomaticIncidentTemplateKey
        {
            get
            {
                return this.automaticIncidentTemplateKey;
            }
            set
            {
                this.SetProperty(value);
                this.automaticIncidentTemplateKey = value;
            }
        }

        private string dynamicFormPcfConfigJson;
        [CrmEntityMapper("alt_dynamicformpcfconfigjson", CrmPropertyType.String)]
        public string DynamicFormPcfConfigJson
        {
            get
            {
                return dynamicFormPcfConfigJson;
            }
            set
            {
                this.SetProperty(value);
                this.dynamicFormPcfConfigJson = value;
            }
        }

        private ApiEntity operationalProcess;
        [CrmEntityMapper("alt_operationalprocessid", CrmPropertyType.EntityReference)]
        public ApiEntity OperationalProcess
        {
            get
            {
                return this.operationalProcess;
            }
            set
            {
                this.SetProperty(value);
                this.operationalProcess = value;
            }
        }

        private ApiPortfolio portfolioId;
        /// <summary>
        /// חשבון שנהב
        /// </summary>
        [CrmEntityMapper("alt_portfolioid", CrmPropertyType.EntityReference)]
        public ApiPortfolio Portfolio
        {
            get
            {
                return portfolioId;
            }
            set
            {
                this.SetProperty(value);
                this.portfolioId = value;
            }
        }

        private string externalIdentifier;
        /// <summary>
        /// מזהה חיצוני
        /// </summary>
        [StringLength(100)]
        [CrmEntityMapper("alt_externalidentifier", CrmPropertyType.String)]
        public string ExternalIdentifier
        {
            get
            {
                return externalIdentifier;
            }
            set
            {
                this.SetProperty(value);
                this.externalIdentifier = value;
                this.SetEntityKeys("alt_externalidentifier", value);
            }
        }

        private string operationalDetails;
        public string OperationalDetails
        {
            get
            {
                return operationalDetails;
            }
            set
            {
                this.SetProperty(value);
                this.operationalDetails = value;
            }
        }
    }
}
