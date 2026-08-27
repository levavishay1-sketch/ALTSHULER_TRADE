using Alt.DataAccessLayer.Crm.External;
using Alt.DataAccessLayer.ExternalServices.ESB;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Alt.Framework.Utils;
using Alt.Framework.Extensions;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class PopulationRegistryCustomerVerificationBL : ExternalBLBase, ICrmOutgoing<ApiPopulationRegistryCustomerVerification>
    {
        Configuration populationRegistryVerificationConfiguraiton;
        public PopulationRegistryCustomerVerificationBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public ActionResult ExecuteOutgoingLogicHandler(ApiContext<ApiPopulationRegistryCustomerVerification> apiContext)
        {
            this.GlobalContext.LogEntry();
            ActionResult actionResult = new ActionResult();

            ApiPopulationRegistryCustomerVerification retrievedPopulationRegistryCustomerVerification = this.GetDetails(apiContext.Target.Id);
            if (retrievedPopulationRegistryCustomerVerification.TransferStatusCode.Value == (int)TransferStatusCode.Sending)
            {
                ESBPopulationRegistryCustomerVerificationDAL eSBPopulationRegistryCustomerVerificationDal = new ESBPopulationRegistryCustomerVerificationDAL(this.GlobalContext, this.ApiConfiguration);
                actionResult = eSBPopulationRegistryCustomerVerificationDal.ExecuteRequest(retrievedPopulationRegistryCustomerVerification);
                this.HandleVerificationResponse(actionResult, retrievedPopulationRegistryCustomerVerification);
            }
            else
            {
                actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidStatusForSendToExternalService, new string[] { Enum.GetName(typeof(TransferStatusCode), retrievedPopulationRegistryCustomerVerification.TransferStatusCode.Value) });
            }

            return actionResult;
        }

        private ApiPopulationRegistryCustomerVerification GetDetails(Guid? id)
        {
            this.GlobalContext.LogEntry();

            PopulationRegistryCustomerVerificationDAL populationRegistryCustomerVerificationDal = new PopulationRegistryCustomerVerificationDAL(this.GlobalContext);
            return populationRegistryCustomerVerificationDal.Get(id.Value, null);
        }

        private void HandleVerificationResponse(ActionResult actionResult, ApiPopulationRegistryCustomerVerification apiEntity)
        {
            this.GlobalContext.LogEntry();

            PopulationRegistryCustomerVerificationDAL populationRegistryCustomerVerificationDal = new PopulationRegistryCustomerVerificationDAL(this.GlobalContext);
            ApiPopulationRegistryCustomerVerification populationRegistryCustomerVerificationToUpdate = new ApiPopulationRegistryCustomerVerification { Id = apiEntity.Id };
            ESBResultStatusCode? resultStatus = ESBResultStatusCode.Error;
            string errorMessage;
            if (actionResult.IsSuccess)
            {
                var response = base.GetDeserializedContent<ESBResponse<ESBPopulationRegistryCustomerVerificationResponse>>(actionResult.ReturnObject?.ToString());
                resultStatus = response.ResultStatusCode;
                errorMessage = response.ErrorMessage;
                if (resultStatus == null)
                {
                    actionResult.SetToFailedActionResult(CustomErrorCodes.InvalidEsbResultStatusError, new[] { response.ErrorCode?.ToString() });
                }
                else
                {
                    populationRegistryCustomerVerificationToUpdate.IDIssuanceDateVerificationResultCode = this.GetVerificationResultCode(response.ResponseData?.IndTaaricHanpakaTzMatchDb);
                    populationRegistryCustomerVerificationToUpdate.VerificationResultCode = this.GetVerificationResultCode(response.ResponseData?.KodImut);
                    populationRegistryCustomerVerificationToUpdate.ResponseDetails = base.Serialize(response.ResponseData);
                    if (response.ResponseData?.KodImut == (int)ESBPopulationRegisterVerificationResultCode.Verified)
                    {
                        Configuration configuration = this.GetPopulationRegistryVerificationConfiguration(apiEntity.CompanyCodeInt.Value);
                        if (configuration != null)
                        {
                            this.HandleResponseDetails(populationRegistryCustomerVerificationToUpdate, apiEntity, response.ResponseData);
                        }
                        else
                        {
                            this.GlobalContext.Log.Warning($"Not Defind Configurations for Company Code ({apiEntity.CompanyCodeInt}) in Global Parameter");
                        }
                    }              
                }
            }
            else
            {
                errorMessage = actionResult.Error?.Message;
            }
            populationRegistryCustomerVerificationToUpdate.TransferStatusCode =  resultStatus == ESBResultStatusCode.Success ?
                (int)TransferStatusCode.Sent : (int)TransferStatusCode.Failed;
            populationRegistryCustomerVerificationToUpdate.ErrorMessageDetails = errorMessage?.SubstringByLength(populationRegistryCustomerVerificationDal.GetErrorMessageDetailsMaxLength());

            populationRegistryCustomerVerificationDal.Update(populationRegistryCustomerVerificationToUpdate);
        }

        private void HandleResponseDetails(ApiPopulationRegistryCustomerVerification apiEntityToUpdate, ApiPopulationRegistryCustomerVerification apiEntity, ESBPopulationRegistryCustomerVerificationResponse responseData)
        {
            this.GlobalContext.LogEntry();

            DefinitionsByEntity relatedEntityDefinition = null;
            Dictionary<string, string> dataToDisplay = null;
            Dictionary<string, string> relatedRecordValues = null;
            Configuration configurations = this.GetPopulationRegistryVerificationConfiguration(apiEntity.CompanyCodeInt.Value);

            if (apiEntity.RelatedRecordId != null)
            {
                relatedEntityDefinition = configurations.DefinitionsByEntity?
                    .Where(c => c.LogicalName == apiEntity.RelatedRecordId.LogicalName).FirstOrDefault();
                if (relatedEntityDefinition?.AttributesToCompare != null 
                    && relatedEntityDefinition.AttributesToCompare.Count > 0)
                {
                    relatedRecordValues = GetRelatedRecordValues(apiEntity.RelatedRecordId, relatedEntityDefinition.AttributesToCompare);
                }
            }
            dataToDisplay = this.GenerateResponseToDisplayForPCF(responseData, relatedEntityDefinition?.AttributesToDisplay ?? configurations.DefaultAttributesToDisplay, relatedRecordValues);
            apiEntityToUpdate.ResponseDitailsToDisplay = dataToDisplay != null ? base.Serialize(dataToDisplay): null;

            if (apiEntity.CompareDataBit != null && apiEntity.CompareDataBit.Value)
            {               
                var discrepancyData = dataToDisplay?.Where(d => d.Value.StartsWith("#")).Select(d => d.Key).ToList();
                bool isNotMatch = discrepancyData != null && discrepancyData.Count() > 0;
                apiEntityToUpdate.DiscrepancyDetails = isNotMatch ?
                    string.Join(Environment.NewLine, discrepancyData) : null;
                apiEntityToUpdate.DataComparisonStatusCode = isNotMatch ?
                    (int)DataComparisonStatusCode.NotMatch : (int)DataComparisonStatusCode.Match;
            }
        }

        private Dictionary<string, string> GenerateResponseToDisplayForPCF(ESBPopulationRegistryCustomerVerificationResponse responseData, List<string> attributesToDisplay, Dictionary<string, string> relatedRecordValues = null)
        {
            this.GlobalContext.LogEntry();

            Dictionary<string, string> data = new Dictionary<string, string>();
            if (attributesToDisplay != null && attributesToDisplay.Count > 0)
            {
                foreach (var item in attributesToDisplay)
                {
                    string key = ObjectUtils.GetDescriptionAttribute<ESBPopulationRegistryCustomerVerificationResponse>(item) ?? item;
                    string valueStr = string.Empty;
                    object value = responseData[item];
                    if (value != null)
                    {
                        if (value is DateTime?)
                        {
                            DateTime? dateTime = value as DateTime?;
                            valueStr = dateTime.Value.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            valueStr = value.ToString();
                        }
                    }
                    if (!data.ContainsKey(key))
                    {
                        if (relatedRecordValues != null
                            && relatedRecordValues.ContainsKey(item)
                            && valueStr.Trim() != relatedRecordValues[item].Trim())
                        {
                            valueStr = $"#{valueStr}";
                        }
                        data.Add(key ?? item, valueStr);
                    }
                }
            }
            return data;
        }

        private Dictionary<string, string> GetRelatedRecordValues(ApiEntity relatedRecord, Dictionary<string, string> attributesToCompare)
        {
            this.GlobalContext.LogEntry();
            Dictionary<string, string> result = null;
            if (attributesToCompare != null)
            {
                CommonDAL commonDAL = new CommonDAL(this.GlobalContext, relatedRecord.LogicalName);
                result = commonDAL.GetValuesToCompare(relatedRecord.Id.Value, attributesToCompare);
            }
            return result;
        }

        private Configuration GetPopulationRegistryVerificationConfiguration(int companyCode)
        {
            this.GlobalContext.LogEntry();
            if (this.populationRegistryVerificationConfiguraiton == null)
            {
                string populationRegisterVerificationConfiguration = this.GlobalContext.CacheManager.GetGlobalParameter<string>("PopulationRegistryVerificationConfigurations");
                if (!string.IsNullOrWhiteSpace(populationRegisterVerificationConfiguration))
                {
                    var populationRegistryVerificationSettings = base.GetDeserializedContent<PopulationRegistryVerificationSettings>(populationRegisterVerificationConfiguration);
                    this.populationRegistryVerificationConfiguraiton = populationRegistryVerificationSettings.configurations
                        .Where(s => s.CompanyCode == companyCode).FirstOrDefault();
                }
            }
            return this.populationRegistryVerificationConfiguraiton;
        }

        private int? GetVerificationResultCode(int? kodImut)
        {
            this.GlobalContext.LogEntry();
            int? resultCode = null;
            if (kodImut != null)
            {
                switch (kodImut.Value)
                {
                    case (int)ESBPopulationRegisterVerificationResultCode.Verified:
                        {
                            resultCode = (int)PopulateReqisterVerificationCode.Verified;
                            break;
                        }
                    default:
                        {
                            resultCode = (int)PopulateReqisterVerificationCode.NotVerified;
                            break;
                        }

                }
            }
            return resultCode;
        }

        private void UpdatePopulationRegistryCustomerVerification(ApiPopulationRegistryCustomerVerification populationRegistryCustomerVerificationToUpdate)
        {
            this.GlobalContext.LogEntry();

            PopulationRegistryCustomerVerificationDAL populationRegistryCustomerVerificationDal = new PopulationRegistryCustomerVerificationDAL(this.GlobalContext);
            populationRegistryCustomerVerificationDal.Update(populationRegistryCustomerVerificationToUpdate);
        }
    }
}
