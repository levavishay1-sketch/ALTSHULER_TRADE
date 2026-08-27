using Alt.Framework.Utils;
using System.ComponentModel.DataAnnotations;

namespace Alt.Framework.External.ValidationAttributes
{
    public class IsraeliPhoneAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value == null ? true : ValidationUtils.IsValidPhone(value.ToString());
        }
    }
}
