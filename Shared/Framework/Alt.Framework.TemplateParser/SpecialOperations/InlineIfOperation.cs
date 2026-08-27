using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using Alt.Framework.TemplateParser.ValueResolvers;
using System.Linq.Expressions;
using System.Data;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class InlineIfOperation : SpecialOperationBase
    {
        private PDFReportValueResolver valueResolver = new PDFReportValueResolver();

        public InlineIfOperation(string prefix, string suffix) : base(prefix, suffix, SpecialOperationType.RegualrWithInlineOperation)
        {

        }

        public override string GetTextContentFromSpecialOperationPattern(string text, string prefix, string suffix)
        {
            string result = null;
            if (text.Contains("?") && text.Contains(":") && !text.Contains("OTM("))
            {
                int conditionIfKeyPosition = text.IndexOf("?");
                int conditionElseKeyPosition = text.IndexOf(":");

                List<string> inlineCondtionsText = InlineCondition.ExtractCondtionsConditionInlineText(text);
                InlineConditionPlaceHolder inlineConditionPlaceHolder = new InlineConditionPlaceHolder();
                foreach (var inlineCondtionText in inlineCondtionsText)
                {
                    inlineConditionPlaceHolder.InlineConditions.Add(this.GetTextContentFromInlineOperationPattern(inlineCondtionText));
                }
                inlineConditionPlaceHolder.ConditionsThenOperationtKey = text.Substring(conditionIfKeyPosition, conditionElseKeyPosition - conditionIfKeyPosition).Replace("?", "").Trim();
                inlineConditionPlaceHolder.ConditionsElseOperationtKey = text.Substring(conditionElseKeyPosition, text.Length - conditionElseKeyPosition).Replace(":", "").Trim();

                inlineConditionPlaceHolder.OriginalInlineConditionPlaceHolder = text;
                result = JsonSerializer.Serialize(inlineConditionPlaceHolder).Replace("\\u003E", ">");
            }
            return result;
        }

        private InlineCondition GetTextContentFromInlineOperationPattern(string text)
        {
            string operatorText = InlineCondition.ExtractOperatorFromConditionText(text);
            InlineCondition inlineCondition = new InlineCondition();
            //{{alt_foreigntaxresidencybit}} == 1 ? {{alt_lastname}} : ""
            int operatorPosition = text.IndexOf(operatorText);
            int lastPlaceHolderPosition = text.LastIndexOf("{{");
            int rightSideStartPostion = lastPlaceHolderPosition == 0 ? operatorPosition : lastPlaceHolderPosition;

            inlineCondition.ConditionLeftSideKey = text.Substring(0, operatorPosition).Trim();
            inlineCondition.ConditionRightSideKey = text.Substring(rightSideStartPostion, text.Length - rightSideStartPostion).Replace(operatorText, "").Trim();
            inlineCondition.Operator = operatorText;

            inlineCondition.ConditionLeftSideKey = !inlineCondition.ConditionLeftSideKey.Contains("{{")
            && (inlineCondition.ConditionLeftSideKey?.ToLower() == "null"
                || inlineCondition.ConditionLeftSideKey?.ToLower() == "\"\"")
                ? string.Empty : inlineCondition.ConditionLeftSideKey;

            inlineCondition.ConditionRightSideKey = !inlineCondition.ConditionRightSideKey.Contains("{{")
       && (inlineCondition.ConditionRightSideKey?.ToLower() == "null"
           || inlineCondition.ConditionRightSideKey?.ToLower() == "\"\"")
           ? string.Empty : inlineCondition.ConditionRightSideKey;

            return inlineCondition;
        }

        public override string ExecuteSpecialOperationLogic(Entity entity, string key, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            string result = null;

            InlineConditionPlaceHolder inlineConditionPlaceHolder = JsonSerializer.Deserialize<InlineConditionPlaceHolder>(specialOperationPlaceHolder.Content);
            StringBuilder stringBuilder = new StringBuilder();
            string conditionsSeperator = null;
            List<InlineCondition> inlineConditionList = inlineConditionPlaceHolder.InlineConditions;
            for (int i = 0; i < inlineConditionList.Count; i++)
            {
                string placrHolder = ConvertToOriginalPlaceHolder(key);
                HandleParsePlaceHolderConditionSides(entity, key, placrHolder, inlineConditionList[i]);
                if (inlineConditionPlaceHolder.ConditionsThenOperationtKey == $"{{{{{placrHolder}}}}}")
                {
                    inlineConditionPlaceHolder.ConditionsThenOperationtKey = inlineConditionPlaceHolder.ConditionsThenOperationtKey.Replace($"{{{{{placrHolder}}}}}", valueResolver.GetAttributeValue(key, entity)); ;
                }

                if (inlineConditionPlaceHolder.ConditionsElseOperationtKey == $"{{{{{placrHolder}}}}}")
                {
                    inlineConditionPlaceHolder.ConditionsElseOperationtKey = inlineConditionPlaceHolder.ConditionsElseOperationtKey.Replace($"{{{{{placrHolder}}}}}", valueResolver.GetAttributeValue(key, entity)); ;
                }
                stringBuilder.Append($"{inlineConditionList[i].ConditionLeftSideKey}{inlineConditionList[i].Operator}{inlineConditionList[i].ConditionRightSideKey}");
                if (i == 0)
                {
                    //
                    conditionsSeperator = inlineConditionPlaceHolder.OriginalInlineConditionPlaceHolder.Contains("&&") ? "&&" : "||";
                    stringBuilder.Append(conditionsSeperator);
                }
            }

            stringBuilder.Append($"{inlineConditionPlaceHolder.ConditionsThenOperationtKey}{inlineConditionPlaceHolder.ConditionsElseOperationtKey}");
            string conditionStr = stringBuilder.ToString();
            result = conditionStr.Contains("{{") ? specialOperationPlaceHolder.Content : this.GetInlineConditionPlaceHolderResult(inlineConditionPlaceHolder, conditionsSeperator);
            specialOperationPlaceHolder.Content = JsonSerializer.Serialize(inlineConditionPlaceHolder);

            return result;
        }

        private string GetInlineConditionPlaceHolderResult(InlineConditionPlaceHolder inlineConditionPlaceHolder, string conditionsSeperator)
        {
            List<bool> inlineConditionResultList = new List<bool>();
            List<InlineCondition> inlineConditionList = inlineConditionPlaceHolder.InlineConditions;
            foreach (var inlineCondition in inlineConditionList)
            {
                inlineConditionResultList.Add(inlineCondition.GetInlineConditionResult());
            }

            bool finalCondtionResult = inlineConditionPlaceHolder.OriginalInlineConditionPlaceHolder.Contains("&&")
                ? inlineConditionResultList.Where(c => c).ToList().Count == inlineConditionResultList.Count
                : inlineConditionResultList.Where(c => c).ToList().Count > 0;
            string result = finalCondtionResult ? inlineConditionPlaceHolder.ConditionsThenOperationtKey : inlineConditionPlaceHolder.ConditionsElseOperationtKey;

            inlineConditionPlaceHolder.IsAllSubPlaceHoldersParsed = true;

            return result;
        }

        private void HandleParsePlaceHolderConditionSides(Entity entity, string key, string placrHolder, InlineCondition inlineCondition)
        {
            if (entity.Contains(key))
            {
                if (inlineCondition.ConditionLeftSideKey == $"{{{{{placrHolder}}}}}")
                {
                    inlineCondition.ConditionLeftSideKey = inlineCondition.ConditionLeftSideKey.Replace($"{{{{{placrHolder}}}}}", valueResolver.GetAttributeValue(key, entity));
                }

                if (inlineCondition.ConditionRightSideKey == $"{{{{{placrHolder}}}}}")
                {
                    inlineCondition.ConditionRightSideKey = inlineCondition.ConditionRightSideKey.Replace($"{{{{{placrHolder}}}}}", valueResolver.GetAttributeValue(key, entity)); ;
                }
            }
        }



        protected virtual string ConvertToOriginalPlaceHolder(string attributeKey)
        {
            string[] placeholder = attributeKey.Split('.');
            var builder = new StringBuilder();
            if (placeholder.Length == 1)
            {
                builder.Append(placeholder[0]);
            }
            else
            {
                builder.Append(placeholder[0]);
                for (int i = 1; i < placeholder.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        builder.Append(".").Append(placeholder[i]);
                    }
                    else
                    {
                        builder.Append(">").Append(placeholder[i]);

                    }
                }
            }
            return builder.ToString();
        }

    }
}
