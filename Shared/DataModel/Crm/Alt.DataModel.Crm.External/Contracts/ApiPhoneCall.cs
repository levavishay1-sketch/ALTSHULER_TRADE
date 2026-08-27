using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiPhoneCall : ApiActivityPointer
    {
        public ApiPhoneCall(string logicalName) : base(logicalName) { }

        public const string EntityLogicalName = "phonecall";

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

        private int? phoneStatusCode;
        [CrmEntityMapper("alt_statuscode", CrmPropertyType.OptionSet)]
        public int? PhoneStatusCode
        {
            get
            {
                return this.phoneStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.phoneStatusCode = value;
            }
        }

        private DateTime? scheduledEnd;
        [CrmEntityMapper("scheduledend", CrmPropertyType.DateTime)]
        public DateTime? ScheduledEnd
        {
            get
            {
                return this.scheduledEnd;
            }
            set
            {
                this.SetProperty(value);
                this.scheduledEnd = value;
            }
        }


        private string phoneNumber;
        /// <summary>
        /// מספר טלפון
        /// </summary>
        [CrmEntityMapper("phonenumber", CrmPropertyType.String)]
        public string PhoneNumber
        {
            get
            {
                return this.phoneNumber;
            }
            set
            {
                this.SetProperty(value);
                this.phoneNumber = value;
            }
        }

        private bool? directionCode;
        /// <summary>
        /// כיוון
        /// </summary>
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


        private int? priorityCode;
        /// <summary>
        /// עדיפות
        /// </summary>
        [CrmEntityMapper("prioritycode", CrmPropertyType.OptionSet)]
        public int? PriorityCode
        {
            get
            {
                return this.priorityCode;
            }
            set
            {
                this.SetProperty(value);
                this.priorityCode = value;
            }
        }

        private bool? completeActivity;
        /// <summary>
        /// האם להשלים פעילות - פנימי
        /// </summary>
        [CrmEntityMapper("alt_completeactivitybit", CrmPropertyType.Bool)]
        public bool? CompleteActivity
        {
            get
            {
                return this.completeActivity;
            }
            set
            {
                this.SetProperty(value);
                this.completeActivity = value;
            }
        }
    }
}