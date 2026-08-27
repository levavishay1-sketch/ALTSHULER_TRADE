using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Alt.Framework.Extensions
{
   public static class ObjectExtensions
    {
        public static string GetDescriptionAttribute<T>(this T source)
        {
            string descriptionResult = null;
            FieldInfo fieldInfo = source.GetType().GetField(source.ToString());
            if (fieldInfo != null)
            {
                descriptionResult = source.ToString();
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.
                    GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    descriptionResult = attributes[0].Description;
                }
            }
          
            return descriptionResult;
        }
    }
}
