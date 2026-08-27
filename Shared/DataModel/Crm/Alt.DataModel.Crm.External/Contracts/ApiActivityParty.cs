using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiActivityParty : ApiEntityBase
    {
        public ApiActivityParty() : base(null)
        {
        }

        public ApiActivityParty(string logicalName) : base(logicalName)
        {
        }
        private int? activityPartyTypeCode;
        public int? ActivityPartyTypeCode
        {
            get { return activityPartyTypeCode; }
            set
            {
                this.SetProperty(value);
                this.activityPartyTypeCode = value;
                if (this.activityPartyTypeCode != null && string.IsNullOrWhiteSpace(LogicalName))
                {
                    switch ((ActivityPartyType)this.activityPartyTypeCode)
                    {
                        case ActivityPartyType.Account:
                            base.LogicalName = ApiAccount.EntityLogicalName;
                            break;
                        case ActivityPartyType.Contact:
                            base.LogicalName = ApiContact.EntityLogicalName;
                            break;
                        case ActivityPartyType.Lead:
                            base.LogicalName = ApiLead.EntityLogicalName;
                            break;
                        case ActivityPartyType.SystemUser:
                            base.LogicalName = ApiSystemUser.EntityLogicalName;
                            break;
                        case ActivityPartyType.Queue:
                            base.LogicalName = ApiQueue.EntityLogicalName;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private string addressUsed;
        [CrmEntityMapper("addressused", CrmPropertyType.String, true, false)]
        public string AddressUsed
        {
            get { return this.addressUsed; }
            set
            {
                this.SetProperty(value);
                this.addressUsed = value;
            }
        }

        private ApiEntity partyid;
        [CrmEntityMapper("partyid", CrmPropertyType.EntityReference)]
        public ApiEntity Partyid
        {
            get { return this.partyid; }
            set
            {
                this.SetProperty(value);
                this.partyid = value;
            }
        }
    }
}
