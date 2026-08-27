using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.DataAccessLayer.Crm
{
    public class LeadDAL : CrmBaseDAL<Lead>
    {
        private string[] attributesToRetrieve =
{
            Lead.Fields.StateCode,
            Lead.Fields.StatusCode,
            Lead.Fields.QualifyingOpportunityId,
            Lead.Fields.MobilePhone,
            Lead.Fields.EMailAddress1,
            Lead.Fields.ParentContactId,
            Lead.Fields.ParentAccountId,
            Lead.Fields.alt_LeadIdentityNumber,
            Lead.Fields.alt_MarketingSource,
            Lead.Fields.LeadSourceCode,
            Lead.Fields.alt_ReferralSourceId,
            Lead.Fields.alt_TreatmentStatusId
        };

        public LeadDAL(GlobalContext globalContext) : base(globalContext, Lead.EntityLogicalName)
        {
        }

        public bool IsNeedToDisqualify(Lead targetLead)
        {
            this.GlobalContext.LogEntry();

            var openedLeads = this.GetOpenedLeadsByMobilePhone(targetLead.MobilePhone);
            this.GlobalContext.Log.Info($"Opened Leads Count: {openedLeads?.Count}");
            var qualifiedLeads = this.GetQualifiedLeadsWithOpenedDigitalForm(targetLead.MobilePhone);
            this.GlobalContext.Log.Info($"Qualified Leads Count with Opened Digital Form: {qualifiedLeads.Count}");

            //int leadsCount = openedLeads != null && qualifiedLeads != null
            //     ? openedLeads.Count + qualifiedLeads.Count
            //     : openedLeads != null ? openedLeads.Count : qualifiedLeads.Count;

            int leadsCount = openedLeads.Count + qualifiedLeads.Count;
            this.GlobalContext.Log.Info($"Leads Count: {leadsCount}");

            return leadsCount > 1;
        }

        public List<Lead> GetOpenedLeadsByMobilePhone(string mobilePhone)
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = Lead.EntityLogicalName,
                ColumnSet = new ColumnSet(Lead.Fields.LeadId, Lead.Fields.CreatedOn),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = {
                        new ConditionExpression(Lead.Fields.MobilePhone, ConditionOperator.Equal, mobilePhone),
                        new ConditionExpression(Lead.Fields.StateCode, ConditionOperator.Equal, 0)
                    },

                },
            };
            query.NoLock = true;
            query.AddOrder(Lead.Fields.CreatedOn, OrderType.Ascending);

            return this.GetMultiple(query);
        }

        public List<Lead> GetQualifiedLeadsWithOpenedDigitalForm(string mobilePhone)
        {
            this.GlobalContext.LogEntry();

            var query = new QueryExpression(Lead.EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet(Lead.Fields.LeadId, Lead.Fields.CreatedOn),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(Lead.Fields.MobilePhone, ConditionOperator.Equal, mobilePhone),
                        new ConditionExpression(Lead.Fields.StateCode, ConditionOperator.Equal, 1)
                    }
                },
                Orders =
                {
                    new OrderExpression(Lead.Fields.CreatedOn, OrderType.Ascending)
                },
                LinkEntities =
                {
                    new LinkEntity(Lead.EntityLogicalName, alt_DigitalForm.EntityLogicalName, Lead.Fields.LeadId,alt_DigitalForm.Fields.RegardingObjectId, JoinOperator.Inner)
                    {
                        EntityAlias = "digitalForm",
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression(alt_DigitalForm.Fields.StateCode, ConditionOperator.Equal, 0)
                            }
                        }                      
                    }
                },
                NoLock = true
            };
            return this.GetMultiple(query);
        }

        public Lead GetLeadDetails(Guid id, string[] columns = null)
        {
            this.GlobalContext.LogEntry($"Id : {id}");
            return base.Get(id, columns ?? attributesToRetrieve);
        }

        public QualifyLeadResponse QualifyLead(EntityReference leadEntityReference, OptionSetValue statusCode, bool SuppressDuplicateDetection = false)
        {
            this.GlobalContext.LogEntry();
            QualifyLeadRequest qualifyLeadRequest = new QualifyLeadRequest
            {
                CreateAccount = true,
                CreateContact = true,
                CreateOpportunity = true,
                LeadId = leadEntityReference,
                Status = statusCode
            };

            if (SuppressDuplicateDetection)
            {
                qualifyLeadRequest.Parameters.Add("SuppressDuplicateDetection", SuppressDuplicateDetection);
            }


            QualifyLeadResponse response = (QualifyLeadResponse)this.Execute(qualifyLeadRequest);
            return response;
        }

        public void DisqualifyLead(Lead targetLead, LeadStatusCode statusCode)
        {
            this.GlobalContext.LogEntry();

            this.Update(new Lead
            {
                Id = targetLead.Id,
                StateCode = LeadState.Disqualified,
                StatusCode = new OptionSetValue((int)statusCode)
            });
        }

        public Lead GetFirstOrDefautlLeadByDigitalForm(Guid digitalFormId, string[] columns = null)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = Lead.EntityLogicalName,
                NoLock = true,
                ColumnSet = new ColumnSet(columns ?? attributesToRetrieve),
                LinkEntities =
                    {
                        new LinkEntity
                        {
                            JoinOperator = JoinOperator.Inner,
                            LinkFromEntityName = Lead.EntityLogicalName,
                            LinkFromAttributeName =Lead.Fields.LeadId,
                            LinkToEntityName = alt_DigitalForm.EntityLogicalName,
                            LinkToAttributeName = alt_DigitalForm.Fields.RegardingObjectId,
                            LinkCriteria = new FilterExpression
                            {
                                FilterOperator = LogicalOperator.And,
                                Conditions =
                                {
                                    new ConditionExpression(alt_DigitalForm.Fields.Id, ConditionOperator.Equal, digitalFormId)
                                }
                            },
                        }
            }
            };
            return this.GetFirstOrDefault(query);
        }

        public Lead GetOriginatingLead(Guid opportunityId, ColumnSet columnSet)
        {
            this.GlobalContext.LogEntry(entityLogicalName);
            QueryExpression query = new QueryExpression()
            {
                EntityName = Lead.EntityLogicalName,
                ColumnSet = columnSet,
                Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression(Lead.Fields.QualifyingOpportunityId, ConditionOperator.Equal, opportunityId)
                        }
                    }

            };
            return this.GetFirstOrDefault(query);
        }
    }
}