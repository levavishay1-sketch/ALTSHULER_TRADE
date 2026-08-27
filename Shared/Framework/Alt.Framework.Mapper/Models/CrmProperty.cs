using Alt.DataModel.Crm.Core.Enums;

namespace Alt.Framework.Mapper.Models
{
    public class CrmProperty
    {
        public string CrmMetaData { get; set; }
        public CrmPropertyType CrmPropertyType { get; set; }

        public CrmProperty(string crmMetaData, CrmPropertyType crmPropertyType)
        {
            this.CrmMetaData = crmMetaData;
            this.CrmPropertyType = crmPropertyType;
        }
    }
}
