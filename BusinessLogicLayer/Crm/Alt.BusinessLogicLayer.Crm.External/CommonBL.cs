using Alt.DataAccessLayer.Crm.External;
using Alt.DataModel.Crm.Core.Enums;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.BusinessLogicLayer.Crm.External
{
    public class CommonBL
    {
        public static List<ApiConfiguration> GetApiConfigurationByType(GlobalContext globalContext, ApiTypeCode apiTypeCode)
        {
            globalContext.LogEntry();

            ApiConfigurationDAL apiConfigurationDal = new ApiConfigurationDAL(globalContext);
            return apiConfigurationDal.GetApiConfigurationByType(apiTypeCode);
        }
    }
}
