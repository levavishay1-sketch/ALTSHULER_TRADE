using Alt.DataModel.Crm.Core.Enums;
using System.Collections.Generic;

namespace Alt.Framework.Mapper.Models
{
    public class ApiProperty
    {
        public string PropertName { get; set; }
        public CrmProperty CrmProperty { get; set; }
        public object Value { get; set; }

        public ApiProperty(string propertName, string crmMetaData, CrmPropertyType crmPropertyType, object value)
        {
            this.CrmProperty = new CrmProperty(crmMetaData, crmPropertyType);
            this.Value = value;
            this.PropertName = propertName;
        }

        public override bool Equals(object obj)
        {
            bool isDifferent = true;
            ApiProperty castedApiProperty = obj as ApiProperty;
            var propertyType = this.CrmProperty.CrmPropertyType;

            var propertyName = this.PropertName;
            isDifferent = ((castedApiProperty?.Value != null && !castedApiProperty.Value.Equals(this.Value))
                || (castedApiProperty?.Value == null && castedApiProperty?.Value != this.Value));

            if (isDifferent && propertyType == CrmPropertyType.String)
            {
                if (string.IsNullOrWhiteSpace(castedApiProperty?.Value?.ToString())
                    && string.IsNullOrWhiteSpace(this.Value?.ToString()))
                {
                    isDifferent = false;
                }
            }

            return !isDifferent;
        }

        public bool IsValueEnumerable()
        {
            return Value is IEnumerable<object>;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
