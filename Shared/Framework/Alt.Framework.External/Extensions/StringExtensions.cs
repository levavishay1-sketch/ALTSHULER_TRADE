using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Alt.Framework.External.Extensions
{
    public static class StringExtensions
    {
        public static string ReverseOnlyHebrew(this string stringToReverse)
        {
            StringBuilder reversedText = null;
            var firstHebChar = System.Text.ASCIIEncoding.Default.GetBytes(new[] { (char)1488 }).FirstOrDefault(); //א
            var lastHebChar = System.Text.ASCIIEncoding.Default.GetBytes(new[] { (char)1514 }).FirstOrDefault(); //ת
            if (!string.IsNullOrWhiteSpace(stringToReverse))
            {
                reversedText = new StringBuilder();
                string[] arrSplit = Regex.Split(stringToReverse, "( )|([א-ת]+)");
                int arrlenth = arrSplit.Length - 1;
                for (int i = arrlenth; i >= 0; i--)
                {
                    if (arrSplit[i] == " ")
                    {
                        reversedText.Append(" ");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(arrSplit[i]))
                        {
                            int outInt;
                            if (int.TryParse(arrSplit[i], out outInt))
                            {
                                reversedText.Append(Convert.ToInt32(arrSplit[i]));
                            }
                            else
                            {
                                arrSplit[i] = arrSplit[i].Trim();
                                byte[] codes = System.Text.ASCIIEncoding.Default.GetBytes(arrSplit[i].ToCharArray(), 0, 1);
                                if (codes[0] >= firstHebChar && codes[0] <= lastHebChar) // is Hebrew character
                                {
                                    reversedText.Append(Reverse(arrSplit[i]));
                                }
                                else
                                {
                                    reversedText.Append(arrSplit[i].Trim());
                                }
                            }
                        }
                    }
                }
            }

            return reversedText != null ? reversedText.ToString() : null;
        }

        public static string Reverse(this string stringToReverse)
        {
            string reversedText = stringToReverse;
            if (!string.IsNullOrWhiteSpace(stringToReverse))
            {
                char[] charArray = stringToReverse.ToCharArray();
                Array.Reverse(charArray);
                reversedText = new string(charArray);
            }
            return reversedText;
        }
    }
}
