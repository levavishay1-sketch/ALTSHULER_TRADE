using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.External.Models
{
    public class CSVExportExtraColumns
    {
        public List<EntityInfo> Entities { get; set; }
    }

    public class EntityInfo
    {
        public string LogicalName { get; set; }

        public List<string> Columns { get; set; }
    }
}
