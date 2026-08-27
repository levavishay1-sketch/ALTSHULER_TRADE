using Alt.DataModel.ExernalServices;

namespace Alt.DataModel.ExternalServices.ESB
{
    public class ESBFeezbackLinkRequest : ExternalEntityBase
    {
        private string contactIdNumber;
        public string ContactIdNumber
        {
            get => contactIdNumber;
            set
            {
                this.SetProperty(value);
                contactIdNumber = value;
            }
        }

        private string productId;
        public string ProductId
        {
            get => productId;
            set
            {
                this.SetProperty(value);
                productId = value;
            }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set
            {
                this.SetProperty(value);
                firstName = value;
            }
        }


        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                this.SetProperty(value);
                lastName = value;
            }
        }
        private string email;
        public string Email
        {
            get => email;
            set
            {
                this.SetProperty(value);
                email = value;
            }
        }

        private string phone;
        public string Phone
        {
            get => phone;
            set
            {
                this.SetProperty(value);
                phone = value;
            }
        }

        private string agentId;
        public string AgentId
        {
            get => agentId;
            set
            {
                this.SetProperty(value);
                agentId = value;
            }
        }

        private string productAccountNumber;
        public string ProductAccountNumber
        {
            get => productAccountNumber;
            set
            {
                this.SetProperty(value);
                productAccountNumber = value;
            }
        }

        private string bankCode;
        public string BankCode
        {
            get => bankCode;
            set
            {
                this.SetProperty(value);
                bankCode = value;
            }
        }

        private string bankName;
        public string BankName
        {
            get => bankName;
            set
            {
                this.SetProperty(value);
                bankName = value;
            }
        }

        private string bankAccountNumber;
        public string BankAccountNumber
        {
            get => bankAccountNumber;
            set
            {
                this.SetProperty(value);
                bankAccountNumber = value;
            }
        }

        private string bankBranchNumber;
        public string BankBranchNumber
        {
            get => bankBranchNumber;
            set
            {
                this.SetProperty(value);
                bankBranchNumber = value;
            }
        }

        private string bankBranchName;
        public string BankBranchName
        {
            get => bankBranchName;
            set
            {
                this.SetProperty(value);
                bankBranchName = value;
            }
        }

        private string bankId;
        public string BankId
        {
            get => bankId;
            set
            {
                this.SetProperty(value);
                bankId = value;
            }
        }

        private string amount;
        public string Amount
        {
            get => amount;
            set
            {
                this.SetProperty(value);
                amount = value;
            }
        }

        private string companyId;
        public string CompanyId
        {
            get => companyId;
            set
            {
                this.SetProperty(value);
                companyId = value;
            }
        }

        private string source;
        public string Source
        {
            get => source;
            set
            {
                this.SetProperty(value);
                source = value;
            }
        }
    }
}
