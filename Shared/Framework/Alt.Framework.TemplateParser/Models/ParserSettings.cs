using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using System.Collections.Generic;

namespace Alt.Framework.TemplateParser.Models
{
    public class ParserSettings
    {
        public string ValueToParseInEmptyOrInvalidPlaceHolders { get; set; } = "";

        public string RegardingObjectEntityLogicalName { get; set; }

        public string MessageToParse { get; set; }

        public string RegardingObjectId { get; set; }

        public string CrmUrl { get; set; }

        public bool RemoveHtmlTagsInRegularPlaceHolders { get; set; } = false;
        
        public bool DecodeMssageForHtmlParsing { get; set; } = false;

        public IEntityValueResolver EntityValueResolver { get; set; }

        public List<SpecialOperationBase> SpecialOperations { get; set; }
    }
}
