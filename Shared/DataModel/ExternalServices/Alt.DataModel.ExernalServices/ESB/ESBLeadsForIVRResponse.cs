using System.Collections.Generic;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBLeadsForIVRResponse : ExternalEntityBase
    {

        private int statusCode;
        public int StatusCode
        {
            get => statusCode;
            set
            {
                this.SetProperty(value);
                statusCode = value;
            }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                this.SetProperty(value);
                statusMessage = value;
            }
        }

        private int receivedCount;
        public int ReceivedCount
        {
            get => receivedCount;
            set
            {
                this.SetProperty(value);
                receivedCount = value;
            }
        }

        private int successCount;
        public int SuccessCount
        {
            get => successCount;
            set
            {
                this.SetProperty(value);
                successCount = value;
            }
        }

        private int failedCount;
        public int FailedCount
        {
            get => failedCount;
            set
            {
                this.SetProperty(value);
                failedCount = value;
            }
        }

        private List<LeadResult> results;
        public List<LeadResult> Results
        {
            get => results;
            set
            {
                this.SetProperty(value);
                results = value;
            }
        }
    }

    public class LeadResult : ExternalEntityBase
    {
        private string leadId;
        public string LeadId
        {
            get => leadId;
            set
            {
                this.SetProperty(value);
                leadId = value;
            }
        }

        private int loadStatusCode;
        public int LoadStatusCode
        {
            get => loadStatusCode;
            set
            {
                this.SetProperty(value);
                loadStatusCode = value;
            }
        }

        private string loadStatusMessage;
        public string LoadStatusMessage
        {
            get => loadStatusMessage;
            set
            {
                this.SetProperty(value);
                loadStatusMessage = value;
            }
        }
    }
}
