using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    public class InlineCondition
    {
        public string Operator { get; set; }
        public string ConditionLeftSideKey { get; set; }
        public string ConditionRightSideKey { get; set; }
        public bool ConditionResult { get; set; }

        public static readonly ConcurrentBag<string> ConditionOperators = new ConcurrentBag<string> { "==", "!=", "<", ">=", "<=" };

        public bool GetInlineConditionResult()
        {
            bool result = false;
            switch (this.Operator)
            {
                case "==":
                    {
                        result = this.ConditionRightSideKey == this.ConditionLeftSideKey;
                        break;
                    }
                case "!=":
                    {
                        result = this.ConditionRightSideKey != this.ConditionLeftSideKey;
                        //? this.ConditionResultKey : this.ConditionElseResultKey;
                        break;
                    }
                //case ">":
                //    {
                //        result = decimal.Parse(this.ConditionRightSideKey) > decimal.Parse(this.ConditionLeftSideKey);
                //        break;
                //    }
                case "<":
                    {
                        result = decimal.Parse(this.ConditionRightSideKey) < decimal.Parse(this.ConditionLeftSideKey);
                        break;
                    }
                case "<=":
                    {
                        result = decimal.Parse(this.ConditionRightSideKey) <= decimal.Parse(this.ConditionLeftSideKey);
                        break;
                    }
                case ">=":
                    {
                        result = decimal.Parse(this.ConditionRightSideKey) >= decimal.Parse(this.ConditionLeftSideKey);
                        break;
                    }
            }
            return result;
        }

        public static List<string> ExtractCondtionsConditionInlineText(string text)
        {
            List<string> subCondtions = new List<string>();
            if (text.Contains("&&") || text.Contains("||"))
            {
                string splitCondtion = text.Contains("&&") ? "&&" : "||";
                subCondtions = text.Split(new string[] { splitCondtion, "?" }, StringSplitOptions.None).Select(t => t.Trim()).ToList();
                if (subCondtions.Count == 3)
                {
                    subCondtions.RemoveAt(subCondtions.Count - 1);
                }

            }
            else
            {
                List<string> splitCondition = text.Split('?').Select(t => t.Trim()).ToList();
                subCondtions.Add(splitCondition[0]);
            }

            return subCondtions;
        }

        public static string ExtractOperatorFromConditionText(string text)
        {
            string operationText = null;
            var conditionOperators = InlineCondition.ConditionOperators;
            foreach (var conditionOperator in conditionOperators)
            {
                if (text.Contains(conditionOperator))
                {
                    operationText = conditionOperator;
                    break;
                }
            }
            return operationText;
        }
    }
}
