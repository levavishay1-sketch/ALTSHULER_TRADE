using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.TemplateParser.Interfaces;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.TemplateParser.ParserEngine;
using Alt.Framework.TemplateParser.SpecialOperations;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Alt.Framework.TemplateParser
{
    public class Parser
    {
        private static ConcurrentBag<SpecialOperationPlaceHolder> supportedSpecialOperationPlaceHolders = new ConcurrentBag<SpecialOperationPlaceHolder>();
        private Func<QueryBase, IEnumerable<Entity>> executeQueryFunc;
        private Engine engin = null;
        private string parserRelaySettings;
        private GlobalContext globalContext;
        ParserSettings parserSettings;

        public Parser(string entityName, string Message, string id, string crmUrl, IEntityValueResolver entityValueResolver = null
          , List<SpecialOperationBase> specialOperations = null)
        {
            engin = new Engine(entityName, Message, id, crmUrl, entityValueResolver);
            this.BuildsSupportedSpecialOperationPlaceHolders(specialOperations, entityName, id, crmUrl);
            engin.SupportedSpecialOperationPlaceHolders = supportedSpecialOperationPlaceHolders.ToList();
        }

        public Parser(ParserSettings parserSettings, GlobalContext globalContext = null)
        {
            this.parserSettings = parserSettings; // for relay serialize
            this.globalContext = globalContext;
            this.parserRelaySettings = this.globalContext != null ? this.globalContext.CacheManager.GetGlobalParameter<string>("ParserRelaySettings") : null;
            engin = new Engine(parserSettings);
            this.BuildsSupportedSpecialOperationPlaceHolders(parserSettings.SpecialOperations, parserSettings.RegardingObjectEntityLogicalName, parserSettings.RegardingObjectId.ToString(), parserSettings.CrmUrl);
            engin.SupportedSpecialOperationPlaceHolders = supportedSpecialOperationPlaceHolders.ToList();

        }

        public virtual string GetParsedMessage(Func<QueryBase, IEnumerable<Entity>> executeQuery, IOrganizationService organizationService = null)
        {
            bool contimueUsingRelay = false;
            Dictionary<String, string> dic = null;
            string parsedMessageResult = null;
            //{"ServiceEndpointId":"Id","SpecialOperations":"OTM(,MTM("}
            if (globalContext != null && globalContext.EntryPointType != DataModel.Crm.Core.Enums.EntryPointTypeCode.ThirdParty && !string.IsNullOrWhiteSpace(this.parserRelaySettings)) // continue parse in relay
            {
                dic = JsonSerializer.Deserialize<Dictionary<String, string>>(this.parserRelaySettings);
                List<string> specialOperations = dic["SpecialOperations"].Split(',').Select(t => t.Trim()).ToList();
                foreach (var specialOperation in specialOperations)
                {
                    if (engin.Message.Contains(specialOperation))
                    {
                        contimueUsingRelay = true;
                        break;
                    }
                }
            }
            if (contimueUsingRelay)
            {
                string serializedParserSettings = JsonSerializer.Serialize(this.parserSettings);
               // var compressedParserSettings = Encoding.UTF8.GetString(CompressionUtils.ToCompressedJson(Encoding.UTF8.GetBytes(serializedParserSettings), CompressionType.GZip));
                string relayResult = this.globalContext.SendMessageToParsedInRelay(serializedParserSettings, dic["ServiceEndpointId"]);
                ActionResult actionResult = JsonSerializer.Deserialize<ActionResult>(relayResult);
                if (actionResult.IsSuccess)
                {
                    parsedMessageResult = actionResult.ReturnObject.ToString();
                }
                else
                {
                    throw new Exception($"Error Occurred While Parsing, Details: {actionResult?.Error?.ToString()}");
                }
            }
            else // continue parsing in current context plugin or webjob
            {
                this.executeQueryFunc = executeQuery;
                var customLinkEntity = engin.InitiateCustomLinkEntityBuilder();

                QueryExpressionParser queryExpressionParser = new QueryExpressionParser(customLinkEntity);
                var query = queryExpressionParser.ConvertToQueryExpression(organizationService);
                var queryResult = this.executeQueryFunc(query);

                parsedMessageResult = engin.ParseEntitiesToMessage(queryResult);
            }

            return parsedMessageResult;
        }



        public virtual string GetParsedMessage(IOrganizationService organizationService)
        {
            Func<QueryBase, IEnumerable<Entity>> executeQuery = (query) => organizationService.RetrieveMultiple(query).Entities.ToList();
            return GetParsedMessage(executeQuery);
        }

        protected virtual void BuildsSupportedSpecialOperationPlaceHolders(List<SpecialOperationBase> specialOperations, string entityName, string id, string crmUrl)
        {
            if (supportedSpecialOperationPlaceHolders?.Count == 0)
            {
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new UrlOperation("url(", ")", entityName, id, crmUrl)));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new OneToManyOperation("OTM(", ")") { ExecuteQueryFunc = ExecuteDefaultSpecialOperationQueryFunc }));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new ManyToManyOperation("MTM(", ")") { ExecuteQueryFunc = ExecuteDefaultSpecialOperationQueryFunc }));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new CreateHtmlTableOperation("CreateHtmlTable(", ")") { ExecuteQueryFunc = ExecuteDefaultSpecialOperationQueryFunc }));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new CreateTableByHtmlTemplateOperation("CreateTableByHtmlTemplate(", ")") { ExecuteQueryFunc = ExecuteDefaultSpecialOperationQueryFunc }));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new CreateDocxTableOperation("CreateDocxTable(", ")") { ExecuteQueryFunc = ExecuteDefaultSpecialOperationQueryFunc }));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new DateNowOperation("DateNow(", ")")));
                supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(new InlineIfOperation(null, null)));

                if (specialOperations != null)
                {
                    foreach (var specialOperation in specialOperations)
                    {
                        if (specialOperation.SpecialOperationType == SpecialOperationType.LinkEntityPlaceHolder && specialOperation.ExecuteQueryFunc == null)
                        {
                            specialOperation.ExecuteQueryFunc = ExecuteDefaultSpecialOperationQueryFunc;
                        }

                        supportedSpecialOperationPlaceHolders.Add(new SpecialOperationPlaceHolder(specialOperation));
                    }
                }
            }
        }

        protected virtual IEnumerable<Entity> ExecuteDefaultSpecialOperationQueryFunc(QueryBase query)
        {
            return this.executeQueryFunc(query);
        }
    }
}
