using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class IncidentBusinessProcessFlow
    {
        public List<Stages> stages { get; set; }
    }
    public class Stages
    {
        public string label { get; set; }
        public int? order { get; set; }

        private bool? _isCurrentStep;
        public bool? isCurrentStep
        {
            get { return _isCurrentStep == null ? false : _isCurrentStep; }
            set { _isCurrentStep = value; }
        }
    }
}
