using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiCustomerOperationRequest : ApiEntity
    {
        public const string EntityLogicalName = "alt_customeroperationrequest";
        public ApiCustomerOperationRequest() : base(EntityLogicalName) { }

        private bool? deleteIfSuccessfulBit;
        /// <summary>
        /// האם למחוק בהצלחה
        /// </summary>
        [CrmEntityMapper("alt_deleteifsuccessfulbit", CrmPropertyType.Bool)]
        public bool? DeleteIfSuccessfulBit
        {
            get
            {
                return this.deleteIfSuccessfulBit;
            }
            set
            {
                this.SetProperty(value);
                this.deleteIfSuccessfulBit = value;
            }
        }

        private string sendResult;
        /// <summary>
        /// תוצאת ריצה
        /// </summary>
        [CrmEntityMapper("alt_sendresult", CrmPropertyType.String)]
        public string SendResult
        {
            get
            {
                return this.sendResult;
            }
            set
            {
                this.SetProperty(value);
                this.sendResult = value;
            }
        }

        private ApiEntity relatedRecordId;
        /// <summary>
        /// קשור
        /// </summary>
        [CrmEntityMapper("alt_relatedrecordid", CrmPropertyType.EntityReference)]
        public ApiEntity RelatedRecordId
        {
            get
            {
                return relatedRecordId;
            }
            set
            {
                this.SetProperty(value);
                this.relatedRecordId = value;
            }
        }

        /// <summary>
        /// קוד תבנית הפקת PDF
        /// </summary>
        private int? pdfProductionTemplateCode;
        [CrmEntityMapper("alt_pdfproductiontemplatecode", CrmPropertyType.Int)]
        public int? PDFProductionTemplateCode
        {
            get
            {
                return pdfProductionTemplateCode;
            }
            set
            {
                this.SetProperty(value);
                this.pdfProductionTemplateCode = value;
            }
        }

        private ApiCustomerOperationTemplate customerOperationTemplateId;
        [CrmEntityMapper("alt_customeroperationtemplateid", CrmPropertyType.EntityReference)]
        public ApiCustomerOperationTemplate CustomerOperationTemplateId 
        { 
            get => customerOperationTemplateId;
            set
            {
                this.SetProperty(value);
                this.customerOperationTemplateId = value;
            }
        }


        private int? customerOperationTemplateCode;
        [CrmEntityMapper("alt_customeroperationtemplatecodeint", CrmPropertyType.Int)]
        public int? CustomerOperationTemplateCode
        {
            get => customerOperationTemplateCode;
            set
            {
                this.SetProperty(value);
                this.customerOperationTemplateCode = value;
            }
        }
    }
}
