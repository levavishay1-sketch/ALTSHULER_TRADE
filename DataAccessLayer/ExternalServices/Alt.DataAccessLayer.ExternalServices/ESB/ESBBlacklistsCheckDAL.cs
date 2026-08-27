using Alt.DataModel.Crm.External.Contracts;
using Alt.DataModel.ExernalServices.ESB;
using Alt.Framework;

namespace Alt.DataAccessLayer.ExternalServices.ESB
{
    public class ESBBlacklistsCheckDAL : ExternalServicesBaseDAL<ESBBlacklistsCheck, ApiBlacklistsCheck>
    {
        public ESBBlacklistsCheckDAL(GlobalContext globalContext, ApiConfiguration apiConfiguration) : base(globalContext, apiConfiguration)
        {
        }

        protected override ESBBlacklistsCheck MapApiEntityToTargetModel(ApiBlacklistsCheck apiEntity)
        {
            return new ESBBlacklistsCheck
            {
                FirstName = apiEntity.FirstName,
                LastName = apiEntity.LastName,
                ExternalNumber = apiEntity.IdentityNumber,
                DateOfBirth = apiEntity.BirthDate
            };
        }
    }
}
