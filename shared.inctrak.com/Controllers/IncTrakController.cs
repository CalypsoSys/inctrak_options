using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IncTrak.Data;
using IncTrak.Models;
using inctrak.com;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Web;

namespace IncTrak.Controllers
{
    public abstract class IncTrakController : ControllerBase
    {
        protected readonly IOptions<AppSettings> _options;

        protected IncTrakController(IOptions<AppSettings> options)
        {
            _options = options;
        }

        protected string LoginBaseUrl(string redirect)
        {
            return GetIncTrakApiUrl("/api/login/{0}/", redirect);
        }

        protected string GetIncTrakUrl(string path, params object[] args)
        {
            return BuildUrl(_options.Value.GetIncTrakDns(), "https://shared.inctrak.com/", path, args);
        }

        protected string GetIncTrakApiUrl(string path, params object[] args)
        {
            return BuildUrl(GetRequestOrigin(), "https://shared.inctrak.com/", path, args);
        }

        protected string GetRequestOrigin()
        {
            if (Request?.Host.HasValue == true && string.IsNullOrWhiteSpace(Request.Scheme) == false)
            {
                string pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
                return $"{Request.Scheme}://{Request.Host}{pathBase}";
            }

            return "https://shared.inctrak.com/";
        }

        private static string BuildUrl(string configuredUrl, string defaultUrl, string path, params object[] args)
        {
            string url = string.IsNullOrWhiteSpace(configuredUrl) ? defaultUrl : configuredUrl;

            if (string.IsNullOrWhiteSpace(path) == false)
            {
                if (args != null && args.Length > 0)
                    path = string.Format(path, args);
                url = url.TrimEnd('/');
                path = path.TrimStart('/');
                return string.Format("{0}/{1}", url, path);
            }
            else
            {
                return url;
            }
        }

        protected LoginRights GetLoginUser()
        {
            return GetLoginUser(null, Guid.Empty.ToString(), true);
        }

        protected LoginRights GetLoginUser(inctrakContext context, string passedUuid, bool noNull = false)
        {
            Guid groupKey = Guid.Empty;
            Guid userKey = Guid.Empty;
            string uuidKey = passedUuid;
            try
            {
                uuidKey = RequestAuthReader.GetUuid(Request);
                if (passedUuid != Guid.Empty.ToString() && uuidKey != passedUuid)
                {
                    if (noNull)
                        return new LoginRights(passedUuid, false, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
                    else
                        return null;
                }

                if (context != null)
                {
                    Users user = AccessController.GetLoginUserKey(context, uuidKey);
                    if (user != null)
                    {
                        if (user.Administrator)
                            groupKey = user.GroupFk;
                        else
                            userKey = user.UserPk;
                        return new LoginRights(uuidKey, user.Administrator, groupKey, userKey, user.UserPk, user.GroupFk);
                    }
                }
            }
            catch
            {
            }

            if (noNull)
                return new LoginRights(uuidKey, false, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
            else
                return null;
        }

        protected const int SearchLimit = 50;
        protected enum SearchType { All, Any, Exact, InLine, None };
        protected SearchType SearchParts(string searchString, string searchType, out string part1, out string part2, out string part3, out string part4, out string part5, out string part6)
        {
            part1 = part2 = part3 = part4 = part5 = part6 = null;
            SearchType st = GetSearchType(searchType);
            if (st != SearchType.None && string.IsNullOrWhiteSpace(searchString) == false)
            {
                if (st == SearchType.Exact)
                {
                    part1 = searchString.Trim();
                    return SearchType.Exact;
                }
                else
                {
                    string[] parts = searchString.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 1)
                    {
                        part1 = parts[0];
                        if (parts.Length >= 2)
                        {
                            part2 = parts[1];
                            if (parts.Length >= 3)
                            {
                                part3 = parts[2];
                                if (parts.Length >= 4)
                                {
                                    part4 = parts[3];
                                    if (parts.Length >= 5)
                                    {
                                        part5 = parts[4];
                                        if (parts.Length >= 6)
                                        {
                                            part6 = parts[5];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return st;
                }
            }

            return SearchType.None;
        }

        private SearchType GetSearchType(string searchType)
        {
            if (string.IsNullOrWhiteSpace(searchType) == false)
            {
                SearchType st;
                if (Enum.TryParse<SearchType>(searchType.Trim(), true, out st))
                    return st;
            }

            return SearchType.None;
        }

        protected string GetClientInfo()
        {
            string clientInfo = "Unknown";
            try
            {
                if (Request.HttpContext.Connection != null)
                {
                    clientInfo = string.Format("ID: {0}\r\nUser: {1}\r\n", Request.HttpContext.Connection.RemoteIpAddress, Request.HttpContext.Connection.Id);
                }
            }
            catch (Exception cexcp)
            {
                clientInfo = cexcp.Message;
            }

            return clientInfo;
        }

        protected void SendMail(string to, string subject, string body)
        {
            // Auth/account mail flows are being retired. Keep the call sites harmless until
            // those flows are removed or replaced with a proper delivery mechanism.
        }

        protected void SendSlackMessage(string webhookUrl, string text)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                throw new InvalidOperationException("AppSettings:SlackFeedbackWebhookUrl must be configured for Slack notifications.");

            using (var client = new HttpClient())
            using (var content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json"))
            {
                var response = client.PostAsync(webhookUrl, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode == false)
                {
                    string responseBody = response.Content == null
                        ? string.Empty
                        : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    throw new InvalidOperationException(string.Format(
                        "Slack webhook rejected notification. StatusCode={0}, Body={1}",
                        (int)response.StatusCode,
                        string.IsNullOrWhiteSpace(responseBody) ? "(empty)" : responseBody));
                }
            }
        }

        public static string BuildSlackMailMessage(string to, string subject, string body)
        {
            return string.Format(
                "Notification\nTo: {0}\nSubject: {1}\nMessage: {2}",
                string.IsNullOrWhiteSpace(to) ? "unknown" : to.Trim(),
                string.IsNullOrWhiteSpace(subject) ? "none" : subject.Trim(),
                ConvertHtmlToSlackText(body));
        }

        public static string BuildFeedbackSlackMessage(string name, string emailAddress, string messageType, string clientData, string message)
        {
            return string.Format(
                "New feedback submitted\nType: {0}\nName: {1}\nEmail: {2}\nClient: {3}\nMessage: {4}",
                string.IsNullOrWhiteSpace(messageType) ? "unknown" : messageType.Trim(),
                string.IsNullOrWhiteSpace(name) ? "none" : name.Trim(),
                string.IsNullOrWhiteSpace(emailAddress) ? "none" : emailAddress.Trim(),
                FileLogWriter.SanitizeSingleLine(clientData),
                string.IsNullOrWhiteSpace(message) ? "none" : message.Trim());
        }

        private static string ConvertHtmlToSlackText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "none";

            string plainText = value
                .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("</p>", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("</div>", "\n", StringComparison.OrdinalIgnoreCase);

            plainText = Regex.Replace(plainText, "<[^>]+>", string.Empty);
            plainText = System.Net.WebUtility.HtmlDecode(plainText);
            plainText = plainText.Replace("\r\n", "\n").Replace('\r', '\n');
            plainText = Regex.Replace(plainText, "\n{3,}", "\n\n").Trim();

            return string.IsNullOrWhiteSpace(plainText) ? "none" : plainText;
        }
    }
}
