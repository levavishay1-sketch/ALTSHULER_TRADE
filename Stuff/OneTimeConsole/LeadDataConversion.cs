using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using OneTimeConsole.DataAccessLayer;
using OneTimeConsole.Enums;
using System;
using System.Collections.Generic;

namespace OneTimeConsole
{
    public class LeadDataConversion : OperationBase
    {
        public LeadDataConversion(GlobalContext globalContext, OperationCode operationCode)
            : base(globalContext, operationCode) { }

        public override void Run()
        {
            this.GlobalContext.LogEntry();
            switch (this.OperationCode)
            {
                case OperationCode.InactiveLeadsClosedOnDate:
                    {
                        this.HandleInactiveLeadsClosedOnDateLogic();
                        break;
                    }
                default:
                    break;
            }
        }

        private void HandleInactiveLeadsClosedOnDateLogic()
        {
            this.GlobalContext.LogEntry();

            List<Lead> leadsToUpdate = new List<Lead>();

            CommonDAL commonDAL = new CommonDAL(this.GlobalContext, Lead.EntityLogicalName);
            LeadDAL leadDAL = new LeadDAL(this.GlobalContext);
            List<Lead> retrievedInactiveLeads = leadDAL.RetrieveInactiveLeadsWithEmptyClosedOnDate();

            if (retrievedInactiveLeads?.Count > 0)
            {
                foreach (Lead retrievedLead in retrievedInactiveLeads)
                {
                    leadsToUpdate.Add(new Lead()
                    {
                        Id = retrievedLead.Id,
                        alt_ClosedOnDate = retrievedLead.ModifiedOn,
                    });
                }
            }

            if (leadsToUpdate.Count > 0)
            {
                commonDAL.ExecuteMultipleRequestsInChunks(leadsToUpdate, RequestType.Update);
            }
        }
    }
}
