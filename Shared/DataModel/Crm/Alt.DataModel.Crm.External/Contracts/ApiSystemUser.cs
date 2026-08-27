using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiSystemUser: ApiEntity
    {
        public const string EntityLogicalName = "systemuser";
        public ApiSystemUser() : base(EntityLogicalName)
        {
        }
        private string fullName;
        [CrmEntityMapper("fullname", CrmPropertyType.String)]
        public string FullName
        {
            get { return fullName; }
            set
            {
                this.SetProperty(value);
                fullName = value;
            }
        }

        private string domainName;
        [CrmEntityMapper("domainname", CrmPropertyType.String, mappToCrm: false, mappFromCrm: true)]
        public string DomainName
        {
            get { return this.domainName; }
            set
            {
                this.SetProperty(value);
                this.domainName = value;
            }
        }

        private string internalEmailAddress;
        [CrmEntityMapper("internalemailaddress", CrmPropertyType.String, mappToCrm: false, mappFromCrm: true)]
        public string InternalEmailAddress
        {
            get { return this.internalEmailAddress; }
            set
            {
                this.SetProperty(value);
                this.internalEmailAddress = value;
            }
        }
    }
}
