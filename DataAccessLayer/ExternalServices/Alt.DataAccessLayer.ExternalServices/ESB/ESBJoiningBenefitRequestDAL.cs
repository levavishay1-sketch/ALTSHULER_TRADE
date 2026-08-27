using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Extensions;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBJoiningBenefitRequestDAL : ExternalServicesBaseDAL<ESBJoiningBenefitRequest, ApiAccountHolder>
    {
        public ESBJoiningBenefitRequestDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration)
        : base(globalContext, apiConfiguration) { }

        protected override ESBJoiningBenefitRequest MapApiEntityToTargetModel(ApiAccountHolder apiEntity)
        {
            int? mobilePhoneNumbersCount;
            return new ESBJoiningBenefitRequest
            {
                TPID = apiEntity.MobilePhone != null
                    && base.ApiConfiguration.TryGetSettingsItemValue<int?>(nameof(mobilePhoneNumbersCount),out mobilePhoneNumbersCount)
                    && mobilePhoneNumbersCount != null?
                    apiEntity.MobilePhone.GetLast(mobilePhoneNumbersCount.Value)
                    : apiEntity.MobilePhone
            };
        }
    }
}
