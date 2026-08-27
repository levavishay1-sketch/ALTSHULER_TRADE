using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    public enum SpecialOperationType
    {
        Regualr,
        LinkEntityPlaceHolder,
        RegualrWithInlineOperation
    }

    public abstract class SpecialOperationBase
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }

        //public bool IsLinkEntityPlaceHolder { get; set; } = false;
        public SpecialOperationType SpecialOperationType { get; set; } = SpecialOperationType.Regualr;

        public int? numberOfInputParameterInContent = null;

        public Func<QueryBase, IEnumerable<Entity>> ExecuteQueryFunc { get; set; }

        public SpecialOperationBase(string prefix, string suffix, SpecialOperationType specialOperationType = SpecialOperationType.Regualr)
        {
            this.Prefix = prefix;
            this.Suffix = suffix;
            this.SpecialOperationType = specialOperationType;
        }

        public SpecialOperationBase(SpecialOperationBase specialOperation)
        {
            this.Prefix = specialOperation.Prefix;
            this.Suffix = specialOperation.Suffix;
            this.SpecialOperationType = specialOperation.SpecialOperationType;
        }

        public abstract string ExecuteSpecialOperationLogic(Entity entity, string key, SpecialOperationPlaceHolder specialOperationPlaceHolder);

        public virtual string GetTextContentFromSpecialOperationPattern(string text, string prefix, string suffix)
        {
            int position1 = text.IndexOf(this.Prefix);
            int position2 = text.LastIndexOf(this.Suffix);
            if (position1 == -1 || position2 == -1)
            {
                return null;
            }

            position1 += this.Prefix.Length;
            return text.Substring(position1, position2 - position1);
        }

        public virtual string ReplaceResultInOriginalMessage(string wholeMessageWithOriginalPlaceHolder, string placeHolderWithPrefixAndSuffix, SpecialOperationPlaceHolder specialOperationPlaceHolder, string value)
        {
            return wholeMessageWithOriginalPlaceHolder.Replace(placeHolderWithPrefixAndSuffix, value);//replace the special placeHolder
        }
    }
}
