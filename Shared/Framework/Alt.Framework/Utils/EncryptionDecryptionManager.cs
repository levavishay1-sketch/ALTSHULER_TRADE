using Alt.DataModel.Crm.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.Utils
{
    public class EncryptionDecryptionManager
    {
        public static string EncryptText(string input, string key = null)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            // Hash the password with SHA256
            byte[] bytesEncrypted = EncryptionDecryptionManager.AES_EncryptOrDecrypt(bytesToBeEncrypted, SecurityOperation.Encrypt, key);
            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public static string DecryptText(string input, string key = null)
        {

            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] bytesDecrypted = EncryptionDecryptionManager.AES_EncryptOrDecrypt(bytesToBeDecrypted, SecurityOperation.Decrypt, key);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }

        public static byte[] AES_EncryptOrDecrypt(byte[] bytesToBeEncryptedOrDecrypted, SecurityOperation securityOperation, string key)
        {
            byte[] encryptedOrDecryptedBytes = null;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(key);
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var rfcKey = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = rfcKey.GetBytes(AES.KeySize / 8);
                    AES.IV = rfcKey.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    var aesEncryptOrDecryptCallBack = (securityOperation == SecurityOperation.Encrypt)
                        ? (Func<ICryptoTransform>)AES.CreateEncryptor
                        : (Func<ICryptoTransform>)AES.CreateDecryptor;

                    using (var cs = new CryptoStream(ms, aesEncryptOrDecryptCallBack.Invoke(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncryptedOrDecrypted, 0, bytesToBeEncryptedOrDecrypted.Length);
                        cs.Close();
                    }
                    encryptedOrDecryptedBytes = ms.ToArray();
                }
            }
            return encryptedOrDecryptedBytes;
        }
    }
}
