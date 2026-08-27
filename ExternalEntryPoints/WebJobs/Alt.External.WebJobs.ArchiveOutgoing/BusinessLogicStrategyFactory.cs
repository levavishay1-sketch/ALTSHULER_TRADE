using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Errors;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.Crm.External.Interfaces;
using Alt.Framework.External.WebJobs;
using System;

namespace Alt.External.WebJobs.ArchiveOutgoing
{
    public class BusinessLogicStrategyFactory
    {
        internal static IBusinessLogicStrategy GenerateBusinessLogic(WebJobProcessHandler webJobProcessHandler)
        {
            webJobProcessHandler.ThirdPartyBase.GlobalContext.LogEntry();

            IBusinessLogicStrategy businessLogicStrategy;
            switch (webJobProcessHandler.PrimaryEntityLogicalName)
            {
                case ApiDocument.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiDocument, DocumentBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
                        break;
                    }
                case ApiArchiveDocumentSearch.EntityLogicalName:
                    {
                        businessLogicStrategy = new GlobalOutgoingBusinessLogicProducer<ApiArchiveDocumentSearch, ArchiveDocumentSearchBL>(webJobProcessHandler.ThirdPartyBase.GlobalContext, webJobProcessHandler.RemoteContext);
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
