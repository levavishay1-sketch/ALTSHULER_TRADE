using Alt.Framework.Azure.KeyVault;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;

namespace Alt.Framework.EntryPoints.External
{
    public class ConnectionQueue
    {
        static ConcurrentDictionary<int, CrmServiceManager> connectionPool = new ConcurrentDictionary<int, CrmServiceManager>();
        static int currentItemIndex = 1;

        static ConnectionQueue()
        {
            string[] crmConnectionKVNames = ConfigurationManager.AppSettings["CrmConnectionKVName"].Split(',');
            int i = 1;
            foreach (var crmConnectionKVName in crmConnectionKVNames)
            {
                string connectionString = KeyVaultUtils.GetSecretByNameAsync(crmConnectionKVName);
               //string connectionString = "AuthType=ClientSecret;url=https://altshulerdev.crm4.dynamics.com;ClientId=d370b8c2-8b79-4cb6-9c22-6b088197257f;ClientSecret=ibd8Q~weiAEDW1w1iotidQ.FP2cRqJwCHbGnNbS.";
                CrmServiceManager crmServiceManager = new CrmServiceManager(connectionString);
                connectionPool.TryAdd(i++, crmServiceManager);
            }
        }

        public CrmServiceManager GetConnection()
        {
            int key = connectionPool.Count == 1 ? 1 :
                (currentItemIndex == connectionPool.Count
                    ? currentItemIndex--
                        : currentItemIndex++);
            return connectionPool.ContainsKey(key) ? connectionPool[key] : connectionPool.FirstOrDefault().Value;
        }
    }
}
