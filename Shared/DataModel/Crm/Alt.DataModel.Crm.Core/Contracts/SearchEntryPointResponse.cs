using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class SearchEntryPointResponse
    {
        public string Columns { get; set; }

        public List<ItemGroup> ItemGroups { get; set; }

        public string EntityName { get; set; }

        public string EntitySchemaName { get; set; }
    }

    public class ItemGroup
    {
        public string EntityName { get; set; }

        public string EntitySchemaName { get; set; }

        public List<Entity> Items { get; set; }
    }
}
