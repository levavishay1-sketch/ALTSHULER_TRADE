
namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBPortfolioStatementsMaildef : ExternalEntityBase
    {
        private string isPost;
        public string IsPost
        {
            get => this.isPost;
            set
            {
                base.SetProperty(value);
                this.isPost = value;
            }
        }

        private string isEmail;
        public string IsEmail
        {
            get => this.isEmail;
            set
            {
                base.SetProperty(value);
                this.isEmail = value;
            }
        }

        private string statementType;
        public string StatementType
        {
            get => this.statementType;
            set
            {
                base.SetProperty(value);
                this.statementType = value;
            }
        }
    }
}
