using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Extensions;
using Alt.Framework.Mapper;
using System;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiCustomer : ApiEntity
    {
        private int? customerTypeCode;
        public ApiCustomer() : base(null)
        {
        }

        public ApiCustomer(string logicalName) : base(logicalName)
        {
            if (logicalName?.ToLower() != ApiContact.EntityLogicalName && logicalName?.ToLower() != ApiAccount.EntityLogicalName)
            {
                base.LogicalName = null;
            }
        }

        private string customerName;
        [StringLength(100)]
        public string CustomerName
        {
            get { return customerName; }
            set
            {
                this.SetProperty(value);
                customerName = value;
            }
        }

        [CrmEntityMapper(null, CrmPropertyType.Int)]
        [Range(1, 2)]
        public int? CustomerTypeCode
        {
            get
            {
                return this.customerTypeCode;
            }
            set
            {
                this.SetProperty(value);
                this.customerTypeCode = value;
                if (this.customerTypeCode != null && string.IsNullOrWhiteSpace(LogicalName))
                {
                    base.LogicalName = (CustomerTypeCode)this.customerTypeCode == Core.Enums.CustomerTypeCode.Contact ?
                        ApiContact.EntityLogicalName : ApiAccount.EntityLogicalName;
                }
                this.SetInternalIdentifier(this.customerIdentity.GetPadedLeftZeroString());
            }
        }     
     
        private string telephone2;
        [CrmEntityMapper("telephone2", CrmPropertyType.String)]
        public string Telephone2
        {
            get
            {
                return this.telephone2;
            }
            set
            {
                this.SetProperty(value);
                this.telephone2 = value;
            }
        }

        private string telephone;
        [CrmEntityMapper("telephone1", CrmPropertyType.String)]
        public string Telephone
        {
            get
            {
                return telephone;
            }
            set
            {
                this.SetProperty(value);
                this.telephone = value;
            }
        } 

        public virtual ApiCustomer ConvetToCustomer()
        {
            throw new NotImplementedException();
        }

        private string customerIdentity;
        public string CustomerIdentity
        {
            get
            {
                return this.customerIdentity;
            }
            set
            {
                this.SetInternalIdentifier(value.GetPadedLeftZeroString());
                this.SetProperty(value);
                this.customerIdentity = value;
            }
        }

        private void SetInternalIdentifier(string identifierValue)
        {
            if (!string.IsNullOrWhiteSpace(identifierValue) && !(this is ApiContact || this is ApiAccount))
            {
                string value = identifierValue;
                if (this.customerTypeCode != null)
                {
                    if ((CustomerTypeCode)this.customerTypeCode == Core.Enums.CustomerTypeCode.Contact)
                    {
                        this.SetEntityKeys("alt_internalgovernmentid", value);
                    }
                    else
                    {
                        this.SetEntityKeys("alt_internalaccountnumber", value);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(this.LogicalName))
                {
                    if (this.LogicalName == ApiContact.EntityLogicalName)
                    {
                        this.SetEntityKeys("alt_internalgovernmentid", value);
                    }
                    else
                    {
                        this.SetEntityKeys("alt_internalaccountnumber", value);
                    }
                }
            }
        }
    }
}
