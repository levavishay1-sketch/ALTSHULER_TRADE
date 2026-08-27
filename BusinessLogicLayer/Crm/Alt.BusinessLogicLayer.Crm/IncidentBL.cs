using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class IncidentBL : CrmBaseBL
    {
        private string Subject2CodesForWithdrawalsParameterName = "Subject2CodesForWithdrawals";

        alt_IncidentStatus incidentStatus { get; set; }
        alt_Subject2 subject2 { get; set; }

        public IncidentBL(GlobalContext globalContext) : base(globalContext) { }

        public void SetIncidentTitle(Incident targetIncident)
        {
            GlobalContext.LogEntry();

            if (targetIncident.Contains(Incident.Fields.alt_Subject1Id)
                && targetIncident.Contains(Incident.Fields.alt_Subject2Id))
            {
                List<string> titleParts = new List<string>();
                SubjectsBL subjectsBl = new SubjectsBL(GlobalContext);

                titleParts.Add(targetIncident.alt_Subject1Id?.Name ?? subjectsBl.GetSubjectName(targetIncident.alt_Subject1Id));
                titleParts.Add(this.GetSubject2(targetIncident.alt_Subject2Id.Id).alt_Name);

                targetIncident.Title = string.Join(" - ", titleParts);
            }
        }

        public void SetResponsibleSystemUser(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();

            Incident mergedIncident = !targetIncident.Equals(preIncident) ? targetIncident.Merge(preIncident) : targetIncident;

            var withdrawalSubject2Codes = this.GlobalContext.CacheManager.GetGlobalParameter<string>(Subject2CodesForWithdrawalsParameterName);
            List<int> codes = withdrawalSubject2Codes?.Split(',').Select(s => int.Parse(s.Trim())).ToList();

            if (mergedIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_PortfolioId))
            {
                Subject2DAL subject2DAL = new Subject2DAL(this.GlobalContext);
                alt_Subject2 retrievedSubject2 = subject2DAL.Get(mergedIncident.alt_Subject2Id.Id, new string[] { alt_Subject2.Fields.alt_CodeInt });

                if (retrievedSubject2 != null && codes.Contains(retrievedSubject2.alt_CodeInt.Value))
                {
                    PortfolioDAL portfolioDAL = new PortfolioDAL(this.GlobalContext);
                    alt_Portfolio retrievedPortfolio = portfolioDAL.Get(mergedIncident.alt_PortfolioId.Id, new string[] { alt_Portfolio.Fields.alt_EncouragingDepositSystemUserId });
                    targetIncident.alt_ResponsibleSystemUserId = retrievedPortfolio.alt_EncouragingDepositSystemUserId;
                }
            }
        }

        public void HandleBpf(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();
            if (targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId))
            {
                Incident mergedIncident = !targetIncident.Equals(preIncident) ?
                        targetIncident.Merge<Incident>(preIncident) : targetIncident;

                string bpfStagesConfiguration;
                if (!string.IsNullOrWhiteSpace(mergedIncident.alt_BpfStagesJson))
                {
                    bpfStagesConfiguration = mergedIncident.alt_BpfStagesJson;
                }
                else
                {
                    var subject2 = this.GetSubject2(mergedIncident.alt_Subject2Id.Id);
                    bpfStagesConfiguration = subject2.alt_IncidentBpfStagesConfiguration;
                }
                if (!string.IsNullOrWhiteSpace(bpfStagesConfiguration))
                {
                    IncidentBusinessProcessFlow incidentBusinessProcessFlow = JsonSerializer.Deserialize<IncidentBusinessProcessFlow>(bpfStagesConfiguration);
                    if (incidentBusinessProcessFlow != null && incidentBusinessProcessFlow.stages.Count > 0)
                    {
                        alt_IncidentStatus incidentStatus = this.GetIncidentStatus(targetIncident.alt_IncidentStatusId.Id);
                        if (incidentStatus.alt_IncidentStatusCode.Value != (int)IncidentStatusCode.Canceled)
                        {
                            incidentBusinessProcessFlow.stages.Where(c => c.isCurrentStep == true)
                              .Select(c => { c.isCurrentStep = false; return c; }).ToList();

                            if (incidentStatus.alt_IncidentBpfStageOrderInt != null)
                            {
                                incidentBusinessProcessFlow.stages.Where(c => c.order == incidentStatus.alt_IncidentBpfStageOrderInt)
                                    .Select(c => { c.isCurrentStep = true; return c; }).ToList();
                            }
                            targetIncident.alt_BpfStagesJson = JsonSerializer.Serialize(incidentBusinessProcessFlow);
                        }
                        else
                        {
                            targetIncident.alt_BpfStagesJson = null;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(mergedIncident.alt_BpfStagesJson))
                    {
                        targetIncident.alt_BpfStagesJson = null;
                    }
                }
            }
        }

        public void ChangeAssigneeWhenAssignedToUser(ParameterCollection inputParameters)
        {
            GlobalContext.LogEntry();

            var target = inputParameters["Target"] as EntityReference;
            var assignee = inputParameters["Assignee"] as EntityReference;

            if (target != null && assignee != null && assignee.LogicalName == SystemUser.EntityLogicalName)
            {
                IncidentDAL incidentDal = new IncidentDAL(GlobalContext);
                Incident retrievedIncident = incidentDal.Get(target.Id, new[] { Incident.Fields.OwnerId });

                if (retrievedIncident != null && retrievedIncident.AttributeHasValue<EntityReference>(Incident.Fields.OwnerId)
                    && retrievedIncident.OwnerId.LogicalName == Team.EntityLogicalName)
                {
                    Incident incidentToUpdate = new Incident()
                    {
                        Id = target.Id,
                        alt_ResponsibleSystemUserId = assignee
                    };

                    incidentDal.Update(incidentToUpdate);
                    inputParameters["Assignee"] = retrievedIncident.OwnerId;
                }
            }
            else
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CantAssignToTeam, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CantAssignToTeam));
            }
        }

        public void HandleChildCasesCancellation(Incident targetIncident, Incident preIncident)
        {
            if (targetIncident.Contains(Incident.Fields.StateCode)
                && targetIncident.StateCode == IncidentState.Cancelled
                && preIncident.alt_BulkIncidentsParentBit.Value)
            {
                IncidentDAL incidentDal = new IncidentDAL(this.GlobalContext);
                var childCases = incidentDal.GetActiveByAttribute(Incident.Fields.ParentCaseId, targetIncident.Id, new[] { Incident.Fields.IncidentId });
                if (childCases != null && childCases.Count > 0)
                {
                    IncidentStatusLogDAL incidentStatusLogDal = new IncidentStatusLogDAL(this.GlobalContext);
                    foreach (var childCase in childCases)
                    {
                        alt_IncidentStatusLog incidentStatusLogToCreate = new alt_IncidentStatusLog()
                        {
                            alt_IncidentId = childCase.ToEntityReference(),
                            alt_ToIncidentStatusId = preIncident.alt_IncidentStatusId,
                            alt_TeamId = preIncident.OwnerId,
                            alt_AssigningTeamId = preIncident.OwnerId
                        };

                        incidentStatusLogDal.Create(incidentStatusLogToCreate);
                    }
                }
            }
        }

        public void DeactivatePreviousIncidentStatusLog(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();

            if (targetIncident.Contains(Incident.Fields.StateCode) && targetIncident.StateCode != preIncident.StateCode
                || (targetIncident.Contains(Incident.Fields.alt_ActiveIncidentStatusLogId)
                && preIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_ActiveIncidentStatusLogId)))
            {
                IncidentStatusLogDAL incidentStatusLogDal = new IncidentStatusLogDAL(GlobalContext);

                incidentStatusLogDal.Update(new alt_IncidentStatusLog
                {
                    Id = preIncident.alt_ActiveIncidentStatusLogId.Id,
                    StateCode = alt_IncidentStatusLogState.Inactive
                });
            }
        }

        public void HandleIncidentStatusLogCreate(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();

            if (targetIncident.AttributeHasValue<OptionSetValue>(Incident.Fields.alt_StatusToChangeCode)
                && targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId)
                    && !targetIncident.Contains(Incident.Fields.alt_ActiveIncidentStatusLogId))
            {
                Incident mergedIncident = targetIncident.Equals(preIncident) ?
                    targetIncident : targetIncident.Merge<Incident>(preIncident);

                EntityReference fromIncidentStatusId = targetIncident.Equals(preIncident) ? null : preIncident.alt_IncidentStatusId;
                if (targetIncident.alt_IncidentStatusId.Id != fromIncidentStatusId?.Id)
                {
                    IncidentStatusLogDAL incidentStatusLogDal = new IncidentStatusLogDAL(GlobalContext);

                    alt_IncidentStatusLog incidentStatusLogToCreate = new alt_IncidentStatusLog()
                    {
                        alt_IncidentId = mergedIncident.ToEntityReference(),
                        alt_ToIncidentStatusId = mergedIncident.alt_IncidentStatusId,
                        alt_TeamId = mergedIncident.OwnerId,
                        alt_FromIncidentStatusId = fromIncidentStatusId,
                        alt_AutoCreateBit = true,
                        alt_CustomerId = mergedIncident.CustomerId,
                        alt_Subject2Id = mergedIncident.alt_Subject2Id
                    };
                    incidentStatusLogDal.Create(incidentStatusLogToCreate);
                }
            }
        }

        public void HandleStatusToChange(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();
            if (targetIncident.AttributeHasValue<OptionSetValue>(Incident.Fields.alt_StatusToChangeCode))
            {
                IncidentState? futureState = GetIncidentStateByStatus(targetIncident.alt_StatusToChangeCode);
                if (futureState != null)
                {
                    IncidentState? previousState = GetIncidentStateByStatus(preIncident?.alt_StatusToChangeCode);
                    this.ChangeIncidentStateAndStatus(targetIncident, preIncident, futureState, previousState);
                }
            }
        }

        public void HandleSubject2AnnotationCreation(Incident targetIncident)
        {
            GlobalContext.LogEntry();
            if (targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_Subject2Id))
            {
                alt_Subject2 retrievedSubject2 = this.GetSubject2(targetIncident.alt_Subject2Id.Id);
                if (retrievedSubject2.AttributeHasValue<string>(alt_Subject2.Fields.alt_AutoAnnotation))
                {
                    AnnotationDAL annotationDal = new AnnotationDAL(GlobalContext);
                    annotationDal.Create(new Annotation()
                    {
                        Subject = "הערה לנושא",
                        NoteText = retrievedSubject2.alt_AutoAnnotation,
                        ObjectId = targetIncident.ToEntityReference(),
                    });
                }
            }
        }

        public void ValidateIncidentOnCreate(Incident targetIncident)
        {
            GlobalContext.LogEntry();
            if (targetIncident.Contains(Incident.Fields.OwnerId) && !targetIncident.OwnerId.LogicalName.Equals(Team.EntityLogicalName))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.OwnerSelectedIsNotTeam, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.OwnerSelectedIsNotTeam), "אירוע"));
            }
            if (!targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "מצב אירוע"));
            }
            if (!targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.CustomerId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "לקוח"));
            }
            if (!targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_Subject1Id))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "נושא 1"));
            }
            if (!targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_Subject2Id))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "נושא 2"));
            }
        }

        public void ValidateIncidentOnUpdate(Incident targetIncident)
        {
            this.GlobalContext.LogEntry();
            if (targetIncident.Contains(Incident.Fields.OwnerId) && !targetIncident.OwnerId.LogicalName.Equals(Team.EntityLogicalName))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.OwnerSelectedIsNotTeam, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.OwnerSelectedIsNotTeam), "אירוע"));
            }
            if (!targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "מצב אירוע"));
            }
            if (targetIncident.Contains(Incident.Fields.CustomerId))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonRequiredFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonRequiredFieldMessage), "לקוח"));
            }
            if (targetIncident.Contains(Incident.Fields.alt_Subject1Id))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonCantUpdateFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonCantUpdateFieldMessage), "נושא 1"));
            }
            if (targetIncident.Contains(Incident.Fields.alt_Subject2Id))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.CommonCantUpdateFieldMessage, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.CommonCantUpdateFieldMessage), "נושא 2"));
            }
        }

        public void HandleIncidentStatusIdChanges(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();

            if (targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId))
            {
                if (targetIncident.Equals(preIncident) //create
                    || (targetIncident.alt_IncidentStatusId.Id != preIncident.alt_IncidentStatusId.Id)
                    || (targetIncident.Contains(Incident.Fields.StatusCode)
                        && targetIncident.StatusCode.Value == (int)IncidentStatusCode.OpenedBySystem))
                {
                    Incident mergedIncident = !targetIncident.Equals(preIncident) ?
                        targetIncident.Merge<Incident>(preIncident) : targetIncident;

                    alt_IncidentStatus retrievedIncidentStatus = this.GetIncidentStatus(mergedIncident.alt_IncidentStatusId.Id);
                    targetIncident.alt_StatusToChangeCode = retrievedIncidentStatus.alt_IncidentStatusCode;
                }
            }
        }

        public void HandleReOpenClosedIncident(Incident targetIncident, Incident preIncident)
        {
            GlobalContext.LogEntry();
            Incident mergedIncident = targetIncident.Merge<Incident>(preIncident);

            if (targetIncident.AttributeHasValue<EntityReference>(Incident.Fields.alt_IncidentStatusId)
              && mergedIncident.StateCode != IncidentState.Active)
            {
                targetIncident.StateCode = IncidentState.Active;
                targetIncident.StatusCode = new OptionSetValue((int)IncidentStatusCode.OpenedBySystem);
            }
        }

        public void HandleAutoIncidentCreation(Incident targetIncident)
        {
            GlobalContext.LogEntry();

            if (targetIncident.Contains(Incident.Fields.alt_AutomaticIncidentTemplateKey) && !string.IsNullOrWhiteSpace(targetIncident.alt_AutomaticIncidentTemplateKey))
            {
                AutomaticIncidentTemplateDAL automaticIncidentTemplateDAL = new AutomaticIncidentTemplateDAL(this.GlobalContext);
                var automaticIncidentTemplate = automaticIncidentTemplateDAL.GetFirstActivetOrDefaultByAttribute(alt_AutomaticIncidentTemplate.Fields.alt_Key, targetIncident.alt_AutomaticIncidentTemplateKey, null);

                if (automaticIncidentTemplate != null)
                {
                    this.MappTemplateToIncident(targetIncident, automaticIncidentTemplate);
                }
                else
                {
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.AutomaticIncidentTemplateNotFound, string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.AutomaticIncidentTemplateNotFound)));
                }
            }
        }

        public void MappTemplateToIncident(Incident incident, alt_AutomaticIncidentTemplate automaticIncidentTemplate)
        {
            this.GlobalContext.LogEntry();

            HashSet<string> attributesNotToMap = new HashSet<string>
            {
                alt_AutomaticIncidentTemplate.Fields.alt_AutomaticIncidentTemplateId,
                alt_AutomaticIncidentTemplate.Fields.alt_Key,
                alt_AutomaticIncidentTemplate.Fields.alt_Name,
                alt_AutomaticIncidentTemplate.Fields.alt_Description
            };

            Dictionary<string, string> attributesToReplate = new Dictionary<string, string>
            {
               {alt_AutomaticIncidentTemplate.Fields.alt_OwnerId,Incident.Fields.OwnerId },
                {alt_AutomaticIncidentTemplate.Fields.alt_CaseOriginCode,Incident.Fields.CaseOriginCode}
            };

            foreach (var attribute in automaticIncidentTemplate.Attributes)
            {
                string attributeName = attribute.Key;
                if (attributeName.StartsWith("alt_") && !attributesNotToMap.Contains(attributeName))
                {
                    if (attributesToReplate.ContainsKey(attributeName))
                    {
                        attributeName = attributesToReplate[attributeName];
                    }
                    incident[attributeName] = attribute.Value;
                }
            }
        }

        private void ChangeIncidentStateAndStatus(Incident targetIncident, Incident preIncident, IncidentState? futureState, IncidentState? previousState)
        {
            this.GlobalContext.LogEntry($"{(previousState != null ? $"Previous state: {previousState}" : "")} ►►► Future state: {futureState}");

            IncidentDAL incidentDal = new IncidentDAL(GlobalContext);

            switch (futureState.Value)
            {
                case IncidentState.Resolved:
                    {
                        Incident mergedIncident = targetIncident.Merge<Incident>(preIncident);
                        EntityReference owner = mergedIncident?.OwnerId ?? new EntityReference(SystemUser.EntityLogicalName, this.GlobalContext.InitiatingUserId);
                        incidentDal.ResolveIncident(mergedIncident, owner);
                        break;
                    }
                case IncidentState.Active:
                case IncidentState.Cancelled:
                    {
                        if (preIncident == null
                            || targetIncident.alt_StatusToChangeCode.Value != preIncident.alt_StatusToChangeCode?.Value
                            || (targetIncident.Contains(Incident.Fields.StatusCode)
                                && targetIncident.StatusCode?.Value == (int)IncidentStatusCode.OpenedBySystem))
                        {
                            Incident incidentToUpdate = new Incident()
                            {
                                Id = targetIncident.Id,
                                StatusCode = new OptionSetValue((int)targetIncident.alt_StatusToChangeCode.Value),
                            };
                            if (previousState != futureState)
                            {
                                incidentToUpdate.StateCode = futureState;
                            }
                            incidentDal.Update(incidentToUpdate);
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        public void ChangeIncidentStatusFromAction(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();

            EntityReference incidentReference = (EntityReference)inputParameters["Target"];
            Guid incidentId = incidentReference.Id;
            alt_IncidentStatusLog incidentStatusLogToCreate = this.GenerateIncidentStatusLogFromInputParams(inputParameters);

            if (incidentStatusLogToCreate.alt_ToIncidentStatusId != null
                && incidentStatusLogToCreate.alt_TeamId != null)
            {
                incidentStatusLogToCreate.alt_IncidentId = new EntityReference(Incident.EntityLogicalName, incidentId);

                IncidentStatusLogDAL incidentDal = new IncidentStatusLogDAL(this.GlobalContext);
                incidentDal.Create(incidentStatusLogToCreate);
            }
            else
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.InternalServerError, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InternalServerError));
            }
        }

        private IncidentState? GetIncidentStateByStatus(OptionSetValue statusCode)
        {
            GlobalContext.LogEntry();

            switch (statusCode?.Value)
            {
                case (int)IncidentStatusCode.OnGoing:
                case (int)IncidentStatusCode.Holding:
                case (int)IncidentStatusCode.WaitingForDetails:
                case (int)IncidentStatusCode.Checking:
                    {
                        return IncidentState.Active;
                    }
                case (int)IncidentStatusCode.Solved:
                case (int)IncidentStatusCode.InformationProvided:
                    {
                        return IncidentState.Resolved;
                    }
                case (int)IncidentStatusCode.Canceled:
                case (int)IncidentStatusCode.Merged:
                    {
                        return IncidentState.Cancelled;
                    }
                default:
                    return null;
            }
        }

        private alt_IncidentStatusLog GenerateIncidentStatusLogFromInputParams(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();
            alt_IncidentStatusLog incidentStatusLog = new alt_IncidentStatusLog();

            if (inputParameters.ContainsKey("ToIncidentStatusId") && Guid.TryParse(inputParameters["ToIncidentStatusId"]?.ToString(), out Guid toIncidentStatusId))
            {
                incidentStatusLog.alt_ToIncidentStatusId = new EntityReference(alt_IncidentStatus.EntityLogicalName, toIncidentStatusId);
            }
            else if (inputParameters.ContainsKey("ToIncidentStatusCode") && int.TryParse(inputParameters["ToIncidentStatusCode"]?.ToString(), out int toIncidentStatusCode))
            {
                incidentStatusLog.alt_ToIncidentStatusId = new EntityReference(alt_IncidentStatus.EntityLogicalName, "alt_codeint", toIncidentStatusCode);
            }

            if (inputParameters.ContainsKey("FromTeamId") && Guid.TryParse(inputParameters["FromTeamId"]?.ToString(), out Guid fromTeamId))
            {
                incidentStatusLog.alt_AssigningTeamId = new EntityReference(Team.EntityLogicalName, fromTeamId);
            }
            else if (inputParameters.ContainsKey("FromTeamCode") && int.TryParse(inputParameters["FromTeamCode"]?.ToString(), out int fromTeamCode))
            {
                incidentStatusLog.alt_AssigningTeamId = new EntityReference(Team.EntityLogicalName, "alt_codeint", fromTeamCode);
            }

            if (inputParameters.ContainsKey("ToTeamId") && Guid.TryParse(inputParameters["ToTeamId"]?.ToString(), out Guid toTeamId))
            {
                incidentStatusLog.alt_TeamId = new EntityReference(Team.EntityLogicalName, toTeamId);
            }
            else if (inputParameters.ContainsKey("ToTeamCode") && int.TryParse(inputParameters["ToTeamCode"]?.ToString(), out int toTeamCode))
            {
                incidentStatusLog.alt_TeamId = new EntityReference(Team.EntityLogicalName, "alt_codeint", toTeamCode);
            }
            return incidentStatusLog;
        }

        private alt_IncidentStatus GetIncidentStatus(Guid incidentStatusId)
        {
            this.GlobalContext.LogEntry();

            if (this.incidentStatus == null)
            {
                IncidentStatusDAL incidentStatusDal = new IncidentStatusDAL(this.GlobalContext);
                this.incidentStatus = incidentStatusDal.Get(incidentStatusId, new[]
                {
                    alt_IncidentStatus.Fields.alt_Name,
                    alt_IncidentStatus.Fields.alt_IncidentBpfStageOrderInt,
                    alt_IncidentStatus.Fields.alt_IncidentStatusCode,
                    alt_IncidentStatus.Fields.alt_CodeInt
                });
            }
            return this.incidentStatus;
        }

        private alt_Subject2 GetSubject2(Guid subject2Id)
        {
            this.GlobalContext.LogEntry();

            if (this.subject2 == null)
            {
                CommonDAL subject2Dal = new CommonDAL(this.GlobalContext, alt_Subject2.EntityLogicalName);
                this.subject2 = subject2Dal.Get(subject2Id, new[]
                {
                    alt_Subject2.Fields.alt_Name,
                    alt_Subject2.Fields.alt_AutoAnnotation,
                    alt_Subject2.Fields.alt_IncidentBpfStagesConfiguration
                }).ToEntity<alt_Subject2>();
            }
            return this.subject2;
        }
    }
}
