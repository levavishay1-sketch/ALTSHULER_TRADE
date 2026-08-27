using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using System;
using System.IO;

namespace Alt.Framework.External.Utils
{
    public static class FileUtils
    {
        public static byte[] ProtectFile(string base64File, string userPass)
        {
            byte[] fileContents = null;

            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64File)))
            {
                using (PdfDocument document = PdfReader.Open(stream, PdfDocumentOpenMode.Modify))
                {
                    PdfSecuritySettings securitySettings = document.SecuritySettings;

                    // Setting one of the passwords automatically sets the security level to PdfDocumentSecurityLevel.Encrypted128Bit.
                    securitySettings.UserPassword = userPass;

                    // Restrict some rights.
                    securitySettings.PermitAnnotations = false;
                    securitySettings.PermitAssembleDocument = false;
                    securitySettings.PermitExtractContent = false;
                    securitySettings.PermitFormsFill = true;
                    securitySettings.PermitFullQualityPrint = false;
                    securitySettings.PermitModifyDocument = true;
                    securitySettings.PermitPrint = false;
                    // securitySettings.PermitAccessibilityExtractContent = false;

                    // relevant only to protect file modification
                    //securitySettings.OwnerPassword = "owner"; 

                    // Don't use 40 bit encryption unless needed for compatibility reasons
                    //securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        document.Save(outStream, true);
                        fileContents = outStream.ToArray();
                    }
                }
            }
            return fileContents;
        }
    }
}
