using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.Framework.External.WebJobs;
using System;

namespace Alt.External.WebJobs.CrmOutgoing
{
    public class BusinessLogicStrategyFactory
    {
        public static IBusinessLogicStrategy GenerateBusinessLogic(WebJobProcessHandler webJobProcessHandler)
        {
            webJobProcessHandler.ThirdPartyBase.GlobalContext.LogEntry();
            IBusinessLogicStrategy businessLogicStrategy;
            switch (webJobProcessHandler.PrimaryEntityLogicalName)
            {
                case ApiDigitalForm.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiDigitalForm, DigitalFormBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiSms.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiSms, SmsBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiDigitalFormVerification.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiDigitalFormVerification, DigitalFormVerificationBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiPopulationRegistryCustomerVerification.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiPopulationRegistryCustomerVerification, PopulationRegistryCustomerVerificationBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiScheduledOperation.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiScheduledOperation, ScheduledOperationBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiCustomerOperationRequest.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiCustomerOperationRequest, CustomerOperationRequestBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiBlacklistsCheck.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiBlacklistsCheck, BlacklistsCheckBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                default:
                    {
                        throw new Exception(string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.NotImplementedInterfaceError), webJobProcessHandler.PrimaryEntityLogicalName));
                    }
            }
            return businessLogicStrategy;
        }
    }
}
