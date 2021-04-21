using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IncTrak.GoalSetter.Data;
using IncTrak.GoalSetter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IncTrak.GoalSetter.Controllers
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
            return GetIncTrakUrl("/api/login/{0}/", redirect);
        }

        protected string GetIncTrakUrl(string path, params object[] args)
        {
            string url = null;
            try
            {
                url = _options.Value.IncTrakDns;
            }
            catch { }

            if (string.IsNullOrWhiteSpace(url))
            {
                url = "https://goals.inctrak.com/";
            }

            if (string.IsNullOrWhiteSpace(path) == false)
            {
                if (args != null || args.Length > 0)
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

        protected LoginRights GetLoginUser(inctrak_goalsContext context, string passedUuid, bool noNull = false)
        {
            Guid groupKey = Guid.Empty;
            Guid userKey = Guid.Empty;
            string uuidKey = passedUuid;
            try
            {
                Request.Cookies.TryGetValue("UUID", out uuidKey);
                string memberView;
                Request.Cookies.TryGetValue("MEMBER_VIEW", out memberView);
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
                        if (user.Administrator && memberView != "true")
                            groupKey = user.GroupFk;
                        else
                            userKey = user.UserPk;
                        try
                        {
                            return new LoginRights(uuidKey, user.Administrator, groupKey, userKey, user.UserPk, user.GroupFk);
                        }
                        finally
                        {
                            if ( context is GoalsContext)
                                ((GoalsContext)context).SetContext(user);
                        }
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
     }
}