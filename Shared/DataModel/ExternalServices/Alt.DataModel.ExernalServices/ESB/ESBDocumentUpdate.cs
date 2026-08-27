namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBDocumentUpdate : ExternalEntityBase
    {
        private string openTextID;
        public string OpenTextID
        {
            get => openTextID;
            set
            {
                this.SetProperty(value);
                openTextID = value;
            }
        }

        private string customerID;
        public string CustomerID
        {
            get => customerID;
            set
            {
                this.SetProperty(value);
                customerID = value;
            }
        }

        private string productCode;
        public string ProductCode
        {
            get => productCode;
            set
            {
                this.SetProperty(value);
                productCode = value;
            }
        }

        private string productDesc;
        public string ProductDesc
        {
            get => productDesc;
            set
            {
                this.SetProperty(value);
                productDesc = value;
            }
        }

        private string processCode;
        public string ProcessCode
        {
            get => processCode;
            set
            {
                this.SetProperty(value);
                processCode = value;
            }
        }

        private string processDesc;
        public string ProcessDesc
        {
            get => processDesc;
            set
            {
                this.SetProperty(value);
                processDesc = value;
            }
        }

        private string docType;
        public string DocType
        {
            get => docType;
            set
            {
                this.SetProperty(value);
                docType = value;
            }
        }

        private string publish;
        public string Publish
        {
            get => publish;
            set
            {
                this.SetProperty(value);
                publish = value;
            }
        }

        private string docDate;
        public string DocDate
        {
            get => docDate;
            set
            {
                this.SetProperty(value);
                docDate = value;
            }
        }

        private string agentName;
        public string AgentName
        {
            get => agentName;
            set
            {
                this.SetProperty(value);
                agentName = value;
            }
        }
    }
}
