using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm
{
    public class CustomerBL : CrmBaseBL
    {
        public CustomerBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public string GetCustomerName(EntityReference customer)
        {
            if (customer.LogicalName.Equals(Contact.EntityLogicalName))
            {
                ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                Contact retrievedContact = contactDal.Get(customer.Id, new[] { Contact.Fields.FullName });
                return retrievedContact.FullName;
            }
            else
            {
                AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                Account retrievedAccount = accountDal.Get(customer.Id, new[] { Account.Fields.Name });
                return retrievedAccount.Name;
            }
        }

        public string GetCustomerPrimaryAttributeName(Entity customer)
        {
            this.GlobalContext.LogEntry();

            return customer.LogicalName == Contact.EntityLogicalName ? (string)customer["fullname"] : (string)customer["name"];
        }

        public Entity GetCustomerByEntityReference(EntityReference customerEntityReference, params string[] select)
        {
            Entity customer = null;
            switch (customerEntityReference.LogicalName)
            {
                case Contact.EntityLogicalName:
                    {
                        ContactDAL contactDal = new ContactDAL(this.GlobalContext);
                        Contact retrievedContact = contactDal.Get(customerEntityReference.Id, select);
                        customer = retrievedContact;
                        break;
                    }
                case Account.EntityLogicalName:
                    {
                        AccountDAL accountDal = new AccountDAL(this.GlobalContext);
                        Account retrievedAccount = accountDal.Get(customerEntityReference.Id, select);
                        customer = retrievedAccount;
                        break;
                    }
                default:
                    break;
            }
            return customer;
        }
    }
}
