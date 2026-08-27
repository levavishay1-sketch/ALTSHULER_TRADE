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

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class CreateTableByHtmlTemplateOperation : SpecialOperationBase, ILinkEntityOperation
    {
        private const int MaxNumberOfLoop = 100;
        public CreateTableByHtmlTemplateOperation(string prefix, string suffix) : base(prefix, suffix, SpecialOperationType.LinkEntityPlaceHolder) { }

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
                string htmlTemplateContent = tableReturnTemplate.Substring(tableReturnTemplate.IndexOf("<table"));
                result = htmlTemplateContent;
                Regex rx = new Regex(@"{{(.+?)}}");
                Regex rxRows = new Regex(@"<tr>(.+?)</tr>");
                List<string> rows = rxRows.Matches(htmlTemplateContent).Cast<Match>().Select(t => t.Value).ToList();
                string header = rows[0];
                foreach (var row in rows)
                {
                    var attributes = rx.Matches(row).Cast<Match>().Select(m => m.Value.Trim()).ToList();
                    if (attributes.Count > 0)
                    {
                        DefaultEntityValueResolver defaultEntityValueResolver = new DefaultEntityValueResolver();
                        parsedResult = new StringBuilder();
                        foreach (var record in records)
                        {
                            string newRowValue = row;
                            foreach (var column in attributes)
                            {
                                string rowWithInnerPlaceHolder = RemoveBetween(newRowValue, "{{", "}}");
                                string parsedColumn = StripHTML(column.Replace("{{", "").Replace("}}", "")).Replace(">", ".")?.Trim();
                                string value = defaultEntityValueResolver.GetAttributeValue(parsedColumn, record);
                                newRowValue = newRowValue.Replace(column, value);
                            }
                            parsedResult.Append(newRowValue);
                        }
                        result = result.Replace(row, parsedResult.ToString());
                    }
                }
            }
            return result?.ToString();
        }

        public virtual CustomLinkEntity HandleCreateCustomLinkEntitiesByLinkEntityPlaceHolders(SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            CustomLinkEntityBuilder customLinkEntityBuilder = new CustomLinkEntityBuilder(null, null);

            CustomLinkEntity customLinkEntity = null;
            if (specialOperationPlaceHolder != null)
            {
                List<string> placeHolderParts = specialOperationPlaceHolder.Content.Split('[', ']').Select(t => t.Trim()).ToList();
                string[] lookupToStartFromAndToEntityName = placeHolderParts[0].Split(',');
                string toEntityName = lookupToStartFromAndToEntityName[1].Split('.')[0];
                string[] toEntityNameAndAttribte = lookupToStartFromAndToEntityName[1].Split('.');
                string htmlTemplateContent = placeHolderParts[0].Substring(placeHolderParts[0].IndexOf("<table"));
                Regex rx = new Regex(@"{{(.+?)}}");
                var attributesToSelect = rx.Matches(htmlTemplateContent).Cast<Match>().Select(m => StripHTML(m.Value.Replace("{{", "").Replace("}}", "")).Trim()).ToList(); //placeHolderParts[1].Split(',').Select(t => t.Trim()).ToList();

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
        public string RemoveBetween(string strSource, string strStart, string strEnd)
        {
            int start = 0, end = 0;
            int numberOfLoops = 1;
            string textToReturn = strSource;
            while (end <= textToReturn.Length - 1)
            {
                if (textToReturn.Contains(strStart) && textToReturn.Contains(strEnd))
                {
                    start = textToReturn.IndexOf(strStart, end) + strStart.Length;
                    end = textToReturn.IndexOf(strEnd, start);
                    textToReturn = textToReturn.Remove(start, end - start);
                }
                numberOfLoops++;
                if (numberOfLoops > MaxNumberOfLoop)
                {
                    throw new Exception("Infinit Loop While Building Result Table Rows");
                }
            }
            return textToReturn;
        }
    }
}
