using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Models;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class EmailBL : ExternalBLBase
    {
        public EmailBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult CreateEmailByEmailSettings(EmailSettings emailSettings)
        {
            this.GlobalContext.LogEntry();

            bool isWithAttachments = emailSettings.Attachments != null && emailSettings.Attachments.Count > 0;
            bool isTemplateIncluded = emailSettings.TemplateCodeWithAttachment != null
                || emailSettings.TemplateCode != null 
                || emailSettings.EmailTemplateId != null ? true : false;

            ApiEmail apiEmail = new ApiEmail()
            {               
                RegardingObject = emailSettings.Regarding,
                IsAutomaticBit = isTemplateIncluded,
                TemplateCodeInt = isWithAttachments ? emailSettings.TemplateCodeWithAttachment : emailSettings.TemplateCode,
                EmailTemplateId = emailSettings.EmailTemplateId,
                Subject = emailSettings.Subject,
                From = isTemplateIncluded ? null : emailSettings.Sender,
                Description = isWithAttachments ? emailSettings.DescriptionWithAttachment : emailSettings.Description,
                ParserCustomEntryPoint = emailSettings.ParserCustomEntryPoint
            };
            if (emailSettings.Recipients != null)
            {
                apiEmail.ToActivityPartyList = emailSettings.Recipients;
            }
            if (emailSettings.Related != null)
            {
                apiEmail.Related = emailSettings.Related;
            }

            EmailDAL emailDal = new EmailDAL(this.GlobalContext);
            Guid emailId = emailDal.Create(apiEmail);

            if (isWithAttachments)
            {
                this.AttachDocumets(emailId, emailSettings.Subject, emailSettings.Attachments);
              
            }
            return emailDal.SendEmailHandler(emailId);
        }

        private void AttachDocumets(Guid emailId, string subject, List<DocumentDetails> attachments)
        {
            this.GlobalContext.LogEntry();

            ActivityMimeAttachmentDAL activityMimeAttachmentDAL = new ActivityMimeAttachmentDAL(this.GlobalContext);
            int attachmentNumber = 1;
            foreach (var attachment in attachments)
            {
                ApiActivityMimeAttachment apiActivityMimeAttachment = new ApiActivityMimeAttachment()
                {
                    Subject = subject,
                    FileName = attachment.FileName,
                    MimeType = attachment.MimeType,
                    AttachmentNumber = attachmentNumber,
                    Body = attachment.FileBody,
                    ObjectTypeCode = ApiEmail.EntityLogicalName,
                    ObjectId = new ApiEntity(ApiEmail.EntityLogicalName)
                    {
                        Id = emailId
                    }
                };
                activityMimeAttachmentDAL.Create(apiActivityMimeAttachment);
                attachmentNumber++;
            }
        }
    }
}
