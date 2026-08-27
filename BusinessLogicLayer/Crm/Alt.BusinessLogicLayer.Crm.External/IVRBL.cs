using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class IVRBL : ExternalBLBase
    {
        private string successCRMUpdateMessage = "עדכון ב-CRM בוצע בהצלחה";
        private string leadsSentToIVR = "סך הכל לידים שהתקבלו לחייגן: ";
        private string leadsReceivedByIVR = "סך הכל לידים שנטענו לחייגן: ";
        private string leadsFailedToReceiveByIVR = "סך הכל לידים שנכשלו בטעינה לחייגן: ";

        public IVRBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult HandleIVRResponse(ESBLeadsForIVRResponse response)
        {
            this.GlobalContext.LogEntry();

            ActionResult actionResult = new ActionResult();
            StringBuilder stringBuilder = new StringBuilder();

            this.CreateInitialStatusMessage(stringBuilder, response);
            List<ApiLead> leadsToUpdate = this.ProcessLeadsResponsesFromIVR(stringBuilder, response);
            this.HandleLeadsUpdate(stringBuilder, leadsToUpdate);
            actionResult.ReturnObject = stringBuilder.ToString();

            return actionResult;
        }

        private void CreateInitialStatusMessage(StringBuilder stringBuilder, ESBLeadsForIVRResponse response)
        {
            this.GlobalContext.LogEntry();

            this.GlobalContext.Log.Info("message: " + response.StatusMessage);
            this.GlobalContext.Log.Info("sent count: " + response.ReceivedCount);
            this.GlobalContext.Log.Info("received count: " + response.SuccessCount);
            this.GlobalContext.Log.Info("failed count: " + response.FailedCount);

            stringBuilder.AppendLine(response.StatusMessage);
            stringBuilder.Append(leadsSentToIVR).AppendLine(response.ReceivedCount.ToString());
            stringBuilder.Append(leadsReceivedByIVR).AppendLine(response.SuccessCount.ToString());
            stringBuilder.Append(leadsFailedToReceiveByIVR).AppendLine(response.FailedCount.ToString());
        }

        private List<ApiLead> ProcessLeadsResponsesFromIVR(StringBuilder stringBuilder, ESBLeadsForIVRResponse response)
        {
            this.GlobalContext.LogEntry();

            List<ApiLead> leadsToUpdate = new List<ApiLead>();
            foreach (var leadIVR in response.Results)
            {
                if (leadIVR.LoadStatusCode == 0)
                {
                    if (Guid.TryParse(leadIVR.LeadId, out Guid leadId))
                    {
                        leadsToUpdate.Add(new ApiLead
                        {
                            Id = leadId,
                            SentToIVRBit = true,
                            IVRCampaignCode = 1
                        });
                    }
                    else
                    {
                        stringBuilder
                            .Append("מזהה ליד לא תקין: ")
                            .Append(leadIVR.LeadId)
                            .AppendLine();
                    }
                }
                else
                {
                    stringBuilder.Append("ליד עם המזהה: ")
                        .Append(leadIVR.LeadId)
                        .Append(" נכשל בטעינה לחייגן, סיבת הכישלון: ")
                        .Append(leadIVR.LoadStatusMessage)
                        .AppendLine();
                }
            }

            return leadsToUpdate;
        }

        private void HandleLeadsUpdate(StringBuilder stringBuilder, List<ApiLead> leadsToUpdate)
        {
            this.GlobalContext.LogEntry();

            if (leadsToUpdate.Count > 0)
            {
                LeadDAL leadDAL = new LeadDAL(this.GlobalContext);
                ActionResult updateResult = leadDAL.ExecuteMultipleRequestsInChunks(leadsToUpdate, RequestType.Update, 10);
                if (updateResult.IsSuccess)
                {
                    stringBuilder.AppendLine(successCRMUpdateMessage);
                }
                else
                {
                    stringBuilder.AppendLine(updateResult.ReturnObject.ToString());
                }
            }
        }
    }
}
