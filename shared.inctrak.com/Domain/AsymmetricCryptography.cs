using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IncTrak.Domain
{
    public class AsymmetricCryptography
    {
        protected static string Decrypt(string privateKey, string data)
        {
            RSACryptoServiceProvider rsaPrivate = null;
            try
            {
                CspParameters cspParam = new CspParameters();
                cspParam.Flags = CspProviderFlags.UseMachineKeyStore;

                rsaPrivate = new RSACryptoServiceProvider(cspParam);
                // Delete the key entry in the container.
                rsaPrivate.PersistKeyInCsp = false;
                rsaPrivate.ImportCspBlob(Convert.FromBase64String(privateKey));

                return Encoding.Unicode.GetString(rsaPrivate.Decrypt(Convert.FromBase64String(data), false));
            }
            finally
            {
                // Delete the key entry in the container.
                rsaPrivate.Clear();
            }
        }
    }
}
