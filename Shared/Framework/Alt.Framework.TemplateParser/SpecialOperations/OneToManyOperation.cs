using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.ParserEngine;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class OneToManyOperation : SpecialOperationBase, ILinkEntityOperation
    {
        private PDFReportValueResolver valueResolver = new PDFReportValueResolver();
        public OneToManyOperation(string prefix, string suffix) : base(prefix, suffix, SpecialOperationType.LinkEntityPlaceHolder) { }

        /*@{OTM(<span>parentcustomerid></span>account.primarycontactid,incident.contactid,[title, customerid>contact.parentcustomerid>account.name],
        case: title:{{title}}, <span>relatedAccount:</span> {{customerid>contact.parentcustomerid>account.name}}" + Environment.NewLine + @")}@*/
        public override string ExecuteSpecialOperationLogic(Entity entity, string key, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            specialOperationPlaceHolder.Content = specialOperationPlaceHolder.Content.Last() != ')' &&
                (specialOperationPlaceHolder.Content.Contains(".Where(")
                || specialOperationPlaceHolder.Content.Contains(".OrderByDesc(")
                || specialOperationPlaceHolder.Content.Contains(".OrderBy(")
                )
                    ? $"{specialOperationPlaceHolder.Content})"
                        : specialOperationPlaceHolder.Content;

            CustomLinkEntity customEntity = this.HandleCreateCustomLinkEntitiesByLinkEntityPlaceHolders(specialOperationPlaceHolder);
            QueryExpressionParser queryExpressionParser = new QueryExpressionParser(customEntity as CustomLinkEntity);
            EntityReference attribute = entity[key] as EntityReference;
            if (entity[key].GetType().Name == "AliasedValue")
            {
                var aliasedValue = ((AliasedValue)entity[key])?.Value;
                attribute = aliasedValue as EntityReference;
            }
            else if (entity[key].GetType().Name.ToLower() == "guid")
            {
                attribute = new EntityReference(entity.LogicalName, entity.Id);
            }

            var query = queryExpressionParser.ConvertTableToQueryExpression(customEntity, attribute.Id);
            var queryResult = base.ExecuteQueryFunc(query);
            //string tableTemplate = this.ExtractOperationTemplateResultPattern(StripHTML(specialOperationPlaceHolder.Content));
            string tableTemplate = this.ExtractOperationTemplateResultPattern(StripHtmlInBraces(specialOperationPlaceHolder.Content));
            string tableTemplateWithoutSubFunction = this.GetTableTemplateWithoutSubFunction(tableTemplate);
            queryResult = FilterByWhereSubFunction(queryResult, tableTemplate);
            queryResult = OrderByResult(queryResult, tableTemplate);
            string parseTableReturnTemplate = this.ParseOperationTemplateResultPattern(queryResult, tableTemplateWithoutSubFunction) ?? string.Empty;
            if (specialOperationPlaceHolder.Content.Contains(".Where(")
                || specialOperationPlaceHolder.Content.Contains(".OrderByDesc(")
               || specialOperationPlaceHolder.Content.Contains(".OrderBy("))
            {
                specialOperationPlaceHolder.Content = specialOperationPlaceHolder.Content.Remove(specialOperationPlaceHolder.Content.Length - 1);
            }
            return parseTableReturnTemplate;
        }

        private IEnumerable<Entity> FilterByWhereSubFunction(IEnumerable<Entity> queryResult, string tableTemplate)
        {
            List<Entity> result = new List<Entity>();
            if (tableTemplate.Contains(".Where("))
            {
                int indexOfSubeFunction = tableTemplate.LastIndexOf(".Where(");
                int endIndexOfSubeFunction = tableTemplate.Length - indexOfSubeFunction;
                if (tableTemplate.Contains("OrderByDesc"))
                {
                    endIndexOfSubeFunction = tableTemplate.LastIndexOf(".OrderByDesc") - indexOfSubeFunction;
                }
                else if (tableTemplate.Contains("OrderBy"))
                {
                    endIndexOfSubeFunction = tableTemplate.LastIndexOf(".OrderBy") - indexOfSubeFunction;
                }

                string subFunction = tableTemplate.Substring(indexOfSubeFunction, endIndexOfSubeFunction);
                foreach (var record in queryResult)
                {
                    List<InlineCondition> conditions = this.GetCondtionsFromWhereSubFunction(subFunction);
                    this.ResolveCondtions(conditions, record);
                    bool recordFilterResult = ResolveSubQueryResult(conditions, subFunction);

                    if (recordFilterResult)
                    {
                        result.Add(record);
                    }
                }
            }
            else
            {
                result = queryResult.ToList();
            }
            return result;
        }
        private IEnumerable<Entity> OrderByResult(IEnumerable<Entity> queryResult, string tableTemplate)
        {
            List<Entity> result = new List<Entity>();
            string orderPerationName = tableTemplate.Contains(".OrderByDesc(") ? ".OrderByDesc(" : ".OrderBy(";
            if (tableTemplate.Contains(orderPerationName) && queryResult != null && queryResult.Count() > 1)
            {
                int indexOfSubeFunction = tableTemplate.LastIndexOf(orderPerationName);
                string attributeToSortBy = tableTemplate.Substring(indexOfSubeFunction, tableTemplate.Length - indexOfSubeFunction).Trim();
                attributeToSortBy = attributeToSortBy.Replace(orderPerationName, "");
                int lastEndBracket = attributeToSortBy.LastIndexOf(")");
                attributeToSortBy = attributeToSortBy.Substring(0, lastEndBracket)?.Trim().Replace(">", ".");
                result = tableTemplate.Contains(".OrderByDesc(")
                    ? queryResult.OrderByDescending(e => valueResolver.GetAttributeValueForOrderBy(attributeToSortBy, e)).ToList()
                    : queryResult.OrderBy(e => valueResolver.GetAttributeValueForOrderBy(attributeToSortBy, e)).ToList()
                    ;
            }
            else
            {
                result = queryResult.ToList();
            }
            return result;
        }


        private bool ResolveSubQueryResult(List<InlineCondition> conditions, string subFunction)
        {
            bool recordSubQueryResult = false;
            if (conditions.Count > 1)
            {
                if (subFunction.Contains("&&"))
                {
                    recordSubQueryResult = conditions[0].ConditionResult && conditions[1].ConditionResult;
                }
                else
                {
                    recordSubQueryResult = conditions[0].ConditionResult || conditions[1].ConditionResult;
                }
            }
            else
            {
                recordSubQueryResult = conditions[0].ConditionResult;
            }
            return recordSubQueryResult;
        }

        private void ResolveCondtions(List<InlineCondition> conditions, Entity record)
        {
            foreach (var condition in conditions)
            {
                if (condition.ConditionLeftSideKey.Contains("{{"))
                {
                    string leftSideWithouBrackets = condition.ConditionLeftSideKey.Replace("{{", "").Replace("}}", "").Trim();
                    condition.ConditionLeftSideKey = valueResolver.GetAttributeValue(leftSideWithouBrackets, record);
                }

                if (condition.ConditionRightSideKey.Contains("{{"))
                {
                    string rightSideWithouBrackets = condition.ConditionRightSideKey.Replace("{{", "").Replace("}}", "").Trim();
                    condition.ConditionRightSideKey = valueResolver.GetAttributeValue(rightSideWithouBrackets, record);
                }

                condition.ConditionResult = this.GetInlineConditionResult(condition);
            }
        }

        private bool GetInlineConditionResult(InlineCondition inlineCondition)
        {
            bool result = false;


            switch (inlineCondition.Operator)
            {
                case "==":
                    {
                        result = inlineCondition.ConditionRightSideKey == inlineCondition.ConditionLeftSideKey;
                        break;
                    }
                case "!=":
                    {
                        result = inlineCondition.ConditionRightSideKey != inlineCondition.ConditionLeftSideKey;
                        break;
                    }
                case ">":
                    {
                        result = decimal.Parse(inlineCondition.ConditionRightSideKey) > decimal.Parse(inlineCondition.ConditionLeftSideKey);
                        break;
                    }
                case "<":
                    {
                        result = decimal.Parse(inlineCondition.ConditionRightSideKey) < decimal.Parse(inlineCondition.ConditionLeftSideKey);
                        break;
                    }
                case "<=":
                    {
                        result = decimal.Parse(inlineCondition.ConditionRightSideKey) <= decimal.Parse(inlineCondition.ConditionLeftSideKey);
                        break;
                    }
                case ">=":
                    {
                        result = decimal.Parse(inlineCondition.ConditionRightSideKey) >= decimal.Parse(inlineCondition.ConditionLeftSideKey);
                        break;
                    }
            }
            return result;
        }

        public List<InlineCondition> GetCondtionsFromWhereSubFunction(string text)
        {
            text = text.Replace(".Where(", "");
            int lastEndBracket = text.LastIndexOf(")");
            text = text.Substring(0, lastEndBracket);

            List<string> subCondtions = InlineCondition.ExtractCondtionsConditionInlineText(text);
            List<InlineCondition> result = this.BuildConditions(subCondtions);

            return result;
        }

        private List<InlineCondition> BuildConditions(List<string> subCondtions)
        {
            List<InlineCondition> result = new List<InlineCondition>();
            foreach (var subCondition in subCondtions)
            {
                string operatorText = InlineCondition.ExtractOperatorFromConditionText(subCondition);
                if (!string.IsNullOrWhiteSpace(operatorText))
                {
                    int operatorPosition = subCondition.IndexOf(operatorText);
                    InlineCondition inlineCondition = new InlineCondition();
                    inlineCondition.ConditionLeftSideKey = subCondition.Substring(0, operatorPosition).Trim();
                    inlineCondition.ConditionRightSideKey = subCondition.Substring(operatorPosition, subCondition.Length - operatorPosition).Replace(operatorText, "").Trim();
                    inlineCondition.Operator = operatorText;

                    inlineCondition.ConditionLeftSideKey = !inlineCondition.ConditionLeftSideKey.Contains("{{")
           && (inlineCondition.ConditionLeftSideKey?.ToLower() == "null"
               || inlineCondition.ConditionLeftSideKey?.ToLower() == "\"\"")
               ? string.Empty : inlineCondition.ConditionLeftSideKey;

                    inlineCondition.ConditionRightSideKey = !inlineCondition.ConditionRightSideKey.Contains("{{")
               && (inlineCondition.ConditionRightSideKey?.ToLower() == "null"
                   || inlineCondition.ConditionRightSideKey?.ToLower() == "\"\"")
                   ? string.Empty : inlineCondition.ConditionRightSideKey;

                    result.Add(inlineCondition);
                }
            }
            return result;
        }

        private string GetTableTemplateWithoutSubFunction(string tableTemplate)
        {
            string tableTemplateWithoutSubFunction = tableTemplate;
            if (tableTemplate.Contains(".Where("))
            {
                int indexOfSubeFunction = tableTemplate.LastIndexOf(".Where(");
                tableTemplateWithoutSubFunction = tableTemplate.Substring(0, indexOfSubeFunction - 1);
            }
            else if (tableTemplate.Contains(".OrderByDesc("))
            {
                int indexOfSubeFunction = tableTemplate.LastIndexOf(".OrderByDesc(");
                tableTemplateWithoutSubFunction = tableTemplate.Substring(0, indexOfSubeFunction - 1);
            }
            else if (tableTemplate.Contains(".OrderBy("))
            {
                int indexOfSubeFunction = tableTemplate.LastIndexOf(".OrderBy(");
                tableTemplateWithoutSubFunction = tableTemplate.Substring(0, indexOfSubeFunction - 1);
            }

            return tableTemplateWithoutSubFunction;
        }

        public virtual string ExtractOperationTemplateResultPattern(string tablePlaceHolder)
        {
            return tablePlaceHolder.Split('[', ']')[2].Substring(1).Trim();
        }

        public virtual string ParseOperationTemplateResultPattern(IEnumerable<Entity> records, string tableReturnTemplate)
        {
            StringBuilder parsedTableReturnTemplate = null;
            Regex rx = new Regex(@"{{(.+?)}}");
            List<string> attributesWithDuplicates = rx.Matches(tableReturnTemplate).Cast<Match>()
                .Select(m => m.Value.Replace("{{", "").Replace("}}", "").Trim()).ToList();
            HashSet<string> attributes = new HashSet<string>(attributesWithDuplicates);

            if (attributes != null && attributes.Count > 0 && records != null && records.Count() > 0)
            {
                parsedTableReturnTemplate = new StringBuilder();
                int i = 0;
                foreach (var record in records)
                {
                    string recordReturnTemplate = tableReturnTemplate;
                    foreach (var attribute in attributes)
                    {
                        string attributeWithNoWhiteSpaces = attribute.Replace(" ", "");
                        if (attributeWithNoWhiteSpaces.Length <= 3 && attributeWithNoWhiteSpaces.Contains("i")
                            && (attributeWithNoWhiteSpaces.Contains("+") || attributeWithNoWhiteSpaces.Contains("++")))
                        {
                            string[] split = attributeWithNoWhiteSpaces.Split('+');
                            int counterToAdd = split.Length == 3 ? 1 : int.Parse(split[1]);
                            recordReturnTemplate = recordReturnTemplate.Replace($"{{{{{attribute}}}}}", $"{i + counterToAdd}");

                        }
                        else
                        {
                            string placrHolder = attribute.Replace(">", ".");

                            if (record.Contains(placrHolder))
                            {
                                recordReturnTemplate = recordReturnTemplate.Replace($"{{{{{attribute}}}}}", valueResolver.GetAttributeValue(placrHolder, record));
                            }
                            else
                            {
                                recordReturnTemplate = recordReturnTemplate.Replace($"{{{{{attribute}}}}}", "");
                            }


                        }
                    }

                    if (recordReturnTemplate.Contains("?") && recordReturnTemplate.Contains(":") && recordReturnTemplate.Contains("{{"))
                    {
                        recordReturnTemplate = this.HandleParesInnereCnditionsInInSubPlaceHolders(recordReturnTemplate, record);
                    }
                    parsedTableReturnTemplate.AppendLine(recordReturnTemplate);
                    i++;
                }
            }

            return parsedTableReturnTemplate?.ToString();
        }

        private string HandleParesInnereCnditionsInInSubPlaceHolders(string parsedTableReturnTemplate, Entity record)
        {
            string parsedResult = parsedTableReturnTemplate;
            Regex rx = new Regex(@"{{(.+?)}}");
            List<string> attributesWithDuplicates = rx.Matches(parsedTableReturnTemplate).Cast<Match>()
                .Select(m => m.Value.Replace("{{", "").Replace("}}", "")).ToList();
            HashSet<string> attributes = new HashSet<string>(attributesWithDuplicates);
            List<bool> result = new List<bool>();
            foreach (var item in attributes)
            {
                List<string> subCondtions = InlineCondition.ExtractCondtionsConditionInlineText(item);
                List<InlineCondition> conditions = this.BuildConditions(subCondtions);

                this.ResolveCondtions(conditions, record);
                bool recordFilterResult = ResolveSubQueryResult(conditions, item);
                int conditionIfKeyPosition = item.IndexOf("?");
                int conditionElseKeyPosition = item.IndexOf(":");
                string conditionsThenOperationtKey = item.Substring(conditionIfKeyPosition, conditionElseKeyPosition - conditionIfKeyPosition).Replace("?", "").Trim();
                string conditionsElseOperationtKey = item.Substring(conditionElseKeyPosition, item.Length - conditionElseKeyPosition).Replace(":", "").Trim();

                parsedResult = parsedResult.Replace($"{{{{{item}}}}}", recordFilterResult ? conditionsThenOperationtKey : conditionsElseOperationtKey);

            }
            return parsedResult;
        }

        public virtual CustomLinkEntity HandleCreateCustomLinkEntitiesByLinkEntityPlaceHolders(SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            CustomLinkEntityBuilder customLinkEntityBuilder = new CustomLinkEntityBuilder(null, null);

            CustomLinkEntity customLinkEntity = null;
            if (specialOperationPlaceHolder != null)
            {
                List<string> placeHolderParts = specialOperationPlaceHolder.Content.Split('[', ']').Select(t => t.Trim()).ToList();
                string[] lookupToStartFromAndToEntityName = placeHolderParts[0].Split(',');
                string lookupToStartFrom = lookupToStartFromAndToEntityName[0];
                string[] toEntityNameAndAttribte = lookupToStartFromAndToEntityName[1].Split('.');
                string toEntityName = toEntityNameAndAttribte[0].Trim();
                var attributesToSelect = placeHolderParts[1].Split(',').Select(t => t.Trim()).ToList();
                customLinkEntity = new CustomLinkEntity() { EntityName = toEntityName };

                customLinkEntity.IsLinkEntityQuery = true;
                customLinkEntity.TableAttributeFilter = toEntityNameAndAttribte[1].Trim();
                var customEntity = new CustomEntity() { EntityName = toEntityName };
                customLinkEntityBuilder.HandleCreateCustomLinkEntitiesPlaceHolders(customLinkEntity, customEntity, attributesToSelect);
            }

            return customLinkEntity;
        }
        //protected virtual string StripHTML(string input)
        //{
        //    return Regex.Replace(input, "<.*?>", String.Empty);
        //}

        protected virtual string StripHtmlInBraces(string input)
        {
            var _blockRegex = new Regex(
                 @"\{\{(.*?)\}\}",
                 RegexOptions.Singleline | RegexOptions.Compiled);

            var _htmlTagRegex = new Regex(
                @"<.*?>",
                RegexOptions.Singleline | RegexOptions.Compiled);

            return _blockRegex.Replace(input, match =>
            {
                string content = match.Groups[1].Value;

                // Remove HTML tags inside the block
                string cleaned = _htmlTagRegex.Replace(content, string.Empty);

                return "{{" + cleaned + "}}";
            });
        }
    }
}
