
namespace Alt.DataModel.Crm.Contracts
{
    public class TemplateGeneratorDTO
    {
        public string CrmUrlInput { get; set; }
        public string TempLateIdInput { get; set; }
        public int? TemplateTypeInput { get; set; }
        public string RegardingObjectIdInput { get; set; }
        public string RegardingObjectEntityNameInput { get; set; }
        public string DescriptionMessageOutput { get; set; }
        public string SubjectTemplateMessageOutput { get; set; }
        public bool IsSucceededOutput { get; set; }

        public override string ToString()
        {
            return $@"TempLateIdInput : {TempLateIdInput}, TemplateTypeInput:{TemplateTypeInput}, RegardingObjectIdInput:{RegardingObjectIdInput}, RegardingObjectEntityNameInput:{RegardingObjectEntityNameInput} 
                DescriptionMessageOutput : {DescriptionMessageOutput}, SubjectTemplateMessageOutput:{SubjectTemplateMessageOutput}, IsSucceededOutput: {IsSucceededOutput}";
        }
    }
}
