using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm
{
    public class IncidentStatusLogBL : CrmBaseBL
    {
        private string incidentStatusCodesForPreservationParameterName = "IncidentStatusCodesForPreservation";

        public IncidentStatusLogBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void ValidateStatusLog(alt_IncidentStatusLog targetIncidentStatusLog)
        {
            this.GlobalContext.LogEntry();
            if (!targetIncidentStatusLog.AttributeHasValue<EntityReference>(alt_IncidentStatusLog.Fields.alt_IncidentId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "אירוע"));
            }
            else if (!targetIncidentStatusLog.AttributeHasValue<EntityReference>(alt_IncidentStatusLog.Fields.alt_ToIncidentStatusId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "מצב אירוע"));
            }
            else if (!targetIncidentStatusLog.AttributeHasValue<EntityReference>(alt_IncidentStatusLog.Fields.alt_TeamId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "צוות מקבל"));
            }
            else
            {
                var incidentStatusCodesForPreservation = this.GlobalContext.CacheManager.GetGlobalParameter<string>(incidentStatusCodesForPreservationParameterName);
                List<int> codes = incidentStatusCodesForPreservation?.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                IncidentStatusDAL incidentStatusDAL = new IncidentStatusDAL(this.GlobalContext);
                alt_IncidentStatus retrievedIncidentStatus =
                    incidentStatusDAL.Get(targetIncidentStatusLog.alt_ToIncidentStatusId.Id, new string[] { alt_IncidentStatus.Fields.alt_CodeInt });

                if (codes.Contains(retrievedIncidentStatus.alt_CodeInt.Value))
                {
                    IncidentDAL incidentDAL = new IncidentDAL(this.GlobalContext);
                    Incident retrievedIncident = incidentDAL.Get(targetIncidentStatusLog.alt_IncidentId.Id, new string[] { Incident.Fields.alt_PreservationStatusCode });
                    if (!retrievedIncident.AttributeHasValue<OptionSetValue>(Incident.Fields.alt_PreservationStatusCode))
                    {
                        throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.PreservationStatusCodeRequired, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.PreservationStatusCodeRequired));
                    }
                }
            }
        }

        public void HandleCreateSmsAndEmails(alt_IncidentStatusLog targetIncidentStatusLog)
        {
            this.GlobalContext.LogEntry();

            if (targetIncidentStatusLog.AttributeHasValue<EntityReference>(alt_IncidentStatusLog.Fields.alt_CustomerId)
                && targetIncidentStatusLog.alt_CustomerId.LogicalName == Contact.EntityLogicalName)
            {

                IncidentStatusDAL incidentStatusDAL = new IncidentStatusDAL(this.GlobalContext);
                alt_IncidentStatus incidentStatus = incidentStatusDAL.Get(targetIncidentStatusLog.alt_ToIncidentStatusId.Id, new[]
                  {
                 alt_IncidentStatus.Fields.alt_SMSTemplateId,
                 alt_IncidentStatus.Fields.alt_EmailTemplateId });

                if (incidentStatus.AttributeHasValue<EntityReference>(alt_IncidentStatus.Fields.alt_SMSTemplateId))
                {
                    try
                    {
                        SmsBL smsBL = new SmsBL(this.GlobalContext);
                        smsBL.CreateSms(targetIncidentStatusLog.alt_IncidentId, targetIncidentStatusLog.alt_CustomerId, incidentStatus.GetAttributeValue<EntityReference>(alt_IncidentStatus.Fields.alt_SMSTemplateId));
                    }
                    catch (Exception ex)
                    {
                        this.GlobalContext.Log.Error(ex);
                    }
                }
                if (incidentStatus.AttributeHasValue<EntityReference>(alt_IncidentStatus.Fields.alt_EmailTemplateId))
                {
                    try
                    {
                        EmailBL emailBL = new EmailBL(this.GlobalContext);
                        emailBL.CreateEmail(targetIncidentStatusLog.alt_IncidentId, targetIncidentStatusLog.alt_CustomerId, incidentStatus.GetAttributeValue<EntityReference>(alt_IncidentStatus.Fields.alt_EmailTemplateId));
                    }
                    catch (Exception ex)
                    {
                        this.GlobalContext.Log.Error(ex);
                    }
                }
            }
        }

        public void HandleRelatedIncidentUpdate(alt_IncidentStatusLog targetIncidentStatusLog)
        {
            this.GlobalContext.LogEntry();
            EntityReference incidentStatusId = !targetIncidentStatusLog.alt_AutoCreateBit.Value ?
                targetIncidentStatusLog.alt_ToIncidentStatusId : null;

            IncidentDAL incidentDal = new IncidentDAL(this.GlobalContext);
            Incident retrievedIncident = incidentDal.Get(targetIncidentStatusLog.alt_IncidentId.Id, new[]
            {
               Incident.Fields.OwnerId
            });
            Incident incidentToUpdate = new Incident()
            {
                Id = targetIncidentStatusLog.alt_IncidentId.Id,
                alt_ActiveIncidentStatusLogId = targetIncidentStatusLog.ToEntityReference()
            };
            if (incidentStatusId != null)
            {
                incidentToUpdate.alt_IncidentStatusId = incidentStatusId;
            }
            if (!targetIncidentStatusLog.alt_AutoCreateBit.Value
                && targetIncidentStatusLog.AttributeHasValue<EntityReference>(alt_IncidentStatusLog.Fields.alt_TeamId))
            {
                if (targetIncidentStatusLog.alt_TeamId.Id != retrievedIncident.OwnerId?.Id)
                {
                    incidentToUpdate.OwnerId = targetIncidentStatusLog.alt_TeamId;
                }
                //incidentToUpdate.alt_ResponsibleSystemUserId = null;
            }

            incidentDal.Update(incidentToUpdate);
        }

        public void SetIncidentStatusLogTitle(alt_IncidentStatusLog targetIncidentStatusLog)
        {
            this.GlobalContext.LogEntry();
            IncidentStatusDAL incidentStatusDal = new IncidentStatusDAL(this.GlobalContext);
            string toIncidentStatusName = targetIncidentStatusLog.alt_ToIncidentStatusId.Name;
            if (String.IsNullOrWhiteSpace(toIncidentStatusName))
            {
                alt_IncidentStatus retrievedIncidentStatus = incidentStatusDal.Get(targetIncidentStatusLog.alt_ToIncidentStatusId.Id, new[] { alt_IncidentStatus.Fields.alt_Name });
                toIncidentStatusName = retrievedIncidentStatus.alt_Name;
            }

            if (targetIncidentStatusLog.alt_AutoCreateBit.Value)
            {
                targetIncidentStatusLog.alt_Name = $"נוצר אוטומטי - מצב אירוע: {toIncidentStatusName}";
            }
            else
            {
                string fromIncidentStatusName = targetIncidentStatusLog.alt_FromIncidentStatusId.Name;
                if (string.IsNullOrWhiteSpace(fromIncidentStatusName))
                {
                    alt_IncidentStatus retrievedIncidentStatus = incidentStatusDal.Get(targetIncidentStatusLog.alt_FromIncidentStatusId.Id, new[] { alt_IncidentStatus.Fields.alt_Name });
                    fromIncidentStatusName = retrievedIncidentStatus.alt_Name;
                }
                targetIncidentStatusLog.alt_Name = $"שינוי מ\"{fromIncidentStatusName}\" ל\"{toIncidentStatusName}\"";
            }
        }

        public void MapFieldsFromIncident(alt_IncidentStatusLog targetIncidentStatusLog)
        {
            this.GlobalContext.LogEntry();
            if (!targetIncidentStatusLog.Contains(alt_IncidentStatusLog.Fields.alt_CustomerId)
                && !targetIncidentStatusLog.Contains(alt_IncidentStatusLog.Fields.alt_Subject2Id)
                    && !targetIncidentStatusLog.Contains(alt_IncidentStatusLog.Fields.alt_FromIncidentStatusId))
            {
                IncidentDAL incidentDal = new IncidentDAL(this.GlobalContext);
                Incident retrievedIncident = incidentDal.Get(targetIncidentStatusLog.alt_IncidentId.Id, new[]
                {
                   Incident.Fields.alt_Subject2Id,
                   Incident.Fields.CustomerId,
                   Incident.Fields.alt_IncidentStatusId
                });

                if (retrievedIncident.AttributeHasValue<EntityReference>(Incident.Fields.CustomerId))
                {
                    targetIncidentStatusLog.alt_CustomerId = retrievedIncident.CustomerId;
                }

                if (retrievedIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_Subject2Id))
                {
                    targetIncidentStatusLog.alt_Subject2Id = retrievedIncident.alt_Subject2Id;
                }

                if (retrievedIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId))
                {
                    targetIncidentStatusLog.alt_FromIncidentStatusId = retrievedIncident.alt_IncidentStatusId;
                }
            }
        }

        public void HandleOwner(alt_IncidentStatusLog targetIncidentStatusLog)
        {
            this.GlobalContext.LogEntry();
            if (targetIncidentStatusLog.AttributeHasValue<EntityReference>(alt_IncidentStatusLog.Fields.alt_TeamId))
            {
                targetIncidentStatusLog.OwnerId = targetIncidentStatusLog.alt_TeamId;
            }
        }
    }
}
