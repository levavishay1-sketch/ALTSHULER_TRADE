using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Alt.Framework.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum genericEnum)
        {
            Type enumType = genericEnum.GetType();
            MemberInfo[] memberInfo = enumType.GetMember(genericEnum.ToString());
            string enumDescription = null;
            if (memberInfo?.Count() > 0)
            {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes?.Count() > 0)
                {
                    enumDescription = (attributes.FirstOrDefault() as DescriptionAttribute).Description;
                }
            }
            return enumDescription;
        }
    }
}
