using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IncTrak.Data
{
    public class AppSettings
    {
        private const int _keysize = 256;

        public string IncTrakDns { get; set; }
        public string IncTrakApiDns { get; set; }
        public string GoogleSecretKey { get; set; }
        public string GoogleClientId { get; set; }
        public string ErrorsHost { get; set; }
        public string ErrorsUsername { get; set; }
        public string ErrorsPassword { get; set; }
        public string IncTrakConnection { get; set; }
        public string FeedbackConnection { get; set; }
        public bool UseSNMP { get; set; }
        public string SNMPServer { get; set; }
        public int SNMPPort { get; set; }
        public string SNMPAddress { get; set; }
        public string SNMPPassword { get; set; }
        public string EmailApiKey { get; set; }
        public string EmailFrom { get; set; }

        public string GetIncTrakDns()
        {
            return Decrypt(IncTrakDns);
        }

        public string GetIncTrakApiDns()
        {
            if (string.IsNullOrWhiteSpace(IncTrakApiDns))
            {
                return null;
            }

            return Decrypt(IncTrakApiDns);
        }

        public string GetGoogleSecretKey()
        {
            return Decrypt(GoogleSecretKey);
        }
        public string GetGoogleClientId()
        {
            return Decrypt(GoogleClientId);
        }

        public string GetErrorsHost()
        {
            return Decrypt(ErrorsHost);
        }
        public string GetErrorsUsername()
        {
            return Decrypt(ErrorsUsername);
        }
        public string GetErrorsPassword()
        {
            return Decrypt(ErrorsPassword);
        }

        public string GetIncTrakConnection()
        {
            return Decrypt(IncTrakConnection);
        }
        public string GetFeedbackConnection()
        {
            return Decrypt(FeedbackConnection);
        }

        public string GetSNMPServer()
        {
            return Decrypt(SNMPServer);
        }
        public string GetSNMPAddress()
        {
            return Decrypt(SNMPAddress);
        }
        public string GetSNMPPassword()
        {
            return Decrypt(SNMPPassword);
        }

        public string GetEmailApiKey()
        {
            return Decrypt(EmailApiKey);
        }
        public string GetEmailFrom()
        {
            return Decrypt(EmailFrom);
        }

        private Tuple<string, byte[]> GetAppHash()
        {
            string myCode = "o1EdyJSCtJlLbth7u8HUjJ/U6bb0+17lNYbIhbhxzFoHF923nWPQTRXyq4Tma/Mo";
            string companyDesc = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyDescriptionAttribute>().First().Description;
            byte[] salt = new UnicodeEncoding().GetBytes(Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyCompanyAttribute>().First().Company.Substring(0, 8));

            string passPhrase = Decrypt(myCode, companyDesc, salt);
            return Tuple.Create(passPhrase, salt);
        }

        private string Decrypt(string cipherText)
        {
            var tup = GetAppHash();

            return Decrypt(cipherText, tup.Item1, tup.Item2);
        }
        private string Decrypt(string cipherText, string passPhrase, byte[] initVectorBytes)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(_keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
    }
}
