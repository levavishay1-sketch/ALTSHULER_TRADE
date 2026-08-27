using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.Framework.External.WebJobs;
using System;

namespace Alt.External.WebJobs.DigitalForm
{
    public class BusinessLogicStrategyFactory
    {
        internal static IBusinessLogicStrategy GenerateBusinessLogic(WebJobProcessHandler webJobProcessHandler)
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
                default:
                    {
                        throw new Exception(string.Format(CustomErrorCodes.GetErrorMessage(CustomErrorCodes.NotImplementedInterfaceError), webJobProcessHandler.PrimaryEntityLogicalName));
                    }
            }
            return businessLogicStrategy;           
        }
    }
}
