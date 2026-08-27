using System.Text.RegularExpressions;

namespace Alt.Framework.Utils
{
    public static class ValidationUtils
    {
        public static bool IsValidMobilePhone(string mobilePhone)
        {
            return !string.IsNullOrWhiteSpace(mobilePhone) && Regex.IsMatch(mobilePhone, @"^0(5[012345689]){1}(\-)?[^0\D]{1}\d{6}$");
        }

        public static bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^0(5[012345689]|7[12346789]|[23489]){1}(\-)?[^0\D]{1}\d{6}$");
        }

        public static bool IsValidEmailAddress(string mail)
        {
            return !string.IsNullOrWhiteSpace(mail) && Regex.IsMatch(mail, @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$");
        }

        public static bool IsValidGovId(string govId)
        {
            if (string.IsNullOrWhiteSpace(govId) || govId.Length > 9 || govId.Length < 5)
            {
                return false;
            }

            string govIdToValidate = govId.PadLeft(9, '0');

            int sum = 0;

            for (int i = 0; i < govIdToValidate.Length; i++)
            {
                int incNum = int.Parse(govIdToValidate[i].ToString());
                incNum *= (i % 2) + 1;
                if (incNum > 9)
                {
                    incNum -= 9;
                }
                sum += incNum;
            }
            return sum % 10 == 0;
        }
    }
}
