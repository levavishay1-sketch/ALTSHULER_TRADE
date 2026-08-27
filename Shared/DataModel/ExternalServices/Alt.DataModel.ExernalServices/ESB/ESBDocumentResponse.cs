using System.Collections.Generic;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBDocumentResponse : ExternalEntityBase
    {
        private string docBase64;
        public string DocBase64
        {
            get => docBase64;
            set
            {
                this.SetProperty(value);
                docBase64 = value;
            }
        }

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

        private List<ESBDocumentMetaData> searchResults;
        public List<ESBDocumentMetaData> SearchResults
        {
            get => searchResults;
            set
            {
                this.SetProperty(value);
                searchResults = value;
            }
        }
    }

    public class ESBDocumentMetaData : ExternalEntityBase
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

        private string customerFirstName;
        public string CustomerFirstName
        {
            get => customerFirstName;
            set
            {
                this.SetProperty(value);
                customerFirstName = value;
            }
        }

        private string customerLastName;
        public string CustomerLastName
        {
            get => customerLastName;
            set
            {
                this.SetProperty(value);
                customerLastName = value;
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

        private string systemCode;
        public string SystemCode
        {
            get => systemCode;
            set
            {
                this.SetProperty(value);
                systemCode = value;
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

        private string fileName;
        public string FileName
        {
            get => fileName;
            set
            {
                this.SetProperty(value);
                fileName = value;
            }
        }
    }
}
