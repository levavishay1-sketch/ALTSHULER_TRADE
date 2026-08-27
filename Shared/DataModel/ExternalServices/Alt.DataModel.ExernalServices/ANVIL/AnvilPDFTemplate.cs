
using System.Collections.Generic;

namespace Alt.DataModel.ExernalServices.ANVIL
{
    public class AnvilPDFTemplateSettings
    {
        public List<AnvilPDFTemplateSetting> customerAgreementTemplates { get; set; }
    }
    public class AnvilPDFTemplateSetting
    {
        public int? Code { get; set; }
        public string TemplateGlobalParameterName { get; set; }
        public string Title { get; set; }
        public int? FontSize { get; set; }
        public string TextColor { get; set; }
        public string FileName { get; set; }
    }
}
