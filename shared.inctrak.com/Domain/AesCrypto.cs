using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace IncTrak.Domain
{
    // all for nothing as not a machine key, but makes me feel better.....
    public class AesCrypto
    {
        public static string DecryptStringAES(string cipherText, string sharedSecret, string saltString)
        {
            var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(false).OfType<GuidAttribute>();
            string salt2 = attrs.First().Value;
            byte[] salt = Encoding.ASCII.GetBytes(string.Format("{0}{1}", saltString, salt2.ToUpper()));

            return DecryptStringAES(cipherText, sharedSecret, salt);
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        private static string DecryptStringAES(string cipherText, string sharedSecret, byte[] salt)
        {
            using (RijndaelManaged aesAlg = new RijndaelManaged())
            {
                string plaintext = null;

                try
                {
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, salt);

                    byte[] bytes = Convert.FromBase64String(cipherText);
                    using (MemoryStream msDecrypt = new MemoryStream(bytes))
                    {
                        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                        aesAlg.IV = ReadByteArray(msDecrypt);
                        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                finally
                {
                    if (aesAlg != null)
                        aesAlg.Clear();
                }

                return plaintext;
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}