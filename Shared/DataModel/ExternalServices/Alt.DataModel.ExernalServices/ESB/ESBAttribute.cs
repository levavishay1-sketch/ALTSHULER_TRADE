
namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBAttribute : ExternalEntityBase
    {
        private string _name;
        public string name
        {
            get
            {
                return this._name;
            }
            set
            {
                this.SetProperty(value);
                this._name = value;
            }
        }

        private object _value;
        public object value
        {
            get
            {
                return this._value;
            }
            set
            {
                this.SetProperty(value);
                this._value = value;
            }
        }
    }
}
