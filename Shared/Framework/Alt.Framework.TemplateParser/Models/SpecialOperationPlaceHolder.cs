using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    public class SpecialOperationPlaceHolder
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public string Content { get; set; }

        public bool IsValidToParse { get; set; } = true;

        public bool IsTablePlaceHolder { get; set; } = false;

        public SpecialOperationBase SpecialOperation { get; set; }


        public SpecialOperationPlaceHolder(SpecialOperationBase specialOperationType)
        {
            this.SpecialOperation = specialOperationType;
            this.Prefix = specialOperationType.Prefix;
            this.Suffix = specialOperationType.Suffix;
            this.IsTablePlaceHolder = specialOperationType.SpecialOperationType == SpecialOperationType.LinkEntityPlaceHolder;
        }

        public SpecialOperationPlaceHolder(SpecialOperationPlaceHolder specialOperationPlaceHolder, string content)
        {
            this.Prefix = specialOperationPlaceHolder.Prefix;
            this.Suffix = specialOperationPlaceHolder.Suffix;
            this.IsTablePlaceHolder = specialOperationPlaceHolder.IsTablePlaceHolder;
            this.IsValidToParse = specialOperationPlaceHolder.IsValidToParse;
            this.Content = content;
            this.SpecialOperation = specialOperationPlaceHolder.SpecialOperation;

        }
    }
}
