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

        [Route("api/login/get_creds/")]
        public object GetMessage()
        {
            try
            {
                return new Users();
            }
            catch (Exception excp)
            {
                IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "getting base cred");
                return null;
            }
        }

        private string CheckRegistrationGroup(inctrakContext context, string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return "Please enter a valid group name, try again";
            }
            else if (context.Groups.Where(g => g.Description == groupName).FirstOrDefault() != null)
            {
                return "That group already exists, you must be invited to join, try again";
            }
            else
            {
                string groupKey = ServiceUtil.CleanNonAlpha(groupName);
                if (context.Groups.Where(g => g.GroupKey == groupKey).FirstOrDefault() != null)
                {
                    return "That group key already exists, you must be invited to join, try again";
                }
            }
            return null;
        }

        private string CheckRegistrationData(inctrakContext context, string userName, string emailAddres, bool verifiedEmail, string groupName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return "Please enter a username";
            }
            else if (context.Users.Where(up => up.UserName == userName).FirstOrDefault() != null)
            {
                return "That user name is already in use, try again";
            }
            else if (!verifiedEmail)
            {
                return "Please user a verified email";
            }
            else if (string.IsNullOrWhiteSpace(emailAddres))
            {
                return "Please enter email";
            }
            else if (context.Users.Where(up => up.EmailAddress == emailAddres).FirstOrDefault() != null)
            {
                return "That email is already in use, try again";
            }

            return CheckRegistrationGroup(context, groupName);
        }

        [Route("api/login/register_internal/")]
        [HttpPost]
        public ActionResult PostRegisterInternal(USER_UI formData)
        {
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    string errorMessage = null;
                    if (formData == null || (string.IsNullOrWhiteSpace(formData.USER_NAME) && string.IsNullOrWhiteSpace(formData.PASSWORD)))
                    {
                        return Ok(new { success = false, message = "Please enter username and password" });
                    }
                    else if (!formData.IS_REGISTERING)
                    {
                        return Ok(new { success = false, message = "Please select registering checkbox" });
                    }
                    else if (!formData.ACCEPT_TERMS)
                    {
                        return Ok(new { success = false, message = "Please read and accept the terms and conditions" });
                    }
                    else if (string.IsNullOrWhiteSpace(formData.PASSWORD) || string.IsNullOrWhiteSpace(formData.PASSWORD2))
                    {
                        return Ok(new { success = false, message = "Please enter passwords" });
                    }
                    else if (string.Compare(formData.PASSWORD, formData.PASSWORD2) != 0)
                    {
                        return Ok(new { success = false, message = "Passwords do not match" });
                    }
                    else if ((errorMessage = CheckRegistrationData(context, formData.USER_NAME, formData.EMAIL_ADDRESS, true, formData.GROUP_NAME)) != null)
                    {
                        return Ok(new { success = false, message = errorMessage });
                    }
                    else
                    {
                        Groups group = new Groups();
                        group.Description = formData.GROUP_NAME;
                        group.GroupKey = ServiceUtil.CleanNonAlpha(formData.GROUP_NAME);
                        context.Groups.Add(group);

                        Users user = new Users();
                        user.Administrator = true;
                        user.AcceptTerms = true;
                        user.UserName = formData.USER_NAME;
                        user.EmailAddress = formData.EMAIL_ADDRESS;
                        user.Password = "placeholder";
                        user.Activated = false;
                        user.GoogleLogon = false;

                        ActivateUuids uuid = SaveUuid(context, user, EnumUuidType.Login, false);
                        user.ActivateUuids.Add(uuid);
                        group.Users.Add(user);
                        user.GroupFkNavigation = group;

                        context.SaveChanges();
                        user.Password = GetPasswordHash(user, formData.PASSWORD);
                        context.SaveChanges();
                        SendMail(user.EmailAddress, "IncTrak: Activate account link",
                                    string.Format("Hi {0},<br/>Please 'click' the following link to acctivate your account<br/><br/><a href='{1}'>ACTIVATE ACCOUNT</a><br/><br/>Or paste the following url into your browser.<br/><br/>{1}<br/><br/>This link will be valid for 24 hours, Thanks", user.UserName, GetIncTrakUrl("/#/auth/activate/{0}", uuid.Uuid)));

                        return Ok(new { success = true, message = string.Format("Thanks for registering {0}, you will get an email shortly to activate your account. It will contain a link that will be valid for 24 hours. - thx", formData.USER_NAME) });
                    }

                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "registration base cred");
                return Ok(new { success = false, message = message });
            }
        }

        [Route("api/login/login_internal/")]
        [HttpPost]
        public ActionResult PostLoginInternal(USER_UI formData)
        {
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    if (formData == null || (string.IsNullOrWhiteSpace(formData.USER_NAME) && string.IsNullOrWhiteSpace(formData.PASSWORD)))
                    {
                        return Ok( new { success = false, message = "Please enter username and password" });
                    }
                    else
                    {
                        if (formData.IS_REGISTERING == false)
                        {
                            var loginUser = context.Users.Where(up => up.UserName == formData.USER_NAME || up.EmailAddress == formData.USER_NAME).FirstOrDefault();
                            if (loginUser == null)
                            {
                                return Ok( new { success = false, message = "Invalid username/email, try again" });
                            }
                            else
                            {
                                string password = GetPasswordHash(loginUser, formData.PASSWORD);
                                if (password != loginUser.Password)
                                {
                                    return Ok( new { success = false, message = "Invalid password, try again" });
                                }
                                else if (loginUser.Activated == false)
                                {
                                    return Ok( new { success = false, message = "You have not activated your account yet" });
                                }
                                else
                                {
                                    return Ok( new { success = true, uuid = SaveUuid(context, loginUser, EnumUuidType.Login, true).Uuid, Role = UserRole(loginUser), message = "Welcome back!" });
                                }
                            }
                        }
                        else
                        {
                            return Ok( new { success = false, message = "Please do not select registering to login" });
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "login base cred");
                return Ok( new { success = false, message = message });
            }
        }

        [Route("api/login/register_google/")]
        [HttpPost]
        public ActionResult PostRegisterGoogle(USER_UI formData)
        {
            return RetiredGoogleAuthResponse();
        }

        [Route("api/login/register_google_user/")]
        [HttpGet]
        public RedirectResult RegisterGoogleUser(string code, string state, string session_state)
        {
            return RetiredGoogleAuthRedirect();
        }

        [Route("api/login/login_google/")]
        [HttpPost]
        public ActionResult PostLoginGoogle(USER_UI formData)
        {
            return RetiredGoogleAuthResponse();
        }

        [Route("api/login/login_google_user/")]
        [HttpGet]
        public RedirectResult LoginGoogleUser(string code, string state, string session_state)
        {
            return RetiredGoogleAuthRedirect();
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

        private OkObjectResult RetiredGoogleAuthResponse()
        {
            return Ok(new
            {
                success = false,
                message = "Google sign-in has been retired. Use the Supabase login flow instead."
            });
        }

        private RedirectResult RetiredGoogleAuthRedirect()
        {
            string message = Uri.EscapeDataString("Google sign-in has been retired. Use the new login flow instead.");
            return Redirect(string.Format("/#/auth/login?redirect=true&success=false&message={0}", message));
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

        private string Decrypt(string cipherText, string passPhrase, string salt)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(salt);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
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
