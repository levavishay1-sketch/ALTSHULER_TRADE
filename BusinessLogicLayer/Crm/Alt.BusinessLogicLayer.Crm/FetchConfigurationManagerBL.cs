using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Alt.Framework.Utils;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Alt.BusinessLogicLayer.Crm
{
    public class FetchConfigurationManagerBL : CrmBaseBL
    {
        public FetchConfigurationManagerBL(GlobalContext globalContext) : base(globalContext) { }

        public ActionResult FetchRecords(ParameterCollection inputParameters)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            SearchEntryPointObject searchEntryPointObject = new SearchEntryPointObject
            {
                SearchType = int.Parse(inputParameters["SearchType"].ToString()),
                SearchTables = inputParameters.ContainsKey("SearchTables") ? inputParameters["SearchTables"]?.ToString() : null,
                SearchField = inputParameters.ContainsKey("SearchField") ? inputParameters["SearchField"]?.ToString() : null,
                SearchInput = inputParameters.ContainsKey("SearchInput") ? inputParameters["SearchInput"]?.ToString() : null,
                EntityLogicalName = inputParameters.ContainsKey("EntityLogicalName") ? inputParameters["EntityLogicalName"]?.ToString() : null,
                EntityId = inputParameters.ContainsKey("EntityId") ? inputParameters["EntityId"]?.ToString() : null
            };

            this.GlobalContext.Log.Info(searchEntryPointObject.ToString());

            switch (searchEntryPointObject.SearchType)
            {
                case (int)SearchSourceTypeCode.Entity:
                    {
                        actionResult = FetchRecordsForEntity(
                            searchEntryPointObject.SearchType,
                            searchEntryPointObject.EntityLogicalName,
                            searchEntryPointObject.EntityId
                        );
                        break;
                    }
                case (int)SearchSourceTypeCode.SearchPage:
                    {
                        actionResult = FetchEntitiesForSearchPage(
                            searchEntryPointObject.SearchType,
                            searchEntryPointObject.SearchTables,
                            searchEntryPointObject.SearchField,
                            searchEntryPointObject.SearchInput
                            );
                        break;
                    }
                default:
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.SearchSourceTypeCodeInvalid, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.SearchSourceTypeCodeInvalid));
            }

            return actionResult;
        }

        private ActionResult FetchRecordsForEntity(int sourceType, string entityLogicalName, string entityId)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            SearchEntryPointConfigurationDAL searchEntryPointConfigurationDAL = new SearchEntryPointConfigurationDAL(this.GlobalContext);
            alt_SearchEntryPointConfiguration retrievedConfiguration = searchEntryPointConfigurationDAL.GetSearchEntryPointConfiguration(entityLogicalName, sourceType);

            FetchConfigurationDAL fetchConfigurationDAL = new FetchConfigurationDAL(this.GlobalContext);
            List<alt_FetchConfiguration> fetchConfigurations = fetchConfigurationDAL.GetByAttribute(
                alt_FetchConfiguration.Fields.alt_SearchEntryPointConfigurationId,
                retrievedConfiguration.alt_SearchEntryPointConfigurationId,
                new string[] {
                    alt_FetchConfiguration.Fields.alt_TargetEntityName,
                    alt_FetchConfiguration.Fields.alt_TargetEntitySchemaName,
                    alt_FetchConfiguration.Fields.alt_FetchXml
                }
            );

            List<ItemGroup> itemGroups = new List<ItemGroup>();
            foreach (alt_FetchConfiguration fetchConfiguration in fetchConfigurations)
            {
                Parser parser = new Parser(new ParserSettings()
                {
                    RegardingObjectId = entityId,
                    RegardingObjectEntityLogicalName = entityLogicalName,
                    MessageToParse = fetchConfiguration.alt_FetchXml,
                    EntityValueResolver = null,

                }, this.GlobalContext);

                string parsedFetchXml = parser.GetParsedMessage(this.GlobalContext.OrganizationService);
                this.GlobalContext.Log.Info($"parsed fetch xml: {parsedFetchXml}");

                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, fetchConfiguration.alt_TargetEntitySchemaName);
                ItemGroup itemGroup = new ItemGroup
                {
                    EntityName = fetchConfiguration.alt_TargetEntityName,
                    EntitySchemaName = fetchConfiguration.alt_TargetEntitySchemaName,
                    Items = commonDAL.Fetch(parsedFetchXml)
                };
                itemGroups.Add(itemGroup);
            }

            SearchEntryPointResponse response = new SearchEntryPointResponse
            {
                Columns = retrievedConfiguration.alt_TableColumnDefinition,
                ItemGroups = itemGroups
            };

            actionResult.ReturnObject = JsonUtils.Serialize(response);
            return actionResult;
        }

        private ActionResult FetchEntitiesForSearchPage(int sourceType, string searchTables, string searchField, string searchValue)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            string[] tablesToRetrieve = searchTables.Split(',');
            SearchEntryPointConfigurationDAL searchEntryPointConfigurationDAL = new SearchEntryPointConfigurationDAL(this.GlobalContext);
            FetchConfigurationDAL fetchConfigurationDAL = new FetchConfigurationDAL(this.GlobalContext);
            List<SearchEntryPointResponse> searchEntryPointResponses = new List<SearchEntryPointResponse>();

            foreach (string table in tablesToRetrieve)
            {
                alt_SearchEntryPointConfiguration retrievedConfiguration = searchEntryPointConfigurationDAL.GetSearchEntryPointConfiguration(table, sourceType);
                List<alt_FetchConfiguration> fetchfetchConfigurations = fetchConfigurationDAL.GetFetchConfigurationsByFilterFieldAndEntryPoint(retrievedConfiguration.Id, searchField);
                List<ItemGroup> itemGroups = new List<ItemGroup>();

                foreach (alt_FetchConfiguration fetchConfiguration in fetchfetchConfigurations)
                {
                    string parsedXml = fetchConfiguration.alt_FetchXml.Replace("{placeHolder}", searchValue);
                    this.GlobalContext.LogEntry($"{parsedXml}");

                    CommonDAL commonDAL = new CommonDAL(this.GlobalContext, fetchConfiguration.alt_TargetEntitySchemaName);
                    ItemGroup itemGroup = new ItemGroup
                    {
                        EntityName = fetchConfiguration.alt_TargetEntityName,
                        EntitySchemaName = fetchConfiguration.alt_TargetEntitySchemaName,
                        Items = commonDAL.Fetch(parsedXml)
                    };
                    itemGroups.Add(itemGroup);
                }

                searchEntryPointResponses.Add(new SearchEntryPointResponse
                {
                    EntityName = retrievedConfiguration.alt_SourceEntityName,
                    EntitySchemaName = retrievedConfiguration.alt_SourceEntitySchemaName,
                    Columns = retrievedConfiguration.alt_TableColumnDefinition,
                    ItemGroups = itemGroups
                }
                );
            }

            actionResult.ReturnObject = JsonUtils.Serialize(searchEntryPointResponses);

            return actionResult;
        }
    }
}
