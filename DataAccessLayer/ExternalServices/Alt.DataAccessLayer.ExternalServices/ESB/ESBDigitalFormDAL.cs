using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.Enums;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using Alt.Framework.Extensions;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBDigitalFormDAL : ExternalServicesBaseDAL<ESBDigitalForm, ApiDigitalForm>
    {
        public ESBDigitalFormDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBDigitalForm MapApiEntityToTargetModel(ApiDigitalForm apiEntity)
        {
            this.GlobalContext.LogEntry();

            ESBDigitalForm digitalForm = new ESBDigitalForm
            {
                IdForm = apiEntity.Id.ToString(),
                FormCodeId = OutSystemFormIdCode.TradeStockExchangeMemberJoining.GetDescriptionAttribute(),
                SystemName = this.GetSystemName(apiEntity.DigitalFormType),
                LeadNumber = apiEntity.DigitalFormIdentityNumber,
                MobilePhone = this.GetMobilePhoneByRegardingObject(apiEntity.RegardingObject),
                SystemUserId = apiEntity.ModifiedBy.Id.Value.ToString()
            };
            return digitalForm;
        }

        private string GetSystemName(int? digitalFormType)
        {
            this.GlobalContext.LogEntry();

            string systemName = string.Empty;
            if (digitalFormType != null)
            {
                DigitalFormTypeCode digitalFormTypeCode = (DigitalFormTypeCode)digitalFormType.Value;
                switch (digitalFormTypeCode)
                {
                    case DigitalFormTypeCode.TradeJoining:
                        {
                            systemName = ExternalSystemNameCode.CrmTrade.GetDescriptionAttribute();
                            break;
                        }
                    default:
                        break;
                }
            }
            return systemName;
        }

        private string GetMobilePhoneByRegardingObject(ApiEntityBase regardingObject)
        {
            this.GlobalContext.LogEntry();
            string mobilePhone = null;
            if (regardingObject is ApiLead)
            {
                ApiLead apiLead = regardingObject as ApiLead;
                mobilePhone = apiLead.MobilePhone;
            }
            return mobilePhone;
        }
    }
}
