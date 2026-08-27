using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.ParserEngine;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class CreateHtmlTableOperation : SpecialOperationBase, ILinkEntityOperation
    {
        public CreateHtmlTableOperation(string prefix, string suffix) : base(prefix, suffix, SpecialOperationType.LinkEntityPlaceHolder) { }

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
            string tableTemplate = this.ExtractOperationTemplateResultPattern(specialOperationPlaceHolder.Content);
            string parseTableReturnTemplate = this.ParseOperationTemplateResultPattern(queryResult, specialOperationPlaceHolder.Content);
            return parseTableReturnTemplate;
        }

        public virtual string ExtractOperationTemplateResultPattern(string tablePlaceHolder)
        {
            return tablePlaceHolder;
        }

        public virtual string ParseOperationTemplateResultPattern(IEnumerable<Entity> records, string tableReturnTemplate)
        {
            StringBuilder parsedResult = null;
            var squareBarcketsSplit = tableReturnTemplate.Split('[', ']');
            string[] tableTemplatePatternColumns = this.GetFromatedColumnsArray(squareBarcketsSplit[1]);
            string[] actulTableColumns = this.GetFromatedColumnsArray(squareBarcketsSplit[3]);
            string parsedTablePlaceHolder = null;
            if (tableTemplatePatternColumns?.Count() > 0 && records?.Count() > 0)
            {
                //Inet
                DefaultEntityValueResolver defaultEntityValueResolver = new DefaultEntityValueResolver();
                parsedResult = new StringBuilder();
                parsedResult.Append("<table>");
                parsedResult.Append("<tr>");
                foreach (var tableColumn in actulTableColumns)
                {
                    parsedResult.Append("<th>");
                    parsedResult.Append(tableColumn?.Trim());
                    parsedResult.Append("</th>");
                }
                parsedResult.Append("</tr>");

                foreach (var record in records)
                {
                    parsedResult.Append("<tr>");
                    for (int i = 0; i < actulTableColumns.Count(); i++)
                    {
                        parsedTablePlaceHolder = tableTemplatePatternColumns[i].Replace(">", ".")?.Trim();
                        parsedResult.Append("<td>");
                        parsedResult.Append(defaultEntityValueResolver.GetAttributeValue(parsedTablePlaceHolder, record));
                        parsedResult.Append("</td>");
                    }
                    parsedResult.Append("</tr>");
                }
                parsedResult.Append("</table>");
            }
            return parsedResult?.ToString();
        }

        private string[] GetFromatedColumnsArray(string columns)
        {
            return columns.Trim().Replace("\r", "").Replace("\n", "").Split(',');
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
                string toEntityName = toEntityNameAndAttribte[0];
                var attributesToSelect = placeHolderParts[1].Split(',').Select(t => t.Trim()).ToList();
                customLinkEntity = new CustomLinkEntity() { EntityName = toEntityName };

                customLinkEntity.IsLinkEntityQuery = true;
                customLinkEntity.TableAttributeFilter = toEntityNameAndAttribte[1];
                var customEntity = new CustomEntity() { EntityName = toEntityName };
                customLinkEntityBuilder.HandleCreateCustomLinkEntitiesPlaceHolders(customLinkEntity, customEntity, attributesToSelect);
            }

            return customLinkEntity;
        }
    }
}
