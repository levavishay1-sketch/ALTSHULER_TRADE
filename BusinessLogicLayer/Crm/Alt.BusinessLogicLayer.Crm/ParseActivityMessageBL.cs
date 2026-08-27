using Alt.DataAccessLayer.Crm;
using Alt.DataModel.Crm.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.Framework;
using Alt.Framework.TemplateParser;
using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;

namespace Alt.BusinessLogicLayer.Crm
{
    public class ParseActivityMessageBL : CrmBaseBL
    {
        public ParseActivityMessageBL(GlobalContext globalContext) : base(globalContext) { }

        public void HandleParseActivityMessageHandler(ParameterCollection inputParameters, ParameterCollection outputParameters, string crmUrl)
        {
            this.GlobalContext.LogEntry();

            TemplateGeneratorDTO templateGeneratorDTO = new TemplateGeneratorDTO();
            templateGeneratorDTO.CrmUrlInput = crmUrl;
            templateGeneratorDTO.TempLateIdInput = inputParameters["TemplateId"]?.ToString();
            templateGeneratorDTO.TemplateTypeInput = (int)inputParameters["TemplateType"];
            templateGeneratorDTO.RegardingObjectIdInput = inputParameters["RegardingObjectId"]?.ToString();
            templateGeneratorDTO.RegardingObjectEntityNameInput = inputParameters["RegardingObjectName"]?.ToString();

            if (templateGeneratorDTO.TempLateIdInput == null || templateGeneratorDTO.TemplateTypeInput == null
                || templateGeneratorDTO.RegardingObjectIdInput == null || templateGeneratorDTO.RegardingObjectEntityNameInput == null)
            {
                outputParameters["DescriptionMessage"] = CustomErrorCodes.GetErrorMessage(CustomErrorCodes.NotAllRequiredFieldsHaveBeenFilled);
                outputParameters["SubjectTemplateMessage"] = CustomErrorCodes.GetErrorMessage(CustomErrorCodes.NotAllRequiredFieldsHaveBeenFilled);
                outputParameters["IsSucceeded"] = false;
                throw new InvalidPluginExecutionException(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.NotAllRequiredFieldsHaveBeenFilled));
            }

            this.ParseActivityMessageHandler(templateGeneratorDTO, (TemplateType)templateGeneratorDTO.TemplateTypeInput);

            outputParameters["DescriptionMessage"] = templateGeneratorDTO.DescriptionMessageOutput;
            outputParameters["SubjectTemplateMessage"] = templateGeneratorDTO.SubjectTemplateMessageOutput;
            outputParameters["IsSucceeded"] = templateGeneratorDTO.IsSucceededOutput;
        }

        /// <summary>
        /// Parse Activity Message Handler and fill templateGeneratorDTO object output  parameters
        /// </summary>
        /// <param name="templateGeneratorDTO"></param>
        /// <param name="entityLogicalName"></param>
        private void ParseActivityMessageHandler(TemplateGeneratorDTO templateGeneratorDTO, TemplateType templateTypes)
        {
            this.GlobalContext.LogEntry();

            Entity templateEntity = this.GetTemplateEntityByTemplateType(templateTypes, templateGeneratorDTO.TempLateIdInput, "alt_relatedentity", "alt_templatebody", "alt_subjecttemplate");
            try
            {
                templateGeneratorDTO.DescriptionMessageOutput = this.GetParsedMessage(templateEntity.GetAttributeValue<string>("alt_templatebody"), templateGeneratorDTO);
                templateGeneratorDTO.SubjectTemplateMessageOutput = this.GetParsedMessage(templateEntity.GetAttributeValue<string>("alt_subjecttemplate"), templateGeneratorDTO);
                templateGeneratorDTO.IsSucceededOutput = true;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                var errorCode = ex?.Detail?.ErrorCode;
                switch (errorCode)
                {
                    case -2147217149: //ErrorCodes.QueryBuilderNoAttribute.Code
                        {
                            templateGeneratorDTO.DescriptionMessageOutput = ex.Message;
                            templateGeneratorDTO.SubjectTemplateMessageOutput = ex.Message;
                            templateGeneratorDTO.IsSucceededOutput = false;
                            break;
                        }
                    default:
                        throw;
                }
            }
        }

        private Entity GetTemplateEntityByTemplateType(TemplateType templateTypes, string templateIdToGet, params string[] attributes)
        {
            this.GlobalContext.LogEntry();

            switch (templateTypes)
            {
                case TemplateType.Sms:
                    {
                        SMSTemplateDAL smsTemplateDAL = new SMSTemplateDAL(this.GlobalContext);
                        return smsTemplateDAL.Get(new Guid(templateIdToGet), attributes);
                    }

                case TemplateType.Email:
                    {
                        EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL(this.GlobalContext);
                        return emailTemplateDAL.Get(new Guid(templateIdToGet), attributes);
                    }
                default:
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, CustomErrorCodes.InvalidTemplateType, CustomErrorCodes.GetErrorMessage(CustomErrorCodes.InvalidTemplateType));
            }
        }

        private string GetParsedMessage(string message, TemplateGeneratorDTO templateGeneratorDTO)
        {
            this.GlobalContext.LogEntry();

            ParseActivityMessageDAL parseActivityMessageDAL = new ParseActivityMessageDAL(this.GlobalContext);
            if (!string.IsNullOrWhiteSpace(message))
            {
                Parser parser = new Parser(
                    new ParserSettings
                    {
                        RegardingObjectEntityLogicalName = templateGeneratorDTO.RegardingObjectEntityNameInput,
                        RegardingObjectId = templateGeneratorDTO.RegardingObjectIdInput,
                        CrmUrl = templateGeneratorDTO.CrmUrlInput,
                        MessageToParse = message.Replace("&gt;", ">"),
                        EntityValueResolver = null
                    });

                return parser.GetParsedMessage(parseActivityMessageDAL.ExecuteQuery<Entity>, this.GlobalContext.OrganizationService);
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetParsedMessage(string message, EntityReference regardingObject)
        {
            this.GlobalContext.LogEntry();

            ParseActivityMessageDAL parseActivityMessageDAL = new ParseActivityMessageDAL(GlobalContext);

            if (!string.IsNullOrWhiteSpace(message))
            {
                Parser parser = new Parser(new ParserSettings()
                {
                    RegardingObjectId = regardingObject.Id.ToString(),
                    RegardingObjectEntityLogicalName = regardingObject.LogicalName,
                    MessageToParse = message,
                    EntityValueResolver = null
                }) ;

                return parser.GetParsedMessage(parseActivityMessageDAL.ExecuteQuery<Entity>);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
