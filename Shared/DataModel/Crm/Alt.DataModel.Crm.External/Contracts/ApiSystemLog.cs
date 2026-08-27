using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiSystemLog : ApiEntity
    {
        public const string EntityLogicalName = "alt_systemlog";
        public ApiSystemLog() : base(EntityLogicalName)
        {
        }
        private int? messageLevelCode;

        [CrmEntityMapper("alt_messagelevelcode", CrmPropertyType.OptionSet)]
        public int? MessageLevelCode
        {
            get
            {
                return messageLevelCode;
            }
            set
            {
                this.SetProperty(value);
                this.messageLevelCode = value;
            }
        }

        private int? entryPointTypeCode;

        [CrmEntityMapper("alt_entrypointtypecode", CrmPropertyType.OptionSet)]
        public int? EntryPointTypeCode
        {
            get
            {
                return entryPointTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.entryPointTypeCode = value;
            }
        }

        private string messageBlock;
        [CrmEntityMapper("alt_messageblock", CrmPropertyType.String)]
        public string MessageBlock
        {
            get
            {
                return messageBlock;
            }
            set
            {
                this.SetProperty(value);
                this.messageBlock = value;
            }
        }

        private string name;
        [CrmEntityMapper("alt_name", CrmPropertyType.String)]
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

        private string targetId;
        /// <summary>
        /// מזהה יעד
        /// </summary>
        [CrmEntityMapper("alt_targetid", CrmPropertyType.String)]
        public string TargetId
        {
            get
            {
                return targetId;
            }
            set
            {
                this.SetProperty(value);
                this.targetId = value;
            }
        }

        private string targetLogicalName;
        /// <summary>
        /// שם יעד לוגי
        /// </summary>
        [CrmEntityMapper("alt_targetlogicalname", CrmPropertyType.String)]
        public string TargetLogicalName
        {
            get
            {
                return targetLogicalName;
            }
            set
            {
                this.SetProperty(value);
                this.targetLogicalName = value;
            }
        }
    }
}
