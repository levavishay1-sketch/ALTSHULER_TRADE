using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm
{
    public class DigitalFormDAL : CrmBaseDAL<alt_DigitalForm>
    {
        string[] attributesToRetrieve =
        {
            alt_DigitalForm.Fields.StateCode,
            alt_DigitalForm.Fields.StatusCode,
            alt_DigitalForm.Fields.alt_DigitalFormLink,
            alt_DigitalForm.Fields.alt_DigitalFormStatusId,
            alt_DigitalForm.Fields.alt_DigitalFormTypeCode,
            alt_DigitalForm.Fields.Subject,
            alt_DigitalForm.Fields.alt_TransferToOutSystemStatusCode
        };
        public DigitalFormDAL(GlobalContext globalContext) : base(globalContext, alt_DigitalForm.EntityLogicalName)
        {
        }

        public List<alt_DigitalForm> GetDigitalFormsByRegardingObject(Guid regardingObjectId, string[] columns = null)
        {
            return base.GetByAttribute(alt_DigitalForm.Fields.RegardingObjectId, regardingObjectId, columns ?? attributesToRetrieve);
        }

        public alt_DigitalForm GetAnotherActiveDigitalFormByLink(Guid currentDigitalFormId, string link)
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = alt_DigitalForm.EntityLogicalName,
                ColumnSet = new ColumnSet(alt_DigitalForm.Fields.Id),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = {
                        new ConditionExpression(alt_DigitalForm.Fields.alt_DigitalFormLink, ConditionOperator.Equal, link),
                        new ConditionExpression(alt_DigitalForm.Fields.StateCode, ConditionOperator.Equal, 0),
                        new ConditionExpression(alt_DigitalForm.Fields.Id, ConditionOperator.NotEqual, currentDigitalFormId)
                    },

                },
            };
            query.NoLock = true;

            return this.GetMultiple(query).FirstOrDefault();
        }
    }
}
