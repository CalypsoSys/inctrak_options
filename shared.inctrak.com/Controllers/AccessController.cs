using IncTrak.data;
using IncTrak.Data;
using IncTrak.Domain;
using IncTrak.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IncTrak.Controllers
{
    [ApiController]
    public class AccessController : IncTrakController
    {
        private enum EnumUuidType { Login = 1, ResetPassword = 2, AcceptTerms = 3 };

        public AccessController(IOptions<AppSettings> config) : base (config)
        {
        }


        [Route("api/login/activateaccount/")]
        [HttpPost]
        public ActionResult PostActivateAccount(ActivateAccount activateaccount)
        {
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    var uuid = GetUserKey(context, activateaccount.ActivateKey, EnumUuidType.Login);
                    if (uuid == null || DateTime.Now > uuid.ValidUntil || uuid.UserFkNavigation == null)
                    {
                        return Ok( new { success = false, message = "This activation link has expired" });
                    }
                    else
                    {
                        var user = uuid.UserFkNavigation;
                        user.Activated = true;
                        context.SaveChanges();
                        return Ok( new { success = true, message = "Your account has been reset, please login" });
                    }
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "post activate");
                return Ok( new { success = false, message = message });
            }
        }

        [Route("api/login/resetpassword/")]
        [HttpPost]
        public ActionResult PostResetPassword(ResetPassword userNameEmail)
        {
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    if (userNameEmail == null || string.IsNullOrWhiteSpace(userNameEmail.UserNameEmail))
                    {
                        return Ok( new { success = false, message = "Please enter username or email" });
                    }
                    else
                    {
                        var user = context.Users.Where(up => up.UserName == userNameEmail.UserNameEmail || up.EmailAddress == userNameEmail.UserNameEmail).FirstOrDefault();
                        if (user != null)
                        {
                            string uuid = SaveUuid(context, user, EnumUuidType.ResetPassword, true).Uuid;

                            SendMail(user.EmailAddress, "IncTrak: Reset password link",
                                                string.Format("Hi {0},<br/>Please 'click' the following link to reset your password<br/><br/><a href='{1}'>RESET PASSWORD</a><br/><br/>Or paste the following url into your browser.<br/><br/>{1}<br/><br/>This link will be valid for 24 hours, Thanks", user.UserName, GetIncTrakUrl("/#/auth/reset-password/{0}/", uuid)));

                            return Ok( new { success = true, message = "A link to reset your password has been mailed to your account. It will contain a link that will be valid for 24 hours." });
                        }
                        else
                        {
                            return Ok( new { success = false, message = "Could not find your username or email address" });
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "post reset pwd");
                return Ok( new { success = false, message = message });
            }
        }

        [Route("api/login/resetpasswordlink/")]
        [HttpPost]
        public ActionResult PostResetPasswordLink(ResetPasswordLink passwordReset)
        {
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    if (passwordReset == null || !passwordReset.AcceptTerms)
                    {
                        return Ok( new { success = false, message = "Please read and accept terms and conditions" });
                    }
                    else if (passwordReset == null || string.IsNullOrWhiteSpace(passwordReset.ResetPasswordKey) ||
                        string.IsNullOrWhiteSpace(passwordReset.Password1) || string.IsNullOrWhiteSpace(passwordReset.Password2))
                    {
                        return Ok( new { success = false, message = "Please enter passwords" });
                    }
                    else if (string.Compare(passwordReset.Password1, passwordReset.Password2) != 0)
                    {
                        return Ok( new { success = false, message = "Passwords do not match" });
                    }
                    else
                    {
                        var uuid = GetUserKey(context, passwordReset.ResetPasswordKey, EnumUuidType.ResetPassword);
                        if (uuid == null || DateTime.Now > uuid.ValidUntil || uuid.UserFkNavigation == null)
                        {
                            return Ok( new { success = false, message = "This password reset link has expired" });
                        }
                        else
                        {
                            var user = uuid.UserFkNavigation;
                            user.Password = GetPasswordHash(user, passwordReset.Password1);
                            user.AcceptTerms = passwordReset.AcceptTerms;
                            context.SaveChanges();
                            return Ok( new { success = true, message = "Your password has been reset, please login with new password" });
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "reset passwd link");
                return Ok( new { success = false, message = message });
            }
        }

        [Route("api/login/accept_terms/")]
        [HttpPost]
        public ActionResult AcceptTerms(AcceptTermsLink acceptTerms)
        {
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    if (acceptTerms == null || string.IsNullOrWhiteSpace(acceptTerms.AcceptTermsKey) || !acceptTerms.AcceptTerms)
                    {
                        return Ok( new { success = false, message = "Please read and accept terms and conditions" });
                    }
                    else
                    {
                        var uuid = GetUserKey(context, acceptTerms.AcceptTermsKey, EnumUuidType.AcceptTerms);
                        if (uuid == null || DateTime.Now > uuid.ValidUntil || uuid.UserFkNavigation == null)
                        {
                            return Ok( new { success = false, message = "This accept terms link has expired" });
                        }
                        else
                        {
                            var user = uuid.UserFkNavigation;
                            user.AcceptTerms = acceptTerms.AcceptTerms;
                            var loginUuid = SaveUuid(context, user, EnumUuidType.Login, false);
                            context.SaveChanges();
                            return Ok( new { success = true, message = "Thanks for accepting terms", uuid = loginUuid.Uuid, Role = UserRole(user) });
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "accept terms");
                return Ok( new { success = false, message = message });
            }
        }

        private string UserRole(Users user)
        {
            return (user.Administrator ? "admin" : "optionee");
        }

        public static Users GetLoginUserKey(inctrakContext context, string uuidKey)
        {
            ActivateUuids uuid = GetUserKey(context, uuidKey, EnumUuidType.Login);
            if (uuid == null)
                return null;
            else
                return uuid.UserFkNavigation;
        }

        private static ActivateUuids GetUserKey(inctrakContext context, string uuidKey, EnumUuidType type)
        {
            var uuid = context.ActivateUuids.Where(u => u.Uuid == uuidKey && u.Type == (int)type).FirstOrDefault();
            if (uuid == null || DateTime.Now > uuid.ValidUntil)
                return null;
            else
                return uuid;
        }

        public static string ParticipantResetUuid(IOptions<AppSettings> options, inctrakContext context, Users user)
        {
            AccessController ac = new AccessController(options);
            return ac.SaveUuid(context, user, EnumUuidType.ResetPassword, false).Uuid;
        }

        public static void ParticpantResetEmail(IOptions<AppSettings> options, Users user, string uuid)
        {
            AccessController ac = new AccessController(options);
            ac.SendMail(user.EmailAddress, "IncTrak: Reset/activate account/password link",
                                string.Format("Hi {0},<br/>Please 'click' the following link to reset/activate your account/password<br/><br/><a href='{1}'>RESET PASSWORD</a><br/><br/>Or paste the following url into your browser.<br/><br/>{1}<br/><br/>This link will be valid for 24 hours, Thanks", user.UserName, ac.GetIncTrakUrl("/#/auth/reset-password/{0}/", uuid)));
        }

        private ActivateUuids SaveUuid(inctrakContext context, Users user, EnumUuidType type, bool save, bool readValueOnly = false)
        {
            ActivateUuids uuid = user.ActivateUuids.SingleOrDefault(u => u.UserFk == user.UserPk && u.Type == (int)type);
            if (uuid == null)
            {
                uuid = new ActivateUuids() { Type = (int)type, UserFk = user.UserPk };
                context.ActivateUuids.Add(uuid);
                readValueOnly = false;
            }

            if (!readValueOnly)
            {
                uuid.Uuid = Guid.NewGuid().ToString("N");
                uuid.ValidUntil = DateTime.Now.AddDays(1);
            }
            if (save)
                context.SaveChanges();

            return uuid;
        }

        private string GetPasswordHash(Users user, string passPhrase)
        {
            decimal salt1 = user.UserPk.GetHashCode();
            decimal salt2 = user.UserPk.GetHashCode();
            for (int i = 1; salt1.ToString().Length <= 8; i++)
            {
                if (salt1 == 0)
                    break;
                salt1 *= i;
                salt2 *= i + 1;
            }
            return Encrypt(string.Format("{0}{1}", user.UserName, user.UserPk), passPhrase, salt1.ToString().Substring(0, 8) + salt2.ToString().Substring(0, 8));
        }

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;
        private string Encrypt(string plainText, string passPhrase, string salt)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(salt);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

    }
}
