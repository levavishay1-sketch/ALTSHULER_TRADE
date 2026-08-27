using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.ParserEngine;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class CreateDocxTableOperation : SpecialOperationBase, ILinkEntityOperation
    {
        public CreateDocxTableOperation(string prefix, string suffix) : base(prefix, suffix, SpecialOperationType.LinkEntityPlaceHolder) { }

        public override string ExecuteSpecialOperationLogic(Entity entity, string key, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
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
            string parseTableReturnTemplate = this.ParseOperationTemplateResultPattern(queryResult, specialOperationPlaceHolder.Content);
            return parseTableReturnTemplate;
        }

        public virtual string ParseOperationTemplateResultPattern(IEnumerable<Entity> records, string tableReturnTemplate)
        {
            StringBuilder parsedResult = null;
            string result = tableReturnTemplate;
            if (records?.Count() > 0)
            {
                string tableXML = $"<w:tbl>{ExtractString(result, "<w:tbl>", "</w:tbl>")}</w:tbl>";

                var columns = this.GetColumns(tableXML);
                Dictionary<string, string> templateColumns = GetConcreteTempolateColumns(columns);
                string tempTable = tableXML;
                string fristRow = ExtractString(tableXML, "<w:tr", "</w:tr>");
                tempTable = tempTable.Replace($"<w:tr{fristRow}</w:tr>", string.Empty);
                string originalRowTemplate = $"<w:tr{ExtractString(tempTable, "<w:tr", "</w:tr>")}</w:tr>";
                var kvList = templateColumns.ToList();
                parsedResult = new StringBuilder();
                DefaultEntityValueResolver defaultEntityValueResolver = new DefaultEntityValueResolver();

                foreach (var record in records)// result from parser
                {
                    string rowTemplate = originalRowTemplate;
                    int templateColumnsCount = templateColumns.ToList().Count;
                    for (int i = 0; i < templateColumnsCount; i++)
                    {
                        string parsedColumn = StripHTML(kvList[i].Key).Replace(">", ".")?.Trim().Replace("{{",string.Empty).Replace("}}",string.Empty);
                        string placeHolderValue = record.Contains(parsedColumn) ? defaultEntityValueResolver.GetAttributeValue(parsedColumn, record) : null;
                        string valuText = $"<w:t>{placeHolderValue}</w:t>";
                        rowTemplate = rowTemplate.Replace(kvList[i].Value, valuText);
                    }
                    parsedResult.AppendLine(rowTemplate);
                }

                result = tableXML.Replace(originalRowTemplate, parsedResult.ToString());
            }
            return result;
        }


        private List<string> GetColumns(string tableXML)
        {
            List<string> columns = new List<string>();
            string fristRow = ExtractString(tableXML, "<w:tr", "</w:tr>");
            tableXML = tableXML.Replace($"<w:tr{fristRow}</w:tr>", string.Empty);
            string column = ExtractString(tableXML, "<w:tc>", "</w:tc>");
            while (column != null)
            {
                if (!columns.Contains(column))
                {
                    columns.Add(column);
                    tableXML = tableXML.Replace($"<w:tc>{column}</w:tc>", string.Empty);
                    column = ExtractString(tableXML, "<w:tc>", "</w:tc>");
                }
            }
            return columns;
        }

        private Dictionary<string, string> GetConcreteTempolateColumns(List<string> columns)
        {
            Dictionary<string, string> templateColumns = new Dictionary<string, string>();
            foreach (var column in columns)
            {
                int firstindexOfOriginalPlaceholder = column.IndexOf("<w:t>{");
                int lastIndexndexOfOriginalPlaceholder = column.LastIndexOf("}</w:t>") + 7;
                string originalPlaceholder = column.Substring(firstindexOfOriginalPlaceholder, lastIndexndexOfOriginalPlaceholder - firstindexOfOriginalPlaceholder);
                var htmlDecodedPlaceHolder = HttpUtility.HtmlDecode(Regex.Replace(originalPlaceholder, @"<[^>]*>", String.Empty));
                templateColumns.Add(htmlDecodedPlaceHolder, originalPlaceholder);
            }

            return templateColumns;
        }

        public virtual CustomLinkEntity HandleCreateCustomLinkEntitiesByLinkEntityPlaceHolders(SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            CustomLinkEntityBuilder customLinkEntityBuilder = new CustomLinkEntityBuilder(null, null);

            CustomLinkEntity customLinkEntity = null;
            if (specialOperationPlaceHolder != null)
            {
                string content = StripHTML(specialOperationPlaceHolder.Content);
                List<string> placeHolderParts = content.Split('[', ']').Select(t => t.Trim()).ToList();
                string[] lookupToStartFromAndToEntityName = placeHolderParts[0].Split(',');
                string toEntityName = lookupToStartFromAndToEntityName[1].Split('.')[0];
                string[] toEntityNameAndAttribte = lookupToStartFromAndToEntityName[1].Split('.');


                string stripedXmlTemplateContent = StripHTML(specialOperationPlaceHolder.Content.Substring(specialOperationPlaceHolder.Content.IndexOf("<w:tbl>")));
                Regex rx = new Regex(@"{{(.+?)}}");
                var attributesToSelect = rx.Matches(stripedXmlTemplateContent).Cast<Match>().Select(m => StripHTML(m.Value.Replace("{{", "").Replace("}}", "")).Trim()).ToList(); //placeHolderParts[1].Split(',').Select(t => t.Trim()).ToList();

                customLinkEntity = new CustomLinkEntity() { EntityName = toEntityName };

                customLinkEntity.IsLinkEntityQuery = true;
                customLinkEntity.TableAttributeFilter = toEntityNameAndAttribte[1];
                var customEntity = new CustomEntity() { EntityName = toEntityName };
                customLinkEntityBuilder.HandleCreateCustomLinkEntitiesPlaceHolders(customLinkEntity, customEntity, attributesToSelect);
            }

            return customLinkEntity;
        }

        public string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public string ExtractOperationTemplateResultPattern(string tablePlaceHolder)
        {
            return tablePlaceHolder;
        }
        private string ExtractString(string s, string startTag = "@{", string endTag = "}@")
        {
            int startIndex = s.IndexOf(startTag) != -1 ? s.IndexOf(startTag) + startTag.Length : -1;
            int endIndex = startIndex != -1 ? s.IndexOf(endTag, startIndex) : -1;
            return startIndex == -1 ? null : s.Substring(startIndex, endIndex - startIndex);
        }


        public override string GetTextContentFromSpecialOperationPattern(string text, string prefix, string suffix)
        {
            string result = null;
            string textWithoutTags = StripHTML(text);
            if (textWithoutTags.Contains(this.Prefix))
            {
                int position1 = text.IndexOf("(");
                int position2 = text.LastIndexOf(")");
                if (position1 == -1 || position2 == -1)
                {
                    return result;
                }

                position1 += 1;
                result = text.Substring(position1, position2 - position1);
            }

            return result;
        }
        public override string ReplaceResultInOriginalMessage(string wholeMessageWithOriginalPlaceHolder, string placeHolderWithPrefixAndSuffix, SpecialOperationPlaceHolder specialOperationPlaceHolder, string value)
        {
            string first = wholeMessageWithOriginalPlaceHolder.Substring(0,wholeMessageWithOriginalPlaceHolder.IndexOf(specialOperationPlaceHolder.Content));
            string originalPrefixWithXmlTags = first.Substring(first.LastIndexOf("{") -1);
            placeHolderWithPrefixAndSuffix = placeHolderWithPrefixAndSuffix.Replace($"@{{{this.Prefix}", originalPrefixWithXmlTags);
            return base.ReplaceResultInOriginalMessage(wholeMessageWithOriginalPlaceHolder, placeHolderWithPrefixAndSuffix, specialOperationPlaceHolder, value);
        }
    }
}
