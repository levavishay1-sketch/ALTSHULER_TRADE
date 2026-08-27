using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Contracts;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework;
using Alt.Framework.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm
{
    public class TemplateCommonBL : CrmBaseBL
    {
        public static class Fields
        {
            public const string alt_IsAutomaticBit = "alt_isautomaticbit";
            public const string RegardingObjectId = "regardingobjectid";
            public const string alt_EmailTemplateId = "alt_emailtemplateid";
            public const string alt_SMSTemplateId = "alt_smstemplateid";
            public const string Subject = "subject";
            public const string Description = "description";
            public const string alt_SchemaName = "alt_schemaname";
            public const string alt_ParserCustomEntryPoint = "alt_parsercustomentrypoint";
            public const string alt_ParserCustomEntryPointSchemaName = "alt_parsercustomentrypointschemaname";

        }

        public TemplateCommonBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleTemplateParsing(Entity targetEntity, TemplateType templateType)
        {
            this.GlobalContext.LogEntry();

            string templateIdAttributeName = templateType == TemplateType.Email ? Fields.alt_EmailTemplateId : Fields.alt_SMSTemplateId;
            if (targetEntity != null
                && targetEntity.AttributeHasValue<bool>(Fields.alt_IsAutomaticBit)
                && targetEntity.AttributeHasValue<EntityReference>(Fields.RegardingObjectId)
                && targetEntity.AttributeHasValue<EntityReference>(templateIdAttributeName))
            {
                bool isAutomatic = targetEntity.GetAttributeValue<bool>(Fields.alt_IsAutomaticBit);
                if (isAutomatic)
                {
                    EntityReference regardingObject = targetEntity.GetAttributeValue<EntityReference>(Fields.RegardingObjectId);
                    EntityReference parserEntryPointReference = this.GetTemplateParserRegardingObjectHandler(targetEntity);
                    EntityReference template = targetEntity.GetAttributeValue<EntityReference>(templateIdAttributeName);
                    TemplateCommonDAL templateCommonDAL = new TemplateCommonDAL(this.GlobalContext, template.LogicalName);
                    Entity templateEntity = templateCommonDAL.Get(template.Id, new[] { Fields.alt_SchemaName });

                    if (this.IsValidTemplate(templateEntity, regardingObject, parserEntryPointReference))
                    {
                        this.ParseTemplate(targetEntity, template, templateType, parserEntryPointReference, templateCommonDAL);
                    }
                }
            }
        }

        private bool IsValidTemplate(Entity templateEntity, EntityReference regardingObject, EntityReference parserEntryPointReference)
        {
            this.GlobalContext.LogEntry();

            bool isValid;
            string schemaname = templateEntity?.GetAttributeValue<string>(Fields.alt_SchemaName);

            if (templateEntity == null)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.TemplateNotExist, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.TemplateNotExist));
            }
            else if (string.IsNullOrWhiteSpace(schemaname))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.SchemanameRequired, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.SchemanameRequired));
            }
            else if (schemaname != regardingObject.LogicalName)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.SchemanameAndRegardingObjectNotMatched, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.SchemanameAndRegardingObjectNotMatched));
            }
            else if (parserEntryPointReference.LogicalName != regardingObject.LogicalName
                && string.IsNullOrWhiteSpace(Fields.alt_ParserCustomEntryPointSchemaName))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.ParserEntryPointSchemanameAndReferenceNotMatched, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.ParserEntryPointSchemanameAndReferenceNotMatched));
            }
            else
            {
                isValid = true;
            }
            return isValid;
        }

        private void ParseTemplate(Entity targetEntity, EntityReference template, TemplateType templateType, EntityReference regardingObject, TemplateCommonDAL templateCommonDAL)
        {
            this.GlobalContext.LogEntry();

            TemplateGeneratorDTO templateGeneratorDTO = new TemplateGeneratorDTO()
            {
                TempLateIdInput = template.Id.ToString(),
                TemplateTypeInput = (int)templateType,
                RegardingObjectIdInput = regardingObject.Id.ToString(),
                RegardingObjectEntityNameInput = regardingObject.LogicalName.ToString()
            };

            var response = templateCommonDAL.CallTemplateGeneratorAction(templateGeneratorDTO);

            templateGeneratorDTO.DescriptionMessageOutput = response.Results["DescriptionMessage"]?.ToString();
            templateGeneratorDTO.SubjectTemplateMessageOutput = response.Results["SubjectTemplateMessage"]?.ToString();
            templateGeneratorDTO.IsSucceededOutput = response.Results["IsSucceeded"] != null ? (bool)response.Results["IsSucceeded"] : false;
            if (templateGeneratorDTO.IsSucceededOutput)
            {
                targetEntity[Fields.Subject] = templateGeneratorDTO.SubjectTemplateMessageOutput;
                targetEntity[Fields.Description] = templateGeneratorDTO.DescriptionMessageOutput;
            }
            else
            {
                throw new InvalidPluginExecutionException(templateGeneratorDTO.DescriptionMessageOutput);
            }
        }

        private EntityReference GetTemplateParserRegardingObjectHandler(Entity targetEntity)
        {
            EntityReference parserCustomEntryPointReference = null;
            if (targetEntity.AttributeHasValue<string>(Fields.alt_ParserCustomEntryPoint))
            {
                CustomEntityReference customEntityReference = JsonSerializer.Deserialize<CustomEntityReference>(targetEntity.GetAttributeValue<string>(Fields.alt_ParserCustomEntryPoint));
                parserCustomEntryPointReference = new EntityReference(customEntityReference.LogicalName, customEntityReference.Id);
            }
            return parserCustomEntryPointReference ?? targetEntity.GetAttributeValue<EntityReference>(Fields.RegardingObjectId);
        }
    }
}

