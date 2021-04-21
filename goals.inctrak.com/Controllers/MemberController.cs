using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using IncTrak.GoalSetter.Models;
using IncTrak.GoalSetter.Data;
using IncTrak.GoalSetter.data;
using Microsoft.Extensions.Options;

namespace IncTrak.GoalSetter.Controllers
{
    [ApiController]
    public class MemberController : IncTrakController
    {
        public MemberController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object QueryMembers(inctrak_goalsContext context, LoginRights rights, string searchString, string searchType)
        {
            string part1, part2, part3, part4, part5, part6;
            SearchType st = SearchParts(searchString, searchType, out part1, out part2, out part3, out part4, out part5, out part6);

            IQueryable<Members> query;
            if (st == SearchType.None || (st == SearchType.InLine && searchString == "_____"))
            {
                if ((searchString == "_____" && searchType == "_____") ||  (st == SearchType.InLine && searchString == "_____"))
                    query = (from p in context.Members
                             where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                             select p).Take(10);
                else
                    query = from p in context.Members
                            where 1 == 2 && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                            select p;
            }
            else if (st == SearchType.Any || st == SearchType.InLine)
            {
                query = from p in context.Members
                        where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) && 
                        (
                            p.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) ||
                            (part2 != null && p.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part3 != null && p.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part4 != null && p.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part5 != null && p.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part6 != null && p.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select p;
            }
            else
            {
                query = from p in context.Members
                        where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) &&
                        (
                            p.Name.Contains(part1) &&
                            (part2 == null || p.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part3 == null || p.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part4 == null || p.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part5 == null || p.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part6 == null || p.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select p;
            }

            if (st == SearchType.InLine)
            {
                return query.Select(p => new MEMBER_UI(rights.GroupKeyCheck) { SetFromMember = p, CURRENT_GOALS = p.Goalset.Where(g => g.ScheduleFkNavigation.EndDate > DateTime.Now).Sum(g => g.Goals.Count()), TOTAL_GOALS = p.Goalset.Sum(g => g.Goals.Count()), HAS_USER = p.UserFk != null }).ToArray();
            }
            else
            {
                var existingMember = query.Select(p => new MEMBER_UI(rights.GroupKeyCheck) { SetFromMember = p, CURRENT_GOALS = p.Goalset.Where(g => g.ScheduleFkNavigation.EndDate > DateTime.Now).Sum(g => g.Goals.Count()), TOTAL_GOALS = p.Goalset.Sum(g => g.Goals.Count()), HAS_USER = p.UserFk != null }).ToArray();
                if (rights.IsAdmin)
                {
                    var newMember = new MEMBER_UI[1] { new MEMBER_UI(rights.GroupKeyCheck, new Members { MemberPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Member>" }) };
                    return newMember.Union(existingMember).ToArray();
                }

                return existingMember.ToArray();
            }
        }

        [Route("api/company/members/{searchString}/{searchType}/")]
        public object GetMembers(string searchString, string searchType)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryMembers(context, rights, searchString, searchType);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get members");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/member/{memberKey}/{uuidKey}")]
        public object GetMember(Guid memberKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Members member;
                    if (memberKey == Guid.Empty)
                        member = new Members() { GroupFk = rights.GroupKeyCheck };
                    else
                        member = (from s in context.Members
                                    where s.MemberPk == memberKey && (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).FirstOrDefault();
                    string userName=null, emailAddress=null;
                    bool googleUser = false;
                    if (rights.IsAdmin && member.UserFk.HasValue )
                    {
                        var user = (from u in context.Users
                                    where u.UserPk == member.UserFk.Value && u.GroupFk == rights.GroupKey
                                    select u).FirstOrDefault();
                        if ( user != null )
                        {
                            userName = user.UserName;
                            emailAddress = user.EmailAddress;
                            googleUser = user.GoogleLogon;
                        }
                    }

                    return new { success = true, Member = new MEMBER_UI(rights.GroupKeyCheck, member) { USER_NAME = userName, EMAIL_ADDRESS = emailAddress, GOOGLE_USER = googleUser } };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a member");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/member/")]
        [HttpPost]
        public ActionResult SaveMember(SaveData<MEMBER_UI> saveMember)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveMember.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveMember.Key != saveMember.Data.MEMBER_PK)
                        return Ok(new { success = false, message = "Invalid member, keys do not match - thx" });


                    Members member;
                    if (saveMember.Key == Guid.Empty)
                    {
                        member = saveMember.Data.GetMember(rights.GroupKey);
                        context.Members.Add(member);
                    }
                    else
                    {
                        member = (from s in context.Members
                                             where s.MemberPk == saveMember.Key && s.GroupFk == rights.GroupKey
                                     select s).FirstOrDefault();
                        if (member == null)
                            return Ok(new { success = false, message = "Cannot find this member - thx" });
                        saveMember.Data.SetToMember(member, rights.GroupKey);
                    }

                    string uuid = null;
                    if (saveMember.Data.USER_ACTION == "create_user" || saveMember.Data.USER_ACTION == "update_user") {
                        var newUser = (from u in context.Users
                                      where (u.EmailAddress == saveMember.Data.EMAIL_ADDRESS || u.UserName == saveMember.Data.USER_NAME) &&
                                      u.UserPk != member.UserFk
                                      select u).Count();
                        if ( newUser > 0 )
                            return Ok(new { success = false, message = "Invalid user, username or email address already exist - thx" });

                        if ( saveMember.Data.USER_ACTION == "update_user" )
                        {
                            if (saveMember.Data.GOOGLE_USER)
                            {
                                member.UserFkNavigation.GoogleLogon = true;
                                member.UserFkNavigation.UserName = "google_user";
                                member.UserFkNavigation.Activated = false;
                            }
                            else
                            {
                                member.UserFkNavigation.GoogleLogon = false;
                                member.UserFkNavigation.UserName = saveMember.Data.USER_NAME;
                            }
                            member.UserFkNavigation.EmailAddress = saveMember.Data.EMAIL_ADDRESS;
                        }
                        else
                        {
                            Users user = new Users();
                            user.Administrator = false;
                            user.AcceptTerms = false;
                            user.EmailAddress = saveMember.Data.EMAIL_ADDRESS;
                            if (saveMember.Data.GOOGLE_USER)
                            {
                                user.GoogleLogon = true;
                                user.UserName = "google_user";
                                user.Activated = false;
                                user.Password = "google";
                            }
                            else
                            {
                                user.GoogleLogon = false;
                                user.UserName = saveMember.Data.USER_NAME;
                                user.Activated = true;
                                user.Password = "placeholder";
                            }
                            user.GroupFk = rights.GroupKey;
                            context.Users.Add(user);

                            member.UserFkNavigation = user;
                        }
                        uuid = AccessController.MemberResetUuid(_options, context, member.UserFkNavigation);

                    }
                    else if (saveMember.Data.USER_ACTION == "delete_user" && member.UserFk.HasValue)
                    {
                        if (member.UserFkNavigation != null)
                            context.Users.Remove(member.UserFkNavigation);
                        member.UserFk = null;
                    }

                    context.SaveChanges();
                    if (uuid != null && saveMember.Data.SEND_EMAIL)
                        AccessController.MemberResetEmail(_options, member.UserFkNavigation, uuid);
                    return Ok(new { success = true, message = "Member saved.", key=member.MemberPk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save member");
                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/member/{memberKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteMember(Guid memberKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (memberKey != Guid.Empty)
                    {
                        Members member = (from s in context.Members
                                             where s.MemberPk == memberKey && s.GroupFk == rights.GroupKey
                                             select s).FirstOrDefault();
                        if (member != null)
                        {
                            context.Members.Remove(member);
                            context.SaveChanges();
                            return new { success = true, Members = QueryMembers(context, rights, "", "") };
                        }
                    }
                }
                throw new Exception("Cannot find member");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save member");
                return new { success = false, message = message }; ;
            }
        }
    }
}