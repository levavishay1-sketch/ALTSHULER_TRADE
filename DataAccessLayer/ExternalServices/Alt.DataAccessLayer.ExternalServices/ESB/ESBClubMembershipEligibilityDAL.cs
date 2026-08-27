using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;
using System;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBClubMembershipEligibilityDAL : ExternalServicesBaseDAL<ESBClubMembershipEligibilityRequest, ApiAccountHolder>
    {
        public ESBClubMembershipEligibilityDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration)
            : base(globalContext, apiConfiguration) { }

        protected override ESBClubMembershipEligibilityRequest MapApiEntityToTargetModel(ApiAccountHolder apiEntity)
        {
            int? identityNumber = null;
            if (!string.IsNullOrWhiteSpace(apiEntity.IdentificationNumber)
                    && int.TryParse(apiEntity.IdentificationNumber, out int result))
            {
                identityNumber = result;
            }
            return new ESBClubMembershipEligibilityRequest
            {
                IDNumber = identityNumber
            };
        }
    }
}
