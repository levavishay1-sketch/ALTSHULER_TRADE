using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using System;

namespace Alt.DataAccessLayer.Crm
{
    public class EmailDAL : CrmBaseDAL<Email>
    {
        public EmailDAL(GlobalContext globalContext) : base(globalContext, Email.EntityLogicalName) { }

        public SendEmailResponse SendEmail(Guid emailId)
        {
            this.GlobalContext.LogEntry();

            SendEmailRequest sendEmailreq = new SendEmailRequest
            {
                EmailId = emailId,
                TrackingToken = "",
                IssueSend = true
            };

            return (SendEmailResponse)this.Execute(sendEmailreq);
        }
    }
}
