using System.Text.Json;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class SearchEntryPointObject
    {
        public int SearchType { get; set; }
        public string SearchTables { get; set; }
        public string SearchField { get; set; }
        public string SearchInput { get; set; }
        public string EntityLogicalName { get; set; }
        public string EntityId { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
