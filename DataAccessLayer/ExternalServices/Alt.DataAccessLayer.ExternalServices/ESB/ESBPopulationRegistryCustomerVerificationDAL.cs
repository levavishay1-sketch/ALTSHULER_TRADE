using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBPopulationRegistryCustomerVerificationDAL : ExternalServicesBaseDAL<ESBPopulationRegistryCustomerVerification, ApiPopulationRegistryCustomerVerification>
    {
        public ESBPopulationRegistryCustomerVerificationDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration)
            : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBPopulationRegistryCustomerVerification MapApiEntityToTargetModel(ApiPopulationRegistryCustomerVerification apiEntity)
        {
            this.GlobalContext.LogEntry();

            return new ESBPopulationRegistryCustomerVerification()
            {
                CompanyCode = apiEntity.CompanyCodeInt,
                Population = apiEntity.PopulationTypeCode,
                IdNumber = apiEntity.IdentityNumber,
                IdIssueDate = this.ConvertDateTime(apiEntity.IDIssuanceDate.Value),
                TaarichLeda = this.ConvertDateTime(apiEntity.BirthDate.Value),
                SystemUserId = apiEntity.Owner.LogicalName == ApiSystemUser.EntityLogicalName?
                        apiEntity.Owner.Id: null
            };
        }

        private DateTime ConvertDateTime(DateTime date)
        {
            var result = date.ConvertUtcToIsraelTime();
            return DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
        }
    }
}
