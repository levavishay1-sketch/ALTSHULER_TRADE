using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using OneTimeConsole.DataAccessLayer;
using OneTimeConsole.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneTimeConsole
{
    public class DigitalFormVerificationDataConversion : OperationBase
    {
        public DigitalFormVerificationDataConversion(GlobalContext globalContext, OperationCode operationCode) : base(globalContext, operationCode)
        {
            this.OperationCode = operationCode;
        }

        public override void Run()
        {
            this.GlobalContext.LogEntry();
            switch (this.OperationCode)
            {
                //case OperationCode.DigitalFormVerificationMainAccountHolder:
                //    {
                //        HandleMainAccountHolderLogic();
                //        break;
                //    }
                case OperationCode.DigitalFormVerificationEncouragingDepositSystemUser:
                    {
                        HandleEncouragingDepositSystemUserLogic();
                        break;
                    }
                default:
                    break;
            }
        }

        private void HandleMainAccountHolderLogic()
        {
            GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(GlobalContext);
            List<alt_DigitalFormVerification> retrievedDigitalFormVerifications = digitalFormVerificationDAL.GetActiveWithMainAccountHolder();

            if (retrievedDigitalFormVerifications != null)
            {
                var groupedById = retrievedDigitalFormVerifications.GroupBy(d => d.Id)
                                                            .Select(g => g.First())
                                                            .ToList();

                GlobalContext.Log.Info($"DigitalFormVerificationsCount: {groupedById.Count}");

                List<alt_DigitalFormVerification> digitalFormVerificationsToUpdate =
                    groupedById.Select(d => MapWithPrimaryAccountHolder(d)).ToList();

                new CommonDAL(GlobalContext).UpdateRetryParallel(digitalFormVerificationsToUpdate);
            }
        }

        private alt_DigitalFormVerification MapWithPrimaryAccountHolder(alt_DigitalFormVerification digitalFormVerification)
        {
            Guid accountHolderId = digitalFormVerification.GetAliasedAttributeValue<Guid>(alt_AccountHolder.EntityLogicalName, alt_AccountHolder.PrimaryIdAttribute);
            var digitalFormVer = new alt_DigitalFormVerification()
            {
                Id = digitalFormVerification.Id,
                alt_PrimaryAccountHolderId = new EntityReference(alt_AccountHolder.EntityLogicalName, accountHolderId)
            };
            return digitalFormVer;
        }

        private void HandleEncouragingDepositSystemUserLogic()
        {
            GlobalContext.LogEntry();

            DigitalFormVerificationDAL digitalFormVerificationDAL = new DigitalFormVerificationDAL(GlobalContext);
            List<alt_DigitalFormVerification> retrievedDigitalFormVerifications = digitalFormVerificationDAL.GetWithEmptyEncouragingDepositSystemUser();

            if (retrievedDigitalFormVerifications != null)
            {
                GlobalContext.Log.Info($"DigitalFormVerificationsCount: {retrievedDigitalFormVerifications.Count}");

                List<alt_DigitalFormVerification> digitalFormVerificationsToUpdate = new List<alt_DigitalFormVerification>();

                foreach (alt_DigitalFormVerification dfv in retrievedDigitalFormVerifications)
                {
                    EntityReference opportunityOwnerId = dfv.GetAliasedAttributeValue<EntityReference>(Opportunity.EntityLogicalName, Opportunity.Fields.OwnerId);
                    if (opportunityOwnerId.LogicalName == SystemUser.EntityLogicalName)
                    {
                        digitalFormVerificationsToUpdate.Add(MapWithEncouragingDepositSystemUser(dfv, opportunityOwnerId));
                    }
                }
                new CommonDAL(GlobalContext).UpdateRetryParallel(digitalFormVerificationsToUpdate);
            }
        }

        private alt_DigitalFormVerification MapWithEncouragingDepositSystemUser(alt_DigitalFormVerification dfv, EntityReference encouragingDepositSystemUserId)
        {
            var digitalFormVer = new alt_DigitalFormVerification()
            {
                Id = dfv.Id,
                alt_EncouragingDepositSystemUserId = encouragingDepositSystemUserId
            };
            return digitalFormVer;
        }


    }
}