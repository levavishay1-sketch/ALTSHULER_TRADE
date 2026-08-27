using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Alt.External.Services.CrmApi.Framework
{
    public class SASToken
    {
        private string serviceBusNamespaceUrl;
        private string sasKeyName;
        private HMACSHA256 hMACSHA256 { get { return new HMACSHA256(Encoding.UTF8.GetBytes(SasKeyValue)); } }
        private string Signature
        {
            get
            {
                string stringToSign = Uri.EscapeDataString(serviceBusNamespaceUrl).ToLowerInvariant() + "\n" + ExpiryAsString;
                return Convert.ToBase64String(hMACSHA256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            }
        }

        public TimeSpan Expiry { get; set; }
        public DateTime StartTime { get; set; }
        public string ExpiryAsString
        {
            get
            {
                return Convert.ToString((int)this.Expiry.TotalSeconds);
            }
        }
        public string SasKeyValue { get; set; }

        public string Token { get; set; }

        public SASToken(string serviceBusNamespaceUrl, string sasKeyName, string sasKeyValue)
        {
            this.serviceBusNamespaceUrl = serviceBusNamespaceUrl;
            this.sasKeyName = sasKeyName;
            this.SasKeyValue = sasKeyValue;
        }

        public void GenerateToken()
        {
            this.StartTime = DateTime.UtcNow;
            this.Expiry = this.StartTime - new DateTime(1970, 1, 1) + new TimeSpan(0, 0, 90);
            Token = string.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                    Uri.EscapeDataString(serviceBusNamespaceUrl).ToLowerInvariant(), Uri.EscapeDataString(Signature), ExpiryAsString, sasKeyName);
        }
    }
}