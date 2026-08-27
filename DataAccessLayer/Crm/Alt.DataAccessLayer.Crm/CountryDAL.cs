using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class CountryDAL : CrmBaseDAL<alt_Country>
    {
        public CountryDAL(GlobalContext globalContext) : base(globalContext, alt_Country.EntityLogicalName)
        {
        }

        public alt_Country GetCountryByCodeWithCache(int? code = null, string cacheKey = null, int cacheInMinutes = 60)
        {
            GlobalContext.LogEntry();

            string defaultCountryParameterName = "DefaultCountryCode";
            if (!code.HasValue)
            {
                code = GlobalContext.CacheManager.GetGlobalParameter<int?>(defaultCountryParameterName);
            }

            alt_Country retrievedCountry = GlobalContext.CacheManager.GetCachedItem(cacheKey ?? defaultCountryParameterName,
                () => GetFirstOrDefaultByAttribute(alt_Country.Fields.alt_Code, code, new string[] { alt_Country.Fields.alt_CountryId }), cacheInMinutes);
            return retrievedCountry;
        }
    }
}