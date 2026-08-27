

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class ApiActivityPointerBase : ApiEntityBase
    {
        public ApiActivityPointerBase(string logicalName) : base(logicalName)
        {
        }

        protected ApiEntityBase regardingObject;
        public virtual ApiEntityBase RegardingObject
        {
            get
            {
                return regardingObject;
            }
            set
            {
                this.SetProperty(value);
                regardingObject = value;
            }
        }

        protected string subject;
        public virtual string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                this.SetProperty(value);
                subject = value;
            }
        }

        protected string description;
        public virtual string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.SetProperty(value);
                description = value;
            }
        }
    }
}
