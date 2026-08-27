using System.Collections.Generic;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBSms : ExternalEntityBase
    {
        private string from;
        public string From
        {
            get => from;
            set
            {
                this.SetProperty(value);
                from = value;
            }
        }

        private List<string> to;
        public List<string> To
        {
            get => to;
            set 
            {
                this.SetProperty(value);
                to = value; 
            }
        }

        private string text;
        public string Text { 
            get => text;
            set
            {
                this.SetProperty(value);
                text = value;
            }
        }
    }
}
