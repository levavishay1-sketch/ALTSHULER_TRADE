using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Configuration;

namespace Alt.Framework.Azure.KeyVault
{
    public static class KeyVaultUtils
    {
        static SecretClient client;

        static KeyVaultUtils()
        {
            string vaultBaseUrl = ConfigurationManager.AppSettings["VaultBaseURL"].Trim('/');
            client = new SecretClient(new Uri(vaultBaseUrl), new DefaultAzureCredential());
        }

        public static string GetSecretByNameAsync(string secretName)
        {
            KeyVaultSecret keyVaultSecret = client.GetSecretAsync(secretName).Result;
            return keyVaultSecret.Value;
        }
    }
}
