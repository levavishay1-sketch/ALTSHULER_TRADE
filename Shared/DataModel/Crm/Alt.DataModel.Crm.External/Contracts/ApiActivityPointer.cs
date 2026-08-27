using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiActivityPointer : ApiActivityPointerBase
    {
        public ApiActivityPointer(string logicalName) : base(logicalName)
        {
        }

        [CrmEntityMapper("activityid", CrmPropertyType.Guid)]
        public override Guid? Id
        {
            get
            {
                return base.id;
            }
            set
            {
                base.SetProperty(value);
                base.id = value;
            }
        }

        [CrmEntityMapper("regardingobjectid", CrmPropertyType.EntityReference)]
        public override ApiEntityBase RegardingObject
        {
            get
            {
                return base.regardingObject;
            }
            set
            {
                base.SetProperty(value);
                base.regardingObject = value;
            }
        }

        [CrmEntityMapper("subject", CrmPropertyType.String)]
        public override string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                base.SetProperty(value);
                subject = value;
            }
        }

        [CrmEntityMapper("description", CrmPropertyType.String)]
        public override string Description
        {
            get
            {
                return description;
            }
            set
            {
                base.SetProperty(value);
                description = value;
            }
        }

        [CrmEntityMapper("statuscode", CrmPropertyType.OptionSet)]
        public override int? StatusCode
        {
            get
            {
                return base.statusCode;
            }
            set
            {
                base.SetProperty(value);
                base.statusCode = value;
            }
        }

        [CrmEntityMapper("statecode", CrmPropertyType.OptionSet)]
        public override int? StateCode
        {
            get
            {
                return base.stateCode;
            }
            set
            {
                base.SetProperty(value);
                base.stateCode = value;
            }        
        }

        [CrmEntityMapper("createdon", CrmPropertyType.DateTime)]
        public override DateTime? CreatedOn
        {
            get
            {
                return base.createdOn;
            }
            set
            {
                base.SetProperty(value);
                base.createdOn = value;
            }
        }

        [CrmEntityMapper("alt_creationmethodcode", CrmPropertyType.OptionSet)]
        public override int? CreationMethodCode
        {
            get
            {
                return base.creationMethodCode;
            }
            set
            {
                base.SetProperty(value);
                base.creationMethodCode = value;
            }
        }

        protected ApiSystemUser modifiedBy;
        [CrmEntityMapper("modifiedby", CrmPropertyType.EntityReference)]
        public virtual ApiSystemUser ModifiedBy
        {
            get
            {
                return this.modifiedBy;
            }
            set
            {
                this.SetProperty(value);
                this.modifiedBy = value;
            }
        }

        [CrmEntityMapper("ownerid", CrmPropertyType.EntityReference)]
        public override ApiEntityBase Owner
        {
            get
            {
                return this.owner;
            }
            set
            {
                base.SetProperty(value);
                this.owner = value;
            }
        }

        private List<ApiActivityParty> toActivityPartyList;
        [CrmEntityMapper("to", CrmPropertyType.ActivityParty,true,false)]
        public List<ApiActivityParty> ToActivityPartyList
        {
            get
            {
                return toActivityPartyList;
            }
            set
            {
                this.SetProperty(value);
                toActivityPartyList = value;
            }
        }

        private List<ApiActivityParty> fromActivityPartyList;
        [CrmEntityMapper("from", CrmPropertyType.ActivityParty, true, false)]
        public List<ApiActivityParty> FromActivityPartyList
        {
            get
            {
                return fromActivityPartyList;
            }
            set
            {
                this.SetProperty(value);
                fromActivityPartyList = value;
            }
        }

        private ApiActivityParty to;
        [CrmEntityMapper("to", CrmPropertyType.ActivityParty, true, false)]
        public ApiActivityParty To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.SetProperty(value);
                this.to = value;
            }
        }

        private ApiActivityParty from;
        [CrmEntityMapper("from", CrmPropertyType.ActivityParty, true, false)]
        public ApiActivityParty From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.SetProperty(value);
                this.from = value;
            }
        }
    }
}
