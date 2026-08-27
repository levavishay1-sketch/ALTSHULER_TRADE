using Alt.DataModel.Crm.External.Contracts;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.External.Models
{
    public class EmailSettings
    {
        public string Subject { get; set; }
        public string DescriptionWithAttachment { get; set; }
        public string Description { get; set; }
        public List<ApiActivityParty> Recipients { get; set; }
        public List<ApiActivityParty> Related { get; set; }
        public ApiActivityParty Sender { get; set; }
        public int? TemplateCodeWithAttachment { get; set; }
        public int? TemplateCode { get; set; }
        public ApiEntity EmailTemplateId { get; set; }
        public ApiEntity Regarding { get; set; }
        public List<DocumentDetails> Attachments { get; set; }
        public string ParserCustomEntryPoint { get; set; }
        public bool? SendOnEmptyResult { get; set; }
    }
}
