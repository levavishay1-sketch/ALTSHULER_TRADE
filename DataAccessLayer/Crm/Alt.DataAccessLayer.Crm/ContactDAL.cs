using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class ContactDAL : CrmBaseDAL<Contact>
    {
        public ContactDAL(GlobalContext globalContext) : base(globalContext, Contact.EntityLogicalName)
        {
        }

        public Contact GetByGovernmentId(string governmentId, string[] columns = null)
        {
            this.GlobalContext.LogEntry();
            return this.GetFirstOrDefaultByAttribute(Contact.Fields.alt_InternalGovernmentId, governmentId.GetPadedLeftZeroString(),
                columns ?? new[] { Contact.Fields.GovernmentId, Contact.Fields.FullName });
        }

        public List<Contact> GetAllContactsFromCrm()
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = Contact.EntityLogicalName,
                ColumnSet = new ColumnSet(Contact.Fields.ContactId, Contact.Fields.GovernmentId),
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression(Contact.Fields.GovernmentId, ConditionOperator.NotNull));
            query.Criteria.AddFilter(filter);

            return this.GetMultipleWithPaging(query);
        }

        public List<Contact> GetContactsWithFirstNameAndEmptyLastNameFromCrm()
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = Contact.EntityLogicalName,
                ColumnSet = new ColumnSet(Contact.Fields.ContactId, Contact.Fields.FirstName, Contact.Fields.LastName),
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.Conditions.Add(new ConditionExpression(Contact.Fields.FirstName, ConditionOperator.NotNull));
            filter.Conditions.Add(new ConditionExpression(Contact.Fields.LastName, ConditionOperator.Null));

            query.Orders.Add(new OrderExpression(Contact.Fields.FirstName, OrderType.Descending));
            query.Criteria.AddFilter(filter);

            return this.GetMultiple(query);
        }
    }
}
