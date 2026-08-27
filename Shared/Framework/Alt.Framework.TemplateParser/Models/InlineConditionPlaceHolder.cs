using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    public class InlineConditionPlaceHolder
    {
        public List<InlineCondition> InlineConditions { get; set; } = new List<InlineCondition>();
        public string OriginalInlineConditionPlaceHolder { get; set; }
        public string ConditionsThenOperationtKey { get; set; }
        public string ConditionsElseOperationtKey { get; set; }

        public bool IsAllSubPlaceHoldersParsed { get; set; } = false;
    }
}
