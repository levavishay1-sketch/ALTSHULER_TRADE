using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;

namespace Alt.BusinessLogicLayer.Crm
{
    public class ContactBL : CrmBaseBL
    {
        public ContactBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public void ValidateContact(Contact targetContact, Contact preContact)
        {
            this.GlobalContext.LogEntry();
            Contact mergedTargetContact = targetContact.Merge(preContact);
        }

        public bool IsPassedAway(Guid contanctId)
        {
            this.GlobalContext.LogEntry();

            // Need to implement
            return false;
        }

        public void SetInternalGovernmentIdHandler(Contact targetContact)
        {
            if (targetContact.Contains(Contact.Fields.GovernmentId))
            {
                targetContact.GovernmentId = !string.IsNullOrWhiteSpace(targetContact.GovernmentId) ? targetContact.GovernmentId : null;
                targetContact.alt_InternalGovernmentId = targetContact.GovernmentId?.GetPadedLeftZeroString();
            }
        }
    }
}
