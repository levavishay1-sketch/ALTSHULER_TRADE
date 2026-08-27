using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiError : ApiEntity
    {
        public const string EntityLogicalName = "alt_error";
        public ApiError() : base(EntityLogicalName) { }
        private string errorMessage;
        [CrmEntityMapper("alt_errormessage", CrmPropertyType.String)]
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                this.SetProperty(value);
                this.errorMessage = value;
            }
        }

        private string errorKey;
        [CrmEntityMapper("alt_errorkey", CrmPropertyType.String)]
        public string ErrorKey
        {
            get
            {
                return errorKey;
            }
            set
            {
                this.SetProperty(value);
                this.errorKey = value;
            }
        }

        private string description;
        [CrmEntityMapper("alt_description", CrmPropertyType.String)]
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
    }
}
