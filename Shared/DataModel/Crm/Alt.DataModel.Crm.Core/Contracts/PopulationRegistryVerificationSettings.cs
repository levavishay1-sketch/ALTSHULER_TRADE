using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class PopulationRegistryVerificationSettings
    {
        public List<Configuration> configurations { get; set; }
    }

    public class Configuration
    {
        public int? CompanyCode { get; set; }
        public bool? CreateContactIfNotExist { get; set; }
        public bool? UpdateContactIfVerified { get; set; }
        public List<DefinitionsByEntity> DefinitionsByEntity { get; set; }
        public List<string> DefaultAttributesToDisplay { get; set; }
    }

    public class DefinitionsByEntity
    {
        public string LogicalName { get; set; }
        public Dictionary<string, string> AttributesToCompare { get; set; }
        public List<string> AttributesToDisplay { get; set; }
    }
}
