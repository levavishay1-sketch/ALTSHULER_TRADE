using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiAsyncOperation : ApiActivityPointer
    {
        public const string EntityLogicalName = "asyncoperation";
        public ApiAsyncOperation() : base(EntityLogicalName)
        {
        }
        private string name;
        [CrmEntityMapper("name", CrmPropertyType.String)]
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

        private string friendlyMessage;
        [CrmEntityMapper("friendlymessage", CrmPropertyType.String)]
        public string FriendlyMessage
        {
            get
            {
                return friendlyMessage;
            }
            set
            {
                this.SetProperty(value);
                this.friendlyMessage = value;
            }
        }

        private string message;
        [CrmEntityMapper("message", CrmPropertyType.String)]
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                this.SetProperty(value);
                this.message = value;
            }
        }

        private DateTime? startedOn;
        [CrmEntityMapper("startedon", CrmPropertyType.DateTime)]
        public DateTime? StartedOn
        {
            get
            {
                return startedOn;
            }
            set
            {
                this.SetProperty(value);
                this.startedOn = value;
            }
        }
    }
}
