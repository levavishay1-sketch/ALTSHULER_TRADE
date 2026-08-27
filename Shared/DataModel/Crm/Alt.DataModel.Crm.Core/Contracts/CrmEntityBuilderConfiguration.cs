using Alt.DataModel.Crm.Core.Enums;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class CrmEntityBuilderConfiguration
    {
        public RecordMatchingCriteriaCode? RecordMatchtchingCriteria { get; set; }
        public List<string> RecordAlternateKeyAttributes { get; set; }
        public List<TextLookupSetting> TextLookups { get; set; }
        public bool? CreateWithNoKey { get; set; }
        public bool? Create { get; set; }
        public bool? Update { get; set; }
        public bool? Upsert { get; set; }
        public bool? AddSourceSystemCode { get; set; }
        public bool? CreationMethodCode { get; set; }
        public bool? MappOnlyDelta { get; set; }
        public List<string> ListOfAttributesToUpdate { get; set; }
        public int? ChunkSize { get; set; }
        public int? ThreadsCount { get; set; }
        public bool? ExecuteMultipleRequests { get; set; }
        public string DataFlowName { get; set; }
    }

    public class TextLookupSetting
    {
        public string AttributeName { get; set; }
        public string TargetEntity { get; set; }
        public string TargetField { get; set; }
        public int? TargetFieldTypeCode { get; set; }
    }
}
