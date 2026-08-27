using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace Alt.BusinessLogicLayer.Crm
{
    public class RepresentativeRewardBL : CrmBaseBL
    {
        public RepresentativeRewardBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void SetName(alt_RepresentativeReward targetRepresentativeReward, alt_RepresentativeReward preRepresentativeReward = null)
        {
            this.GlobalContext.LogEntry();
            if (targetRepresentativeReward.alt_RepresentativeRewardSystemUserId != null)
            {
                List<string> nameParts = new List<string>();
                nameParts.Add(targetRepresentativeReward.alt_RepresentativeRewardSystemUserId.Name
                    ?? new CommonDAL(this.GlobalContext, SystemUser.EntityLogicalName)
                    .GetPrimeryAttributeValue(targetRepresentativeReward.alt_RepresentativeRewardSystemUserId, SystemUser.Fields.FullName));
                var mergedReprsentativeReward = preRepresentativeReward == null ? targetRepresentativeReward : targetRepresentativeReward.Merge(preRepresentativeReward);
                nameParts.Add(this.GetEntityNameByRelatedRecord(mergedReprsentativeReward.alt_RelatedRecordId));
                targetRepresentativeReward.alt_Name = string.Join(" - ", nameParts);
            }
        }

        private string GetEntityNameByRelatedRecord(EntityReference alt_RelatedRecordId)
        {
            this.GlobalContext.LogEntry();

            string result = null;
            if (alt_RelatedRecordId != null)
            {
                switch (alt_RelatedRecordId.LogicalName)
                {
                    case Lead.EntityLogicalName:
                        {
                            result = "הפניה";
                            break;
                        }
                    case Opportunity.EntityLogicalName:
                        {
                            result = "הזדמנות";
                            break;
                        }
                    case alt_DigitalFormVerification.EntityLogicalName:
                        {
                            result = "בקרת טופס הצטרפות";
                            break;
                        }
                    default:
                        break;
                }
            }
            return result;
        }

        public void SetRepresentativeRewardTypeCode(alt_RepresentativeReward targetRepresentativeReward)
        {
            this.GlobalContext.LogEntry();
            if (targetRepresentativeReward.alt_RepresentativeRewardTypeCode == null
                && targetRepresentativeReward.alt_RelatedRecordId != null)
            {
                targetRepresentativeReward.alt_RepresentativeRewardTypeCode = targetRepresentativeReward.alt_RelatedRecordId.LogicalName == Lead.EntityLogicalName
                    || targetRepresentativeReward.alt_RelatedRecordId.LogicalName == Opportunity.EntityLogicalName ?
                    new OptionSetValue((int)RepresentativeRewardTypeCode.SalesProcess) :
                    new OptionSetValue((int)RepresentativeRewardTypeCode.EncouragingDeposit);
            }
        }

        public void ClosePreviousRepresentativeReward(alt_RepresentativeReward targetRepresentativeReward)
        {
            this.GlobalContext.LogEntry();

            if (targetRepresentativeReward.alt_RelatedRecordId != null)
            {
                RepresentativeRewardDAL representativeRewardsDal = new RepresentativeRewardDAL(this.GlobalContext);
                var retrievedRepresentativeRewards = representativeRewardsDal.GetActiveByAttribute(alt_RepresentativeReward.Fields.alt_RelatedRecordId,
                        targetRepresentativeReward.alt_RelatedRecordId.Id,
                        new string[] { alt_RepresentativeReward.Fields.alt_RepresentativeRewardId }).ToList();
                if (retrievedRepresentativeRewards.Count > 0)
                {
                    foreach (var representativeReward in retrievedRepresentativeRewards)
                    {
                        if (representativeReward.Id != targetRepresentativeReward.Id)
                        {
                            var entityToUpdate = new alt_RepresentativeReward() { Id = representativeReward.Id };
                            entityToUpdate.Attributes.Add(alt_RepresentativeReward.Fields.StateCode, new OptionSetValue((int)CustomStateCode.Inactive));
                            representativeRewardsDal.Update(entityToUpdate);
                        }
                    }
                }
            }
        }

        internal void CreateRepresentativeReward(Entity relatedEntity, EntityReference representative = null)
        {
            this.GlobalContext.LogEntry();

            var relatedRecord = relatedEntity.ToEntityReference();
            var representativeId = representative ?? relatedEntity.GetAttributeValue<EntityReference>(alt_RepresentativeReward.Fields.OwnerId);
            RepresentativeRewardDAL representativeRewardDal = new RepresentativeRewardDAL(this.GlobalContext);
            var activeRepresentativeReward = representativeRewardDal.GetActiveByRepresentativeAndRelatedRecord(representativeId, relatedRecord);
            if (activeRepresentativeReward == null)
            {
                EntityReference defaultTeam = new TeamDAL(GlobalContext).GetTeamByCodeWithCache().ToEntityReference();
                var representativeRewardToCreate = new alt_RepresentativeReward()
                {
                    alt_RepresentativeRewardSystemUserId = representativeId,
                    alt_CreationMethodCode = new OptionSetValue((int)CreationMethodCode.Proccess),
                    alt_RelatedRecordId = relatedRecord,
                    OwnerId = defaultTeam
                };
                this.SetRelatedEntityValues(representativeRewardToCreate, relatedEntity);
                representativeRewardDal.Create(representativeRewardToCreate);
            }
        }

        private void SetRelatedEntityValues(Entity representativeRewardToCreate, Entity relatedEntity)
        {
            this.GlobalContext.LogEntry();

            string joiningProcessNumber = null;
            EntityReference treatmentStatus = null;
            switch (relatedEntity.LogicalName)
            {
                case Lead.EntityLogicalName:
                    {
                        joiningProcessNumber = relatedEntity.GetAttributeValue<string>(Lead.Fields.alt_LeadIdentityNumber);
                        treatmentStatus = relatedEntity.GetAttributeValue<EntityReference>(Lead.Fields.alt_TreatmentStatusId);
                        break;
                    }
                case Opportunity.EntityLogicalName:
                    {
                        joiningProcessNumber = relatedEntity.GetAttributeValue<string>(Opportunity.Fields.alt_OpportunityIdentityNumber);
                        treatmentStatus = relatedEntity.GetAttributeValue<EntityReference>(Opportunity.Fields.alt_TreatmentStatusId);
                        break;
                    }
                case alt_DigitalFormVerification.EntityLogicalName:
                    {
                        joiningProcessNumber = relatedEntity.GetAttributeValue<string>(alt_DigitalFormVerification.Fields.alt_DigitalFormNumber);
                        EntityReference opportunityId = relatedEntity.GetAttributeValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_OpportunityId);
                        if (opportunityId != null)
                        {
                            Opportunity retrievedOpportunity = new OpportunityDAL(this.GlobalContext).Get(opportunityId.Id, new string[] { Opportunity.Fields.alt_TreatmentStatusId });
                            treatmentStatus = retrievedOpportunity.alt_TreatmentStatusId;
                            representativeRewardToCreate.Attributes.Add(alt_RepresentativeReward.Fields.alt_PortfolioId, relatedEntity.GetAttributeValue<EntityReference>(alt_DigitalFormVerification.Fields.alt_PortfolioId));
                        }
                        break;
                    }
                default:
                    break;
            }
            representativeRewardToCreate.Attributes.Add(alt_RepresentativeReward.Fields.alt_JoiningProcessNumber, joiningProcessNumber);
            representativeRewardToCreate.Attributes.Add(alt_RepresentativeReward.Fields.alt_TreatmentStatusId, treatmentStatus);
        }
    }
}
