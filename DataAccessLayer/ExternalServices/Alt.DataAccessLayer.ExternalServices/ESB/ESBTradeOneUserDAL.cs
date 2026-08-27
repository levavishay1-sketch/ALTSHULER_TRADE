using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBTradeOneUserDAL : ExternalServicesBaseDAL<ESBTradeOneUser, ApiAccountHolder>
    {
        public ESBTradeOneUserDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) 
            : base(globalContext, apiConfiguration) { }

        protected override ESBTradeOneUser MapApiEntityToTargetModel(ApiAccountHolder apiEntity)
        {
            this.GlobalContext.LogEntry();
            ESBTradeOneUser tradeOneUser = new ESBTradeOneUser()
            {
                ContactId = apiEntity.IdentificationNumber,
                FirstNameEng = apiEntity.FirstNameEng,
                LastNameEng = apiEntity.LastNameEng,
                FirstNameHeb = apiEntity.FirstName,
                LastNameHeb = apiEntity.LastName,
                Mobile = apiEntity.MobilePhone,
                Email = apiEntity.Email,
                AccountNumber = apiEntity.Portfolio?.ShenhavAccountNumber,
                IsPro = apiEntity.UserCharacteristicCode != null
                            && apiEntity.UserCharacteristicCode.Value == (int)UserCharacteristicCode.ProfessionalUser ?
                            true.ToString() : false.ToString(),
                //group1 = apiEntity.Group != null ? apiEntity.Group.Value.ToString(): string.Empty
                group1 = apiEntity.DigitalFormVerification.CommissionClientType.TradeOneGroup != null ?
                    apiEntity.DigitalFormVerification.CommissionClientType.TradeOneGroup : string.Empty
            };
            return tradeOneUser;
        }
    }
}
