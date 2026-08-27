using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiDocument : ApiEntity
    {
        public const string EntityLogicalName = "alt_document";

        public ApiDocument() : base(EntityLogicalName)
        {
        }

        [CrmEntityMapper("createdon", CrmPropertyType.DateTime)]
        public override DateTime? CreatedOn
        {
            get
            {
                return base.createdOn;
            }
            set
            {
                base.SetProperty(value);
                base.createdOn = value;
            }
        }

        private ApiSystemUser createdBy;
        [CrmEntityMapper("createdby", CrmPropertyType.EntityReference)]
        public ApiSystemUser CreatedBy
        {
            get
            {
                return createdBy;
            }
            set
            {
                this.SetProperty(value);
                this.createdBy = value;
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

        private int? archiveTransferStatusCode;
        [CrmEntityMapper("alt_archivetransferstatuscode", CrmPropertyType.OptionSet)]
        public int? ArchiveTransferStatusCode
        {
            get
            {
                return archiveTransferStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.archiveTransferStatusCode = value;
            }
        }

        private int? archiveDownloadStatusCode;
        [CrmEntityMapper("alt_archivedownloadstatuscode", CrmPropertyType.OptionSet)]
        public int? ArchiveDownloadStatusCode
        {
            get
            {
                return archiveDownloadStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.archiveDownloadStatusCode = value;
            }
        }

        private int? archiveUpdateStatusCode;
        [CrmEntityMapper("alt_archiveupdatestatuscode", CrmPropertyType.OptionSet)]
        public int? ArchiveUpdateStatusCode
        {
            get
            {
                return archiveUpdateStatusCode;
            }
            set
            {
                this.SetProperty(value);
                this.archiveUpdateStatusCode = value;
            }
        }

        private string fileArchiveIdentifier;
        [CrmEntityMapper("alt_filearchiveidentifier", CrmPropertyType.String)]
        public string FileArchiveIdentifier
        {
            get
            {
                return fileArchiveIdentifier;
            }
            set
            {
                this.SetProperty(value);
                this.fileArchiveIdentifier = value;
            }
        }

        private string mimeType;
        [CrmEntityMapper("alt_mimetype", CrmPropertyType.String)]
        public string MimeType
        {
            get
            {
                return mimeType;
            }
            set
            {
                this.SetProperty(value);
                this.mimeType = value;
            }
        }

        private ApiCustomer customerID;
        [CrmEntityMapper("alt_customerid", CrmPropertyType.EntityReference)]
        public ApiCustomer CustomerID
        {
            get
            {
                return customerID;
            }
            set
            {
                this.SetProperty(value);
                this.customerID = value;
            }
        }

        private bool? publish;
        [CrmEntityMapper("alt_publish", CrmPropertyType.Bool)]
        public bool? Publish
        {
            get
            {
                return publish;
            }
            set
            {
                this.SetProperty(value);
                this.publish = value;
            }
        }

        private ApiEntity regarding;
        [CrmEntityMapper("alt_regardingid", CrmPropertyType.EntityReference)]
        public ApiEntity Regarding
        {
            get
            {
                return regarding;
            }
            set
            {
                this.SetProperty(value);
                this.regarding = value;
            }
        }

        private string bodyBase64;
        public string BodyBase64
        {
            get
            {
                return bodyBase64;
            }
            set
            {
                this.SetProperty(value);
                this.bodyBase64 = value;
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

        private string customerIdentityNumber;

        public string CustomerIdentityNumber
        {
            get
            {
                return customerIdentityNumber;
            }
            set
            {
                this.SetProperty(value);
                this.customerIdentityNumber = value;
            }
        }

        private int documentTypeCode;
        [CrmEntityMapper("alt_documenttypecode", CrmPropertyType.OptionSet)]
        public int DocumentTypeCode
        {
            get
            {
                return documentTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.documentTypeCode = value;
            }
        }

        private int productTypeCode;
        [CrmEntityMapper("alt_producttypecode", CrmPropertyType.OptionSet)]
        public int ProductTypeCode
        {
            get
            {
                return productTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.productTypeCode = value;
            }
        }
    }
}
