using Alt.DataModel.Crm.Core.Contracts;
using System;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External.Models
{
    public class ImportDataMappingConfiguration
    {
        public string FilePrefix { get; set; }
        public string ApiEntityName { get; set; }
        public string CrmEntityName { get; set; }
        public int ExecutionOrder { get; set; }
        public CrmEntityBuilderConfiguration EntityBuilderConfiguration { get; set; }
        public List<string> PropertiesToIgnoreInMapping { get; set; }
    }

    public class ImportDataMappingConfigurationComparer : IComparer<ImportDataMappingConfiguration>
    {
        public int Compare(ImportDataMappingConfiguration x, ImportDataMappingConfiguration y)
        {
            return x.ExecutionOrder.CompareTo(y.ExecutionOrder);
        }
    }
}
