using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBSmsDAL : ExternalServicesBaseDAL<ESBSms, ApiSms>
    {
        public ESBSmsDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {      
        }

        protected override ESBSms MapApiEntityToTargetModel(ApiSms apiEntity)
        {
            return new ESBSms
            {
                From = apiEntity.SmsTemplate != null ? apiEntity.SmsTemplate.SendBy : "Altshuler",
                To = new List<string>() { apiEntity.MobilePhone },
                Text = apiEntity.Description
            };
        }
    }
}
