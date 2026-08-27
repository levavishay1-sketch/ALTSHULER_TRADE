using Alt.DataModel.Crm.Core.Enums;
using System;
using System.Runtime.CompilerServices;

namespace Alt.Framework.Mapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CrmEntityMapperAttribute : Attribute
    {
        public string ApiPropertyName { get; set; }
        public string CrmPropertyName { get; set; }
        public bool MappToCrm { get; set; }
        public bool MappFromCrm { get; set; }
        public bool IsCrmPrimaryAttribute { get; set; }
        public CrmPropertyType TargetCrmPropertyType { get; set; }

        public CrmEntityMapperAttribute(string targetPropertyName, CrmPropertyType targetCrmPropertyType, bool mappToCrm = true, bool mappFromCrm = true, bool isCrmPrimaryAttribute = false, [CallerMemberName] string sourcePropertyName = "")
        {
            this.ApiPropertyName = sourcePropertyName;
            this.CrmPropertyName = targetPropertyName;
            this.TargetCrmPropertyType = targetCrmPropertyType;
            this.MappToCrm = mappToCrm;
            this.MappFromCrm = mappFromCrm;
            this.IsCrmPrimaryAttribute = isCrmPrimaryAttribute;
        }
    }
}
