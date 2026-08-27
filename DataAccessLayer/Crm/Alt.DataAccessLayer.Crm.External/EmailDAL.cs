using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm.External
{
    public class EmailDAL: CrmExternalBaseDAL<ApiEmail>
    {
        public EmailDAL(GlobalContext globalContext) : base(globalContext, ApiEmail.EntityLogicalName) { }
        public List<ApiEmail> GetEmailsWithEmptyAttachmentsByDate(DateTime date)
        {
            this.GlobalContext.LogEntry();

            string fetchXML = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                                  "<entity name='email'>" +
                                    "<attribute name='subject' />" +
                                    "<attribute name='activityid' />" +
                                    "<attribute name='regardingobjectid' />" +
                                    "<filter type='and'>" +
                                      "<condition attribute='createdon' operator='on' value='" + date.ToString("yyyy-MM-dd") + "' />" +
                                      "<condition attribute='regardingobjectid' operator='not-null' />" +
                                    "</filter>" +
                                    "<link-entity name='activitymimeattachment' from='objectid' to='activityid' link-type='inner' alias='an'>" +
                                      "<filter type='and'>" +
                                        "<condition attribute='filesize' operator='eq' value='0' />" +
                                          "</filter>" +
                                    "</link-entity>" +
                                  "</entity>" +
                                "</fetch>";
            return base.GetMultiple(new FetchExpression(fetchXML));
        }
        public ActionResult SendEmailHandler(Guid emailIdToSend)
        {
            this.GlobalContext.LogEntry();

            ActionResult apiActionResult = new ActionResult();
            try
            {
                SendEmailResponse response = this.SendEmail(emailIdToSend);
                if (response != null && response.Results != null && response.Results.Count > 0)
                {
                    string result = null;
                    foreach (var responseResult in response.Results)
                    {
                        result += $"{responseResult.Key} : {responseResult.Value?.ToString()}{Environment.NewLine}";
                    }
                    apiActionResult.ReturnObject = result;
                }
            }
            catch (Exception ex)
            {
                apiActionResult.SetToFailedActionResult(ex.Message);
                this.GlobalContext.Log.Error(ex.ToString());
                this.Update(new ApiEmail()
                {
                    Id = emailIdToSend,
                    StateCode = 2, // Canceled
                    StatusCode = (int)EmailStatusCode.Canceled
                });
            }
            return apiActionResult;
        }

        private SendEmailResponse SendEmail(Guid emailId)
        {
            this.GlobalContext.LogEntry();
            SendEmailRequest sendEmailRequest = new SendEmailRequest
            {
                EmailId = emailId,
                TrackingToken = "",
                IssueSend = true
            };
            return (SendEmailResponse)this.Execute(sendEmailRequest);
        }
    }
}
