using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class BankNumberConfiguration
    {
        public Dictionary<string, string> Mapping { get; set; }
        public Dictionary<string, string> Consolidation { get; set; }
    }
}

