using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class ErrorDAL : CrmExternalBaseDAL<ApiError>
    {
        public ErrorDAL(GlobalContext globalContext) : base(globalContext, ApiSystemLog.EntityLogicalName)
        {
        }

        public List<ApiError> GetSystemLogErrorKeys()
        {
            this.GlobalContext.LogEntry();
            QueryExpression query = new QueryExpression
            {
                EntityName = ApiError.EntityLogicalName,
                ColumnSet = new ColumnSet("alt_errormessage", "alt_errorkey", "alt_description"),
            };

            return GetMultiple(query);
        }
    }
}
