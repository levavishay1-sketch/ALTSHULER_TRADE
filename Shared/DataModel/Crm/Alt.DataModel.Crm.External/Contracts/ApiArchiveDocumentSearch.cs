using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiArchiveDocumentSearch : ApiActivityPointer
    {
        public const string EntityLogicalName = "alt_archivedocumentsearch";

        public ApiArchiveDocumentSearch() : base(EntityLogicalName)
        {
        }

        private int? searchFromArchiveStatusCode;
        [CrmEntityMapper("alt_searchfromarchivestatuscode", CrmPropertyType.OptionSet)]
        public int? SearchFromArchiveStatusCode
        {
            get
            {
                return searchFromArchiveStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.searchFromArchiveStatusCode = value;
            }
        }

        private ApiCustomer customer;
        public ApiCustomer Customer
        {
            get
            {
                return customer;
            }
            set
            {
                this.SetProperty(value);
                this.customer = value;
            }
        }

        private string processCode;
        public string ProcessCode
        {
            get
            {
                return processCode;
            }
            set
            {
                this.SetProperty(value);
                this.processCode = value;
            }
        }

        private DateTime? lastSearchDate;
        [CrmEntityMapper("alt_lastsearchdate", CrmPropertyType.DateTime)]
        public DateTime? LastSearchDate
        {
            get
            {
                return lastSearchDate;
            }
            set
            {
                this.SetProperty(value);
                this.lastSearchDate = value;
            }
        }
    }
}
