using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Alt.BusinessLogicLayer.Crm
{
    public class AuthorizationManagementBL : CrmBaseBL
    {
        public AuthorizationManagementBL(GlobalContext globalContext) : base(globalContext) { }



        public void HandleManagerApprovalChangeTracking(
     alt_AuthorizationManagement targetAuthorizationManagement,
     alt_AuthorizationManagement preAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();

            if (!targetAuthorizationManagement.AttributeHasValue<OptionSetValue>(
                alt_AuthorizationManagement.Fields.alt_ControlStageStatusCode))
            {
                return;
            }

            if (targetAuthorizationManagement.alt_ControlStageStatusCode.Value !=
                (int)ControlStageStatusCode.Approval)
            {
                return;
            }

            alt_AuthorizationManagement mergedAuthorizationManagement =
                targetAuthorizationManagement.Merge(preAuthorizationManagement);

            Dictionary<string, int> teamsCodes = GetTeamsCodes();

            TeamDAL teamDal = new TeamDAL(this.GlobalContext);

            string currentControlStage = GetCurrentControlStage(mergedAuthorizationManagement.alt_ControlStageTeamId, teamDal, teamsCodes);

            if ((currentControlStage == "MoneyLaunderingControl" || currentControlStage == "OperationalControl")
                && HasChangesAfterManagerApproval(mergedAuthorizationManagement.alt_DigitalFormVerificationId.Id))
            {
                targetAuthorizationManagement.alt_ControlStageStatusCode = new OptionSetValue((int)ControlStageStatusCode.BackManagerBackControl);
            }
        }

        public void HandleBackControlReasonAnnotaionInDigitalFormVerification(alt_AuthorizationManagement targetAuthorizationManagement, alt_AuthorizationManagement preAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();

            alt_AuthorizationManagement mergedAuthorizationManagement = preAuthorizationManagement != null ?
                targetAuthorizationManagement.Merge(preAuthorizationManagement) : targetAuthorizationManagement;

            if (mergedAuthorizationManagement.AttributeHasValue<string>(alt_AuthorizationManagement.Fields.alt_BackConrolReason)
                && mergedAuthorizationManagement.AttributeHasValue<EntityReference>(alt_AuthorizationManagement.Fields.alt_DigitalFormVerificationId)
                && mergedAuthorizationManagement.AttributeHasValue<EntityReference>(alt_AuthorizationManagement.Fields.alt_SignerNameSystemUserId))
            {
                Annotation annotationToCreate = new Annotation()
                {
                    OwnerId = mergedAuthorizationManagement.alt_SignerNameSystemUserId,
                    NoteText = mergedAuthorizationManagement.alt_BackConrolReason,
                    ObjectId = mergedAuthorizationManagement.alt_DigitalFormVerificationId,
                    Subject = "סיבת חזרה לבקרה"
                };
                AnnotationDAL annotationDAL = new AnnotationDAL(this.GlobalContext);
                annotationDAL.Create(annotationToCreate);
            }
        }

        private bool HasChangesAfterManagerApproval(Guid digitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDal =
                new DigitalFormVerificationDAL(this.GlobalContext);

            alt_DigitalFormVerification digitalFormVerification =
                digitalFormVerificationDal.Get(
                    digitalFormVerificationId,
                    new[]
                    {
                alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate,
                alt_DigitalFormVerification.Fields.alt_ChangeAfterManagerApprovalDate
                    });

            if (digitalFormVerification == null)
            {
                return false;
            }

            if (!digitalFormVerification.AttributeHasValue<DateTime>(
                alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate))
            {
                return false;
            }

            if (!digitalFormVerification.AttributeHasValue<DateTime>(
                alt_DigitalFormVerification.Fields.alt_ChangeAfterManagerApprovalDate))
            {
                return false;
            }

            return digitalFormVerification.alt_ChangeAfterManagerApprovalDate >
                   digitalFormVerification.alt_LastManagerApprovalDate;
        }
        public void SetAuthorizationManagementName(alt_AuthorizationManagement targetAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();
            List<string> nameParts = new List<string>();
            if (targetAuthorizationManagement.AttributeHasValue<EntityReference>(alt_AuthorizationManagement.Fields.alt_DigitalFormVerificationId))
            {
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(GlobalContext);
                nameParts.Add(digitalFormVerificationDal.Get(targetAuthorizationManagement.alt_DigitalFormVerificationId.Id, new string[] { alt_DigitalFormVerification.Fields.alt_DigitalFormNumber }).alt_DigitalFormNumber);
            }
            if (targetAuthorizationManagement.AttributeHasValue<EntityReference>(alt_AuthorizationManagement.Fields.alt_ControlStageTeamId))
            {
                TeamDAL teamDal = new TeamDAL(GlobalContext);
                nameParts.Add(teamDal.GetPrimeryAttributeValue(targetAuthorizationManagement.alt_ControlStageTeamId, Team.Fields.Name));
            }
            string name = string.Join(" - ", nameParts);
            targetAuthorizationManagement.alt_Name = name;
        }

        public void HandleControlStageStatusCode(alt_AuthorizationManagement targetAuthorizationManagement, alt_AuthorizationManagement preAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();
            if (targetAuthorizationManagement.AttributeHasValue<OptionSetValue>(alt_AuthorizationManagement.Fields.alt_ControlStageStatusCode))
            {
                if (targetAuthorizationManagement.alt_ControlStageStatusCode.Value == (int)ControlStageStatusCode.Approval)
                {
                    alt_AuthorizationManagement mergedAuthorizationManagement = targetAuthorizationManagement.Merge(preAuthorizationManagement);
                    Dictionary<string, int> teamsCodes = GetTeamsCodes();

                    TeamDAL teamDal = new TeamDAL(this.GlobalContext);

                    string currentControlStage = GetCurrentControlStage(mergedAuthorizationManagement.alt_ControlStageTeamId, teamDal, teamsCodes);
                    if (currentControlStage != "OperationalControl")
                    {
                        this.SetRecordInactive(targetAuthorizationManagement);
                    }
                }
                else
                {
                    this.SetRecordInactive(targetAuthorizationManagement);
                }
            }
        }

        private void SetRecordInactive(alt_AuthorizationManagement targetAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();
            targetAuthorizationManagement.alt_SignerNameSystemUserId = new EntityReference(SystemUser.EntityLogicalName, GlobalContext.InitiatingUserId);
            targetAuthorizationManagement.alt_SignatureDate = DateTime.UtcNow;
            targetAuthorizationManagement.StateCode = alt_AuthorizationManagementState.Inactive;
            targetAuthorizationManagement.StatusCode = new OptionSetValue((int)AuthorizationManagementStatusCode.Inactive);
        }

        //Happens only after KYC
        public void UpdateDigitalFormVerificationRequirementsByRiskLevel(alt_AuthorizationManagement targetAuthorizationManagement, alt_AuthorizationManagement preAuthorizationManagement = null)
        {
            this.GlobalContext.LogEntry();
            if (targetAuthorizationManagement.AttributeHasValue<OptionSetValue>(alt_AuthorizationManagement.Fields.alt_CapitalRiskLevelAccountCode))
            {
                bool managerVerificationRequired = preAuthorizationManagement == null ?
                    (targetAuthorizationManagement.alt_CapitalRiskLevelAccountCode.Value != (int)CapitalRiskLevelAccountCode.Low) : true;
                bool moneyLaunderingVerification = targetAuthorizationManagement.alt_CapitalRiskLevelAccountCode.Value == (int)CapitalRiskLevelAccountCode.High ? true : false;

                if (moneyLaunderingVerification || managerVerificationRequired)
                {
                    alt_AuthorizationManagement mergedAuthorizationManagement = preAuthorizationManagement == null ? targetAuthorizationManagement : targetAuthorizationManagement.Merge(preAuthorizationManagement);
                    DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
                    alt_DigitalFormVerification digitalFormVerificationRetrieve = digitalFormVerificationDal.Get(mergedAuthorizationManagement.alt_DigitalFormVerificationId.Id, new string[] { alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode, alt_DigitalFormVerification.Fields.alt_MoneyLaunderingVerificationCode });

                    if ((managerVerificationRequired
                            && digitalFormVerificationRetrieve.alt_ManagerVerificationRequiredCode.Value != (int)ManagerVerificationRequiredCode.Yes)
                        || (moneyLaunderingVerification
                            && digitalFormVerificationRetrieve.alt_MoneyLaunderingVerificationCode.Value != (int)MoneyLaunderingVerificationCode.Yes))
                    {
                        alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification()
                        {
                            Id = mergedAuthorizationManagement.alt_DigitalFormVerificationId.Id,
                        };
                        if (managerVerificationRequired
                            && digitalFormVerificationRetrieve.alt_ManagerVerificationRequiredCode.Value != (int)ManagerVerificationRequiredCode.Yes)
                        {
                            digitalFormVerificationToUpdate.alt_ManagerVerificationRequiredCode = new OptionSetValue((int)ManagerVerificationRequiredCode.Yes);
                        }
                        if (moneyLaunderingVerification
                            && digitalFormVerificationRetrieve.alt_MoneyLaunderingVerificationCode.Value != (int)MoneyLaunderingVerificationCode.Yes)
                        {
                            digitalFormVerificationToUpdate.alt_MoneyLaunderingVerificationCode = new OptionSetValue((int)MoneyLaunderingVerificationCode.Yes);
                        }
                        digitalFormVerificationDal.Update(digitalFormVerificationToUpdate);
                    }
                }
            }
        }

        public void UpdateAccountHolder(alt_AuthorizationManagement targetAuthorizationManagement, alt_AuthorizationManagement preAuthorizationManagement = null)
        {
            this.GlobalContext.LogEntry();
            if (targetAuthorizationManagement.AttributeHasValue<OptionSetValue>(alt_AuthorizationManagement.Fields.alt_CapitalRiskLevelAccountCode)
                && targetAuthorizationManagement.alt_CapitalRiskLevelAccountCode.Value == (int)CapitalRiskLevelAccountCode.High)
            {
                alt_AuthorizationManagement mergedAuthorizationManagement = preAuthorizationManagement == null ?
                    targetAuthorizationManagement : targetAuthorizationManagement.Merge(preAuthorizationManagement);
                AccountHolderDAL accountHolderDal = new AccountHolderDAL(this.GlobalContext);
                List<alt_AccountHolder> accountHoldersRetrieve = accountHolderDal.GetAccountHolderByTypeAccountHolderAndDigitalFormVerificationId(mergedAuthorizationManagement.alt_DigitalFormVerificationId.Id, new int[] { (int)AccountHolderTypeCode.Owner }, new[] { alt_AccountHolder.Fields.alt_BeneficiaryDeclarationRequiredBit });
                if (accountHoldersRetrieve.Count > 0)
                {
                    foreach (var accountHolder in accountHoldersRetrieve)
                    {
                        if (accountHolder.alt_BeneficiaryDeclarationRequiredBit == false)
                        {
                            accountHolder.alt_BeneficiaryDeclarationRequiredBit = true;
                            accountHolderDal.Update(accountHolder);
                        }
                    }
                }
            }
        }

        private bool IsAuthorizationManagementInactive(
      alt_AuthorizationManagement targetAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();

            return targetAuthorizationManagement.StateCode == alt_AuthorizationManagementState.Inactive
                && targetAuthorizationManagement.StatusCode?.Value == (int)AuthorizationManagementStatusCode.Inactive;
        }
        public void HandleNextAuthorizationManagement(alt_AuthorizationManagement targetAuthorizationManagement, alt_AuthorizationManagement preAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();


            if (IsAuthorizationManagementInactive(targetAuthorizationManagement))

            {
                AuthorizationWorkflowContext context = BuildWorkflowContext(targetAuthorizationManagement, preAuthorizationManagement);

                ResolveWorkflow(context);

                if (string.IsNullOrEmpty(context.NextControlStage))
                {
                    return;
                }

                CreateNextAuthorizationManagement(context.AuthorizationManagement, context.NextControlStageTeam);

                UpdateDigitalFormVerification(context.DigitalFormVerificationId, context.NextControlStageTeam, context.NextControlStage, context.IsManagerApproval);
            }
        }



        private AuthorizationWorkflowContext BuildWorkflowContext(
    alt_AuthorizationManagement targetAuthorizationManagement,
    alt_AuthorizationManagement preAuthorizationManagement)
        {
            this.GlobalContext.LogEntry();

            alt_AuthorizationManagement mergedAuthorizationManagement = targetAuthorizationManagement.Merge(preAuthorizationManagement);

            TeamDAL teamDal = new TeamDAL(this.GlobalContext);

            Dictionary<string, int> teamsCodes = GetTeamsCodes();

            string currentControlStage = GetCurrentControlStage(mergedAuthorizationManagement.alt_ControlStageTeamId, teamDal, teamsCodes);

            ControlStageStatusCode? currentStatus = null;

            if (mergedAuthorizationManagement.AttributeHasValue<OptionSetValue>(alt_AuthorizationManagement.Fields.alt_ControlStageStatusCode))
            {
                currentStatus = (ControlStageStatusCode)mergedAuthorizationManagement.alt_ControlStageStatusCode.Value;
            }

            return new AuthorizationWorkflowContext()
            {
                AuthorizationManagement = mergedAuthorizationManagement,
                DigitalFormVerificationId = mergedAuthorizationManagement.alt_DigitalFormVerificationId.Id,
                TeamDal = teamDal,
                TeamsCodes = teamsCodes,
                CurrentControlStage = currentControlStage,
                CurrentControlStageStatus = currentStatus
            };
        }


        private void BuildManagerApprovalInformation(
      alt_DigitalFormVerification digitalFormVerificationToUpdate,
      Guid digitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();

            DateTime utcNow = DateTime.UtcNow;

            DigitalFormVerificationDAL digitalFormVerificationDal =
                new DigitalFormVerificationDAL(this.GlobalContext);

            alt_DigitalFormVerification currentDigitalFormVerification =
                digitalFormVerificationDal.Get(
                    digitalFormVerificationId,
                    new[]
                    {
                alt_DigitalFormVerification.Fields.alt_ChangesAfterManagerApproval
                    });

            // נשמר ב-UTC. Dynamics יציג למשתמש לפי אזור הזמן שלו.
            digitalFormVerificationToUpdate.alt_LastManagerApprovalDate = utcNow;

            string currentHistory =
                currentDigitalFormVerification?.alt_ChangesAfterManagerApproval ?? string.Empty;

            ManagerControlChangeTrackingBL managerControlChangeTrackingBL =
                new ManagerControlChangeTrackingBL(GlobalContext);

            string newHeader =
                managerControlChangeTrackingBL.BuildManagerApprovalHeader(utcNow);

            digitalFormVerificationToUpdate.alt_ChangesAfterManagerApproval =
                managerControlChangeTrackingBL.AppendManagerApprovalHistory(
                    currentHistory,
                    newHeader);
        }


        private void ResolveWorkflow(
    AuthorizationWorkflowContext context)
        {
            this.GlobalContext.LogEntry();

            switch (context.CurrentControlStageStatus)
            {
                case ControlStageStatusCode.Approval:
                    ResolveApprovalWorkflow(context);
                    break;

                case ControlStageStatusCode.BackControl:
                    ResolveBackControlWorkflow(context);
                    break;

                case ControlStageStatusCode.BackManagerBackControl:
                    ResolveManagerBackControlWorkflow(context);
                    break;

                case ControlStageStatusCode.FormCancellation:
                    ResolveCancellationWorkflow(context);
                    break;

                default:
                    context.NextControlStage = "JoiningControl";
                    break;
            }

            if (string.IsNullOrEmpty(context.NextControlStage))
            {
                return;
            }

            context.NextControlStageTeam = GetControlStageTeamReference(context.NextControlStage, context.TeamDal, context.TeamsCodes);
            context.IsManagerApproval = context.CurrentControlStage == "ManagementControl" && context.CurrentControlStageStatus == ControlStageStatusCode.Approval;
        }
        private void ResolveApprovalWorkflow(
    AuthorizationWorkflowContext context)
        {
            this.GlobalContext.LogEntry();

            context.NextControlStage = GetNextStageControlForApproval(context.DigitalFormVerificationId, context.CurrentControlStage);
        }
        private void ResolveBackControlWorkflow(
    AuthorizationWorkflowContext context)
        {
            this.GlobalContext.LogEntry();

            context.NextControlStage = "JoiningControl";
        }
        private void ResolveManagerBackControlWorkflow(AuthorizationWorkflowContext context)
        {
            this.GlobalContext.LogEntry();

            context.NextControlStage = "ManagementControl";
        }
        private void ResolveCancellationWorkflow(
    AuthorizationWorkflowContext context)
        {
            this.GlobalContext.LogEntry();

            if (context.CurrentControlStage == "OperationalControl")
            {
                context.NextControlStage = "ManagementControl";
                return;
            }

            UpdateCancellationDigitalFormVerification(
                context.AuthorizationManagement.alt_DigitalFormVerificationId,
                GetControlStageTeamReference(
                    "OperationalControl",
                    context.TeamDal,
                    context.TeamsCodes));
        }

        private Dictionary<string, int> GetTeamsCodes()
        {
            this.GlobalContext.LogEntry();

            return JsonSerializer.Deserialize<Dictionary<string, int>>(GlobalContext.CacheManager.GetGlobalParameter<string>("TeamsCodes"));
        }


        private EntityReference GetControlStageTeamReference(
     string controlStage,
     TeamDAL teamDal,
     Dictionary<string, int> teamsCodes)
        {
            this.GlobalContext.LogEntry();

            return new EntityReference(
                Team.EntityLogicalName,
                teamDal.GetFirstOrDefaultByAttribute(
                    Team.Fields.alt_TeamCodeInt,
                    teamsCodes[controlStage],
                    new[] { Team.Fields.Id }).Id);
        }

        private string GetCurrentControlStage(
       EntityReference controlStageTeamId,
       TeamDAL teamDal,
       Dictionary<string, int> teamsCodes)
        {
            this.GlobalContext.LogEntry();

            return teamsCodes.FirstOrDefault(x =>
                x.Value == teamDal.Get(
                    controlStageTeamId.Id,
                    new[] { Team.Fields.alt_TeamCodeInt })
                .alt_TeamCodeInt).Key;
        }



        private void CreateNextAuthorizationManagement(alt_AuthorizationManagement mergedAuthorizationManagement, EntityReference controlStageTeamId)
        {
            this.GlobalContext.LogEntry();
            AuthorizationManagementDAL authorizationManagementDal = new AuthorizationManagementDAL(this.GlobalContext);
            alt_AuthorizationManagement authorizationManagement = new alt_AuthorizationManagement()
            {
                alt_CapitalRiskLevelAccountCode = mergedAuthorizationManagement.alt_CapitalRiskLevelAccountCode,
                alt_DigitalFormVerificationId = mergedAuthorizationManagement.alt_DigitalFormVerificationId,
                alt_ControlStageTeamId = controlStageTeamId,
                OwnerId = controlStageTeamId,
                alt_CreditRequestCode = mergedAuthorizationManagement.alt_CreditRequestCode,
                alt_CreditAmountNISMny = mergedAuthorizationManagement.alt_CreditAmountNISMny,
                alt_LineWriteOptionsMny = mergedAuthorizationManagement.alt_LineWriteOptionsMny,
                alt_LineStockShortMny = mergedAuthorizationManagement.alt_LineStockShortMny,
                alt_LineAggregateCreditLimitMny = mergedAuthorizationManagement.alt_LineAggregateCreditLimitMny,
                alt_LineAggregateCreditLimitPercentInt = mergedAuthorizationManagement.alt_LineAggregateCreditLimitPercentInt,
                alt_ShortSaleRequestApprovalBit = mergedAuthorizationManagement.alt_ShortSaleRequestApprovalBit,
                alt_OptinExerciseRequestApprovalCode = mergedAuthorizationManagement.alt_OptinExerciseRequestApprovalCode,
                alt_CreditRequestRemarks = mergedAuthorizationManagement.alt_CreditRequestRemarks,
                alt_SubjectiveReportingCode = mergedAuthorizationManagement.alt_SubjectiveReportingCode

            };
            authorizationManagementDal.Create(authorizationManagement);
        }

        private string GetNextStageControlForApproval(Guid digitalFormVerificationId, string currentTeam)
        {
            this.GlobalContext.LogEntry();
            string NextStageControl = string.Empty;
            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
            switch (currentTeam)
            {
                case "JoiningControl":
                    NextStageControl = digitalFormVerificationDal.Get(digitalFormVerificationId, new string[] { alt_DigitalFormVerification.Fields.alt_ManagerVerificationRequiredCode }).alt_ManagerVerificationRequiredCode.Value == (int)ManagerVerificationRequiredCode.Yes ? "ManagementControl" : "OperationalControl";

                    break;
                case "ManagementControl":
                    NextStageControl = digitalFormVerificationDal.Get(digitalFormVerificationId, new string[] { alt_DigitalFormVerification.Fields.alt_MoneyLaunderingVerificationCode }).alt_MoneyLaunderingVerificationCode.Value == (int)MoneyLaunderingVerificationCode.Yes ? "MoneyLaunderingControl" : "OperationalControl";
                    break;
                case "MoneyLaunderingControl":
                    NextStageControl = "OperationalControl";
                    break;
                default: break;
            }
            return NextStageControl;
        }

        private alt_DigitalFormVerification BuildDigitalFormVerificationForNextAuthorization(
       Guid digitalFormVerificationId,
       EntityReference controlStageTeamId,
       string nextControl)
        {
            this.GlobalContext.LogEntry();

            alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification()
            {
                Id = digitalFormVerificationId,
                alt_ControlStageTeamId = controlStageTeamId
            };

            if (nextControl != "OperationalControl")
            {
                DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);

                alt_DigitalFormVerification currentDigitalFormVerification =
                    digitalFormVerificationDal.Get(
                        digitalFormVerificationId,
                        new[]
                        {
                    alt_DigitalFormVerification.Fields.alt_FormStatusCode
                        });

                if (currentDigitalFormVerification == null ||
                    !currentDigitalFormVerification.AttributeHasValue<OptionSetValue>(
                        alt_DigitalFormVerification.Fields.alt_FormStatusCode) ||
                    currentDigitalFormVerification.alt_FormStatusCode.Value !=
                        (int)FormStatusCode.InAuthorizationProcess)
                {
                    digitalFormVerificationToUpdate.alt_FormStatusCode =
                        new OptionSetValue((int)FormStatusCode.InAuthorizationProcess);
                }
            }

            return digitalFormVerificationToUpdate;
        }



        private void UpdateDigitalFormVerification(Guid DigitalFormVerificationId, EntityReference NextControlStageTeam, string NextControlStage, bool IsManagerApproval)
        {
            this.GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);

            alt_DigitalFormVerification digitalFormVerificationToUpdate =
                BuildDigitalFormVerificationForNextAuthorization(
                  DigitalFormVerificationId,
                  NextControlStageTeam,
                  NextControlStage);

            if (IsManagerApproval)
            {
                BuildManagerApprovalInformation(
                    digitalFormVerificationToUpdate,
                    DigitalFormVerificationId);
            }

            digitalFormVerificationDal.Update(digitalFormVerificationToUpdate);
        }



        private void UpdateCancellationDigitalFormVerification(EntityReference alt_DigitalFormVerificationId, EntityReference taemId)
        {
            this.GlobalContext.LogEntry();
            DigitalFormVerificationDAL digitalFormVerificationDal = new DigitalFormVerificationDAL(this.GlobalContext);
            alt_DigitalFormVerification digitalFormVerificationToUpdate = new alt_DigitalFormVerification()
            {
                alt_DigitalFormVerificationId = alt_DigitalFormVerificationId.Id,
                StateCode = alt_DigitalFormVerificationState.Inactive,
                StatusCode = new OptionSetValue((int)DigitalFormVerificationStatusCode.Inactive),
                alt_FormStatusCode = new OptionSetValue((int)FormStatusCode.Canceled),
                alt_ControlStageTeamId = taemId
            };
            digitalFormVerificationDal.Update(digitalFormVerificationToUpdate);
        }
    }

    public class AuthorizationWorkflowContext
    {
        // Entities
        public alt_AuthorizationManagement AuthorizationManagement { get; set; }


        public Guid DigitalFormVerificationId { get; set; }

        // Infrastructure
        public TeamDAL TeamDal { get; set; }

        public Dictionary<string, int> TeamsCodes { get; set; }

        // Current state
        public string CurrentControlStage { get; set; }

        public ControlStageStatusCode? CurrentControlStageStatus { get; set; }

        // Next state
        public string NextControlStage { get; set; }

        public EntityReference NextControlStageTeam { get; set; }

        // Decisions
        public bool IsManagerApproval { get; set; }


    }

}