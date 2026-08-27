using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class Header : ExternalEntityBase
    {
        private string requestID;
        [Required]
        public string RequestID
        {
            get => this.requestID;
            set
            {
                base.SetProperty(value);
                this.requestID = value;
            }
        }

        private string requestTimestamp;
       // [Required]
        public string RequestTimestamp
        {
            get => this.requestTimestamp;
            set
            {
                base.SetProperty(value);
                this.requestTimestamp = value;
            }
        }

        private int? requestingUserID;
       // [Required]
        public int? RequestingUserID
        {
            get => this.requestingUserID;
            set
            {
                base.SetProperty(value);
                this.requestingUserID = value;
            }
        }

        private string userIP;
       // [Required]
        public string UserIP
        {
            get => this.userIP;
            set
            {
                base.SetProperty(value);
                this.userIP = value;
            }
        }

        private string version;
        public string Version
        {
            get => this.version;
            set
            {
                base.SetProperty(value);
                this.version = value;
            }
        }
    }
}
