using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiActivityMimeAttachment : ApiEntity
    {
        public const string EntityLogicalName = "activitymimeattachment";
        public ApiActivityMimeAttachment() : base(EntityLogicalName)
        {
        }
        private string subject;
        [CrmEntityMapper("subject", CrmPropertyType.String)]
        public string Subject
        {
            get { return subject; }
            set
            {
                this.SetProperty(value);
                this.subject = value;
            }
        }
        private string fileName;
        [CrmEntityMapper("filename", CrmPropertyType.String)]
        public string FileName
        {
            get { return fileName; }
            set
            {
                this.SetProperty(value);
                this.fileName = value;
            }
        }
        private string mimeType;
        [CrmEntityMapper("mimetype", CrmPropertyType.String)]
        public string MimeType
        {
            get { return mimeType; }
            set
            {
                this.SetProperty(value);
                this.mimeType = value;
            }
        }

        private int attachmentNumber;
        [CrmEntityMapper("attachmentnumber", CrmPropertyType.Int)]
        public int AttachmentNumber
        {
            get { return attachmentNumber; }
            set
            {
                this.SetProperty(value);
                this.attachmentNumber = value;
            }
        }

        private /*int?*/string objectTypeCode;
        [CrmEntityMapper("objecttypecode", CrmPropertyType.String)]
        public string ObjectTypeCode
        {
            get { return objectTypeCode; }
            set
            {
                this.SetProperty(value);
                this.objectTypeCode = value;
            }
        }

        private ApiEntity objectId;
        [CrmEntityMapper("objectid", CrmPropertyType.EntityReference)]
        public ApiEntity ObjectId
        {
            get { return objectId; }
            set
            {
                this.SetProperty(value);
                this.objectId = value;
            }
        }

        private string body;
        [CrmEntityMapper("body", CrmPropertyType.String)]
        public string Body
        {
            get { return body; }
            set
            {
                this.SetProperty(value);
                this.body = value;
            }
        }
    }
}
