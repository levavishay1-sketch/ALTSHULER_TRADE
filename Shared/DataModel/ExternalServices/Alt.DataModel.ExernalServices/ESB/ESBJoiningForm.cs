using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBJoiningForm : ExternalEntityBase
    {
        private Header header;
       // [Required]
        public Header Header
        {
            get => this.header;
            set
            {
                base.SetProperty(value);
                this.header = value;
            }
        }

        private ESBPortfolioBody body;
        [Required]
        public ESBPortfolioBody Body
        {
            get => this.body;
            set
            {
                base.SetProperty(value);
                this.body = value;
            }
        }
    }
}
