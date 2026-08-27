using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.Utils
{
    public class ObjectUtils
    {
        public static string GetDescriptionAttribute<T>(string fieldName)
        {
            string result;
            PropertyInfo propertyInfo = typeof(T).GetProperty(fieldName);
            if (propertyInfo != null)
            {
                try
                {
                    object[] descriptionAttrs = propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
                    result = (description.Description);
                }
                catch
                {
                    result = null;
                }
            }
            else
            {
                result = null;
            }

            return result;
        }
    }
}
