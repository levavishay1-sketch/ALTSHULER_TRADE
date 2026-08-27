using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alt.BusinessLogicLayer.Crm
{
    public class ManagerControlChangeTrackingBL : CrmBaseBL
    {
        private const string DigitalFormVerificationLookup = "alt_digitalformverificationid";
        private const string ManagerApprovalExcludedFieldsParameter = "ManagerApprovalExcludedFields";

        private string[] GetExcludedFields(string entityLogicalName)
        {
            if (string.IsNullOrWhiteSpace(entityLogicalName))
            {
                return Array.Empty<string>();
            }

            string json = this.GlobalContext.CacheManager
                .GetGlobalParameter<string>(ManagerApprovalExcludedFieldsParameter);

            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<string>();
            }

            Dictionary<string, string[]> excludedFields =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);

            if (excludedFields == null)
            {
                return Array.Empty<string>();
            }

            return excludedFields.TryGetValue(
                entityLogicalName.ToLowerInvariant(),
                out string[] fields)
                ? fields
                : Array.Empty<string>();
        }

        private bool IsDigitalFormVerificationRemoved(Entity target, Entity preImage)
        {
            if (target == null || preImage == null)
                return false;

            if (!target.Attributes.Contains("alt_digitalformverificationid"))
                return false;

            var newValue = target.GetAttributeValue<EntityReference>("alt_digitalformverificationid");

            var oldValue = preImage.GetAttributeValue<EntityReference>("alt_digitalformverificationid");

            return oldValue != null && newValue == null;
        }

        public string AppendManagerApprovalHistory(
    string existingHistory,
    string newEntry)
        {
            if (string.IsNullOrWhiteSpace(existingHistory))
            {
                return newEntry;
            }

            return existingHistory.TrimEnd()
                + Environment.NewLine
                + Environment.NewLine
                + newEntry.Trim();
        }
        public ManagerControlChangeTrackingBL(GlobalContext globalContext)
            : base(globalContext)
        {
        }
        public string BuildManagerApprovalHeader(DateTime managerApprovalDate)
        {
            this.GlobalContext.LogEntry();

            DateTime displayDate = ConvertUtcToIsraelTime(managerApprovalDate);


            return
                "==================================================" + Environment.NewLine +
                $"תאריך אישור מנהל: {displayDate:dd/MM/yyyy HH:mm:ss}" + Environment.NewLine +
                "==================================================";
        }

        private DateTime ConvertUtcToIsraelTime(DateTime dateTime)
        {
            TimeZoneInfo israelTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");

            DateTime utcDateTime =
                DateTime.SpecifyKind(
                    dateTime,
                    DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(
                utcDateTime,
                israelTimeZone);
        }

        public void MoveLastAuthorizationManagementBack<T>(T target,T preImage = null) where T : Entity
        {
            this.GlobalContext.LogEntry();
            if (target == null)
            {
                return;
            }
            if (preImage != null && !HasRevalntFieldsChanged(target, preImage))
            {
                return;
            }
            EntityReference digitalFormVerificationReference = ResolveDigitalFormVerificationReference(target, preImage);

            if (digitalFormVerificationReference == null)
            {
                return;
            }

            alt_DigitalFormVerification digitalFormVerification = GetDigitalFormVerificationForAuthorizationBack(digitalFormVerificationReference.Id);

            if (digitalFormVerification == null)
            {
                return;
            }
            if (!digitalFormVerification.AttributeHasValue<DateTime>(alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate))
            {
                return;
            }

            if (!IsAllowedAuthorizationStatus(digitalFormVerification.alt_FormStatusCode.Value))
            {
                return;
            }

            if (!IsAuthorizationBackControlTeam(digitalFormVerification.alt_ControlStageTeamId))
            {
                return;
            }

            UpdateLastAuthorizationManagement(digitalFormVerificationReference.Id);

        }






        private alt_DigitalFormVerification GetDigitalFormVerificationForAuthorizationBack(
            Guid id)
        {
            DigitalFormVerificationDAL digitalFormVerificationDal =
                new DigitalFormVerificationDAL(this.GlobalContext);

            alt_DigitalFormVerification digitalFormVerification =
                digitalFormVerificationDal.Get(
                    id,
                    new[]
                    {
                        alt_DigitalFormVerification.Fields.alt_ControlStageTeamId,
                        alt_DigitalFormVerification.Fields.alt_FormStatusCode,
                        alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate
                    });

            if (digitalFormVerification == null ||
                digitalFormVerification.alt_ControlStageTeamId == null ||
                !digitalFormVerification.AttributeHasValue<OptionSetValue>(
                    alt_DigitalFormVerification.Fields.alt_FormStatusCode))
            {
                return null;
            }

            return digitalFormVerification;
        }


        private bool IsAllowedAuthorizationStatus(int formStatus)
        {
            return formStatus == (int)FormStatusCode.InAuthorizationProcess
                   ||
                   formStatus == (int)FormStatusCode.AwaitingForDeposit;

        }
        private alt_DigitalFormVerification GetRelatedDigitalFormVerification(
    Entity target,
    Entity preImage)
        {
            EntityReference reference =
                ResolveDigitalFormVerificationReference(target, preImage);

            if (reference == null)
            {
                return null;
            }

            return GetDigitalFormVerificationForChangeTracking(reference.Id);
        }


        public void TrackChanges<T>(
            T target,
            T preImage = null)
            where T : Entity
        {
            this.GlobalContext.LogEntry();

            if (target == null)
            {
                return;
            }


            alt_DigitalFormVerification digitalFormVerification = GetRelatedDigitalFormVerification(target, preImage);

            if (digitalFormVerification == null)
            {
                return;
            }
            if (!IsAllowedAuthorizationStatus(digitalFormVerification.alt_FormStatusCode.Value))
            {
                return;
            }

            string controlStage = GetControlStageName(digitalFormVerification);

            if (!digitalFormVerification.AttributeHasValue<DateTime>(alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate))
            {
                return;
            }

            DateTime actionDate = DateTime.UtcNow;
            string actionBy = GetCurrentUserName();

            if (preImage == null)
            {
                SaveCreation(
                    target,
                    digitalFormVerification,
                    actionDate,
                    actionBy);

                return;
            }

            EntityMetadata entityMetadata = GlobalContext.GetEntityMetadata(target.LogicalName);

            if (entityMetadata == null)
            {
                return;
            }

            string[] excludedFields =
                GetExcludedFields(target.LogicalName);

            List<ChangeLogItem> changes =
                GetChangedFields(
                    target,
                    preImage,
                    entityMetadata,
                    excludedFields,
                    actionDate,
                    actionBy,
                    controlStage);

            if (changes.Count == 0)
            {
                return;
            }

            UpdateChanges(
                target,
                digitalFormVerification,
                changes,
                actionDate);
        }


        private void UpdateChanges(
            Entity target,
            alt_DigitalFormVerification digitalFormVerification,
            List<ChangeLogItem> changes,
            DateTime actionDate)
        {
            if (target.LogicalName.Equals(
                    alt_DigitalFormVerification.EntityLogicalName,
                    StringComparison.OrdinalIgnoreCase))
            {
                UpdateDigitalFormVerificationTargetChangesLog(
                    target,
                    digitalFormVerification,
                    changes,
                    actionDate);
            }
            else
            {
                UpdateDigitalFormVerificationChangesLog(
                    digitalFormVerification,
                    changes,
                    actionDate);
            }
        }


        private void UpdateDigitalFormVerificationTargetChangesLog(
            Entity target,
            alt_DigitalFormVerification digitalFormVerification,
            List<ChangeLogItem> changes,
            DateTime actionDate)
        {
            string newChangesLog =
                BuildChangesLog(changes);

            target[
                alt_DigitalFormVerification.Fields.alt_ChangeAfterManagerApprovalDate]
                = actionDate;

            target[
        alt_DigitalFormVerification.Fields.alt_ChangesAfterManagerApproval]
        =
        AppendManagerApprovalHistory(
            digitalFormVerification.alt_ChangesAfterManagerApproval,
            newChangesLog);
        }


        private bool IsAuthorizationBackControlTeam(
            EntityReference teamReference)
        {
            int? teamCode = GetTeamCode(teamReference);

            if (!teamCode.HasValue)
            {
                return false;
            }

            string teamName =
                GetTeamName(teamCode.Value);

            return
                teamName.Equals(
                    TeamNames.OperationalControl,
                    StringComparison.OrdinalIgnoreCase)
                ||
                teamName.Equals(
                    TeamNames.MoneyLaunderingControl,
                    StringComparison.OrdinalIgnoreCase);
        }


        private Dictionary<string, int> GetTeamsCodes()
        {
            string teamsCodesJson =
                this.GlobalContext.CacheManager
                    .GetGlobalParameter<string>("TeamsCodes");

            if (string.IsNullOrWhiteSpace(teamsCodesJson))
            {
                return null;
            }

            return System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, int>>(teamsCodesJson);
        }


        private int? GetTeamCode(EntityReference teamReference)
        {
            if (teamReference == null)
            {
                return null;
            }

            TeamDAL teamDAL =
                new TeamDAL(this.GlobalContext);

            Team team =
                teamDAL.Get(
                    teamReference.Id,
                    new[]
                    {
                        Team.Fields.alt_TeamCodeInt
                    });

            return team?.alt_TeamCodeInt;
        }
        private string GetTeamName(int teamCode)
        {
            Dictionary<string, int> teamsCodes =
                GetTeamsCodes();

            if (teamsCodes == null)
            {
                return string.Empty;
            }

            return teamsCodes
                .FirstOrDefault(x => x.Value == teamCode)
                .Key ?? string.Empty;
        }


        private string GetControlStageDisplayName(string teamName)
        {
            switch (teamName)
            {
                case TeamNames.OperationalControl:
                    return "בקרה תפעולית";

                case TeamNames.MoneyLaunderingControl:
                    return "בקרת הלבנת הון";

                case TeamNames.ManagerControl:
                    return "בקרת מנהל";

                case TeamNames.JoiningControl:
                    return "בקרת הצטרפות";

                default:
                    return teamName;
            }
        }


        private string GetControlStageName(
            alt_DigitalFormVerification digitalFormVerification)
        {
            this.GlobalContext.LogEntry();

            int? teamCode =
                GetTeamCode(
                    digitalFormVerification?.alt_ControlStageTeamId);

            if (!teamCode.HasValue)
            {
                return string.Empty;
            }

            string teamName =
                GetTeamName(teamCode.Value);

            if (string.IsNullOrWhiteSpace(teamName))
            {
                return string.Empty;
            }

            return GetControlStageDisplayName(teamName);
        }


        private void UpdateLastAuthorizationManagement(
            Guid digitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();

            AuthorizationManagementDAL authorizationManagementDal =
                new AuthorizationManagementDAL(this.GlobalContext);

            alt_AuthorizationManagement retrievedAuthorizationManagement =
                authorizationManagementDal
                    .GetLastCreatedOnAuthorizationManagementByDigitalFormVerificationId(
                        digitalFormVerificationId);

            if (retrievedAuthorizationManagement == null)
            {
                return;
            }

            Entity authorizationManagement =
                new Entity("alt_authorizationmanagement")
                {
                    Id = retrievedAuthorizationManagement.Id
                };
            authorizationManagement["alt_controlstagestatuscode"] = new OptionSetValue((int)ControlStageStatusCode.BackManagerBackControl);

            this.GlobalContext.OrganizationService.Update(  authorizationManagement);



        }


        private EntityReference ResolveDigitalFormVerificationReference(
            Entity target,
            Entity preImage)
        {
            this.GlobalContext.LogEntry();

            if (target == null)
            {
                return null;
            }


            if (target.LogicalName.Equals(
                    "alt_digitalformverification",
                    StringComparison.OrdinalIgnoreCase))
            {
                Guid recordId =
                    target.Id != Guid.Empty
                        ? target.Id
                        : (preImage?.Id ?? Guid.Empty);


                return recordId != Guid.Empty
                    ? new EntityReference(
                        "alt_digitalformverification",
                        recordId)
                    : null;
            }


            EntityReference digitalFormVerificationReference =
                target.GetAttributeValue<EntityReference>(
                    DigitalFormVerificationLookup);


            if (digitalFormVerificationReference != null)
            {
                return digitalFormVerificationReference;
            }


            if (preImage == null)
            {
                return null;
            }


            return preImage.GetAttributeValue<EntityReference>(
                DigitalFormVerificationLookup);
        }


        private void UpdateDigitalFormVerificationChangesLog(
            alt_DigitalFormVerification digitalFormVerification,
            List<ChangeLogItem> changes,
            DateTime changeDate)
        {
            this.GlobalContext.LogEntry();

            string newChangesLog =
                BuildChangesLog(changes);


            Entity entityToUpdate =
                new Entity(
                    alt_DigitalFormVerification.EntityLogicalName)
                {
                    Id = digitalFormVerification.Id
                };


            entityToUpdate[
                alt_DigitalFormVerification.Fields.alt_ChangeAfterManagerApprovalDate]
                = changeDate;


            entityToUpdate[
       alt_DigitalFormVerification.Fields.alt_ChangesAfterManagerApproval]
       =
       AppendManagerApprovalHistory(
           digitalFormVerification.alt_ChangesAfterManagerApproval,
           newChangesLog);


            this.GlobalContext.OrganizationService.Update(entityToUpdate);

        }
        private List<ChangeLogItem> GetChangedFields<T>(
    T target,
    T preImage,
    EntityMetadata entityMetadata,
    IEnumerable<string> excludedFields,
    DateTime changeDate,
    string changedBy,
    string controlStage)
    where T : Entity
        {
            this.GlobalContext.LogEntry();

            List<ChangeLogItem> changes =
                new List<ChangeLogItem>();

            if (entityMetadata == null)
            {
                return changes;
            }


            var excludedFieldsLower =
                excludedFields != null
                    ? excludedFields
                        .Select(f => f.ToLower())
                        .ToList()
                    : new List<string>();


            CommonDAL commonDal =
                new CommonDAL(
                    this.GlobalContext,
                    target.LogicalName);


            var lookupEntity =
                commonDal.RetrieveLookupValues(target);


            target.EnrichLookups(lookupEntity);


            foreach (string fieldName in target.Attributes.Keys)
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    continue;
                }


                if (excludedFieldsLower.Contains(
                        fieldName.ToLower()))
                {
                    continue;
                }


                AttributeMetadata attributeMetadata =
                    entityMetadata.Attributes.FirstOrDefault(
                        x => x.LogicalName.Equals(
                            fieldName,
                            StringComparison.OrdinalIgnoreCase));


                if (attributeMetadata == null)
                {
                    continue;
                }


                string oldValue =
                    preImage == null
                        ? string.Empty
                        : preImage.GetDisplayValue(
                            fieldName,
                            entityMetadata);


                string newValue =
                    target.GetDisplayValue(
                        fieldName,
                        entityMetadata);


                if (string.Equals(
                        oldValue,
                        newValue,
                        StringComparison.Ordinal))
                {
                    continue;
                }


                string recordName =
                    target.GetAttributeValue<string>("alt_name")
                    ??
                    preImage?.GetAttributeValue<string>("alt_name")
                    ??
                    string.Empty;


                changes.Add(
                    new ChangeLogItem()
                    {
                        EntityName =
                            entityMetadata.DisplayName?
                                .UserLocalizedLabel?
                                .Label
                            ??
                            entityMetadata.GetEntityDisplayName()
                            ??
                            entityMetadata.LogicalName,


                        RecordName = recordName,


                        FieldName =
                            attributeMetadata.DisplayName?
                                .UserLocalizedLabel?
                                .Label
                            ??
                            fieldName,


                        OldValue = oldValue,

                        NewValue = newValue,

                        ChangeDate = changeDate,

                        ChangedBy = changedBy
                    });
            }


            return changes;
        }


        private alt_DigitalFormVerification
            GetDigitalFormVerificationForChangeTracking(
                Guid digitalFormVerificationId)
        {
            this.GlobalContext.LogEntry();


            DigitalFormVerificationDAL digitalFormVerificationDal =
                new DigitalFormVerificationDAL(
                    this.GlobalContext);


            return digitalFormVerificationDal.Get(
                digitalFormVerificationId,
                new[]
                {
                    alt_DigitalFormVerification.Fields.alt_LastManagerApprovalDate,

                    alt_DigitalFormVerification.Fields.alt_ChangeAfterManagerApprovalDate,

                    alt_DigitalFormVerification.Fields.alt_ChangesAfterManagerApproval,

                    alt_DigitalFormVerification.Fields.alt_ControlStageTeamId ,

                    alt_DigitalFormVerification.Fields.alt_FormStatusCode ,
                });
        }


        private void SaveCreation(
            Entity target,
            alt_DigitalFormVerification digitalFormVerification,
            DateTime creationDate,
            string createdBy)
        {
            this.GlobalContext.LogEntry();


            string entityName =
                GetEntityDisplayName(
                    target.LogicalName);


            string recordName =
                target.GetAttributeValue<string>("alt_name")
                ?? string.Empty;


            string creationLog =
                $"נוצרה רשומה חדשה של {entityName}{Environment.NewLine}" +
                $"שם הרשומה: {recordName}{Environment.NewLine}" +
                $"תאריך יצירה: {ConvertUtcToIsraelTime(creationDate):dd/MM/yyyy HH:mm:ss}{Environment.NewLine}" +
                $"משתמש: {createdBy}";


            Entity digitalFormVerificationToUpdate =
                new Entity(
                    alt_DigitalFormVerification.EntityLogicalName)
                {
                    Id = digitalFormVerification.Id
                };


            digitalFormVerificationToUpdate[
                "alt_changeaftermanagerapprovaldate"]
                = creationDate;

            digitalFormVerificationToUpdate[
                alt_DigitalFormVerification.Fields.alt_ChangesAfterManagerApproval]
                =
                AppendManagerApprovalHistory(
                    digitalFormVerification.alt_ChangesAfterManagerApproval,
                    creationLog);

            this.GlobalContext.OrganizationService.Update(
                digitalFormVerificationToUpdate);
        }


        private string GetEntityDisplayName(
            string logicalName)
        {
            switch (logicalName)
            {
                case alt_AccountHolder.EntityLogicalName:
                    return "בעל חשבון";


                case alt_KYC.EntityLogicalName:
                    return "הכר את הלקוח";


                case alt_MoneyLaunderingCalculation.EntityLogicalName:
                    return "מחשבון הלבנת הון";


                case alt_DigitalFormVerification.EntityLogicalName:
                    return "בקרת טופס הצטרפות";


                default:
                    return logicalName;
            }
        }


        private string BuildChangesLog(List<ChangeLogItem> changes)
        {
            this.GlobalContext.LogEntry();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < changes.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                var change = changes[i];

                sb.AppendLine($"תאריך שינוי: {ConvertUtcToIsraelTime(change.ChangeDate):dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"ישות: {change.EntityName}");
                sb.AppendLine($"שם הרשומה: {change.RecordName}");
                sb.AppendLine($"שדה: {change.FieldName}");
                sb.AppendLine($"ערך ישן: {change.OldValue}");
                sb.AppendLine($"ערך חדש: {change.NewValue}");
                sb.AppendLine($"משתמש: {change.ChangedBy}");
            }

            return sb.ToString();
        }


        private string GetCurrentUserName()
        {
            this.GlobalContext.LogEntry();


            SystemUserDAL systemUserDal =
                new SystemUserDAL(
                    this.GlobalContext);


            SystemUser systemUser =
                systemUserDal.Get(
                    this.GlobalContext.InitiatingUserId,
                    new[]
                    {
                        SystemUser.Fields.FullName
                    });


            return systemUser?.FullName ?? string.Empty;
        }


        public bool HasRevalntFieldsChanged(
            Entity target,
            Entity preImage)
        {
            this.GlobalContext.LogEntry();


            if (target == null ||
                preImage == null ||
                target.Attributes.Count == 0)
            {
                return false;
            }


            string[] excludedFields =
                GetExcludedFields(
                    target.LogicalName);


            var excludedFieldsLower =
                excludedFields != null
                    ? excludedFields
                        .Select(f => f.ToLower())
                        .ToList()
                    : new List<string>();


            EntityMetadata entityMetadata =
                GlobalContext.GetEntityMetadata(
                    target.LogicalName);


            if (entityMetadata == null)
            {
                return false;
            }


            foreach (string fieldName in target.Attributes.Keys)
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    continue;
                }


                if (excludedFieldsLower.Contains(
                        fieldName.ToLower()))
                {
                    continue;
                }


                string oldValue =
                    preImage.GetDisplayValue(
                        fieldName,
                        entityMetadata);


                string newValue =
                    target.GetDisplayValue(
                        fieldName,
                        entityMetadata);


                if (!string.Equals(
                        oldValue,
                        newValue,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }


            return false;
        }
    }


    public class ChangeLogItem
    {
        public string EntityName { get; set; }

        public string FieldName { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public DateTime ChangeDate { get; set; }

        public string ChangedBy { get; set; }

        public string RecordName { get; set; }
    }
}