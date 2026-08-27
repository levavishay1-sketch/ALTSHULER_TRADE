using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.ParserEngine;
using Alt.Framework.TemplateParser.ValueResolvers;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class ManyToManyOperation : SpecialOperationBase, ILinkEntityOperation
    {        
        public ManyToManyOperation(string prefix, string suffix) : base(prefix, suffix, SpecialOperationType.LinkEntityPlaceHolder) { }

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

            var query = queryExpressionParser.ConvertToManyToManyQueryExpression(customEntity, attribute);
            var queryResult = base.ExecuteQueryFunc(query);
            string tableTemplate = this.ExtractOperationTemplateResultPattern(specialOperationPlaceHolder.Content);
            string parseTableReturnTemplate = this.ParseOperationTemplateResultPattern(queryResult, tableTemplate);
            return parseTableReturnTemplate;
        }
   
        public virtual string ParseOperationTemplateResultPattern(IEnumerable<Entity> records, string tableReturnTemplate)
        {
            StringBuilder parsedTableReturnTemplate = null;
            Regex rx = new Regex(@"{{(.+?)}}");
            List<string> attributes = rx.Matches(tableReturnTemplate).Cast<Match>()
                .Select(m => m.Value.Replace("{{", "").Replace("}}", "")).ToList();
            if (attributes != null && attributes.Count > 0 && records != null && records.Count() > 0)
            {
                DefaultEntityValueResolver defaultEntityValueResolver = new DefaultEntityValueResolver();
                parsedTableReturnTemplate = new StringBuilder();
                foreach (var record in records)
                {
                    string recordReturnTemplate = tableReturnTemplate;
                    foreach (var attribute in attributes)
                    {
                        string placrHolder = attribute.Replace(">", ".");
                        if (record.Contains(placrHolder))
                        {
                            recordReturnTemplate = recordReturnTemplate.Replace($"{{{{{attribute}}}}}", defaultEntityValueResolver.GetAttributeValue(placrHolder, record));
                        }
                        else
                        {
                            recordReturnTemplate = recordReturnTemplate.Replace($"{{{{{attribute}}}}}", "");
                        }

                    }
                    parsedTableReturnTemplate.Append(recordReturnTemplate);
                }
            }
            return parsedTableReturnTemplate?.ToString();
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
                string manyToManyRelationName = lookupToStartFromAndToEntityName[1];
                var attributesToSelect = placeHolderParts[1].Split(',').Select(t => t.Trim()).ToList();
               
                customLinkEntity = new CustomLinkEntity();
                customLinkEntity.IsLinkEntityQuery = true;
                customLinkEntity.EntityName = manyToManyRelationName;

                var customEntity = new CustomEntity() { EntityName = toEntityName };
                customLinkEntityBuilder.HandleCreateCustomLinkEntitiesPlaceHolders(customLinkEntity, customEntity, attributesToSelect);
            }

            return customLinkEntity;
        }

        public string ExtractOperationTemplateResultPattern(string tablePlaceHolder)
        {
            return tablePlaceHolder.Split('[', ']')[2].Substring(1).Trim();
        }
    }
}
