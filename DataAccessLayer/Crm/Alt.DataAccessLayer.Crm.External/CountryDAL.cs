using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class CountryDAL : CrmExternalBaseDAL<ApiCountry>
    {
        public CountryDAL(GlobalContext globalContext) : base(globalContext, ApiCountry.EntityLogicalName) { }

        public List<ApiCountry> GetAll()
        {
            this.GlobalContext.LogEntry();

            QueryExpression query = new QueryExpression()
            {
                EntityName = ApiCountry.EntityLogicalName,
                ColumnSet = new ColumnSet(new string[] { "alt_name", "alt_code", "alt_countryalpha3codeiso", "alt_moneylaunderingriskbit" }),
                NoLock = true
            };

            return base.GetMultipleWithPaging(query);
        }
    }
}
