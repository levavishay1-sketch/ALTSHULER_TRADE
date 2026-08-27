namespace Alt.DataModel.Crm.External.Models
{
    public class FetchAttributeInfo
    {
        public string EntityLogicalName { get; set; }
        public string AttributeLogicalName { get; set; }
        public bool IsLinkedEntity { get; set; } = false;
        public string Alias { get; set; }
        public string PrimaryEntityLogicalName { get; set; }
        public string PrimaryEntityAttributeLogicalName { get; set; }
    }
}
