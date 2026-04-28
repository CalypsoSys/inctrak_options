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
using IncTrak.Models;
using IncTrak.Data;
using IncTrak.data;
using Microsoft.Extensions.Options;

namespace IncTrak.Controllers
{
    [ApiController]
    public class GrantController : IncTrakController
    {
        public GrantController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryGrants(inctrakContext context, LoginRights rights, string searchString, string searchType)
        {
            string part1, part2, part3, part4, part5, part6;
            SearchType st = SearchParts(searchString, searchType, out part1, out part2, out part3, out part4, out part5, out part6);

            IQueryable<Grants> query;
            if (st == SearchType.None)
            {
                if (searchString == "_____" && searchType == "_____")
                    query = (from g in context.Grants
                             where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                             select g).Take(10);
                else
                    query = from g in context.Grants
                            where 1 == 2 && (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                            select g;
            }
            else if (st == SearchType.Any)
            {
                query = from g in context.Grants
                        where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) && 
                        (
                            g.ParticipantFkNavigation.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) ||
                            (part2 != null && g.ParticipantFkNavigation.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part3 != null && g.ParticipantFkNavigation.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part4 != null && g.ParticipantFkNavigation.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part5 != null && g.ParticipantFkNavigation.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part6 != null && g.ParticipantFkNavigation.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select g;
            }
            else
            {
                query = from g in context.Grants
                        where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) &&
                        (
                            g.ParticipantFkNavigation.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) &&
                            (part2 == null || g.ParticipantFkNavigation.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part3 == null || g.ParticipantFkNavigation.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part4 == null || g.ParticipantFkNavigation.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part5 == null || g.ParticipantFkNavigation.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part6 == null || g.ParticipantFkNavigation.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select g;
            }

            var existingGrant = query.ToArray().Select(g => new GRANT_UI(rights.GroupKeyCheck) { SetFromGrant = g, PARTICIPANT_NAME = g.ParticipantFkNavigation.Name, PLAN_NAME = g.PlanFkNavigation.Name });
            if (rights.IsAdmin)
            {
                var newGrant = new GRANT_UI[1] { new GRANT_UI(rights.GroupKeyCheck, new Grants() { GrantPk = Guid.Empty, GroupFk = rights.GroupKeyCheck }) { PARTICIPANT_NAME = "<Create New Grant>", PLAN_NAME = "" } };
                return newGrant.Union(existingGrant).ToArray();
            }

            return existingGrant.ToArray();
        }

        [Route("api/company/grants/{searchString}/{searchType}/")]
        public object GetGrants(string searchString, string searchType)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryGrants(context, rights, searchString, searchType);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "grants");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/grant/{grantKey}")]
        public object GetGrant(Guid grantKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Grants grant;
                    if (grantKey == Guid.Empty)
                    {
                        grant = new Grants() {GroupFk = rights.GroupKeyCheck, DateOfGrant = DateTime.Now, VestingStart = DateTime.Now };
                    }
                    else
                    {
                        grant = (from s in context.Grants
                                    where s.GrantPk == grantKey && (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).FirstOrDefault();
                    }

                    if (grant != null)
                    {
                        PARTICIPANT_UI participant = null;
                        if (grant.ParticipantFk != Guid.Empty)
                            participant = (from p in context.Participants
                                           where p.ParticipantPk == grant.ParticipantFk && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                           select p).ToArray().Select(p => new PARTICIPANT_UI(rights.GroupKeyCheck) { SetFromParticipant = p }).FirstOrDefault();

                        var plans = (from p in context.Plans
                                     where p.GroupFk == rights.GroupKey
                                     select p).ToArray().Select( p => new PLAN_UI(rights.GroupKeyCheck) { SetFromPlan = p });
                        var vesting = (from s in context.Schedules
                                       where s.GroupFk == rights.GroupKey
                                       select s).ToArray().Select(s=>new SCHEDULE_UI(rights.GroupKeyCheck) { SetFromSchedule = s });
                        var terms = (from t in context.Terminations
                                     where t.GroupFk == rights.GroupKey
                                     select t).ToArray().Select(t=>new TERMINATION_UI(rights.GroupKeyCheck) { SetFromTermination = t });

                        return new { success = true, Grant = new GRANT_UI(rights.GroupKeyCheck, grant), Participant = participant, Plans = plans, Vesting = vesting, Terminations = terms };
                    }
                    throw new Exception("Could not find grant");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "grant");
                return new { success = false, message = message };
            }

        }

        [Route("api/company/grant/")]
        [HttpPost]
        public ActionResult SaveGrant(SaveData<GRANT_UI> saveGrant)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." } );
                    if (saveGrant.Key != saveGrant.Data.GRANT_PK)
                        return Ok(new { success = false, message = "Invalid grant, keys do not match - thx" });

                    Grants grant;
                    if (saveGrant.Key == Guid.Empty)
                    {
                        grant = saveGrant.Data.GetGrant(rights.GroupKey);
                        context.Grants.Add(grant);
                    }
                    else
                    {
                        grant = (from s in context.Grants
                                             where s.GrantPk == saveGrant.Key && s.GroupFk == rights.GroupKey
                                     select s).FirstOrDefault();
                        if (grant == null)
                            return Ok(new { success = false, message = "Cannot find this grant - thx" });
                        saveGrant.Data.SetToGrant(grant, rights.GroupKey);
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Grant saved.", key=grant.GrantPk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save grant");
                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/grant/{grantKey}")]
        [HttpDelete]
        public object DeleteGrant(Guid grantKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (grantKey != Guid.Empty)
                    {
                        Grants grant = (from s in context.Grants
                                             where s.GrantPk == grantKey && s.GroupFk == rights.GroupKey
                                             select s).FirstOrDefault();
                        if (grant != null)
                        {
                            context.Grants.Remove(grant);
                            context.SaveChanges();
                            return new { success = true, Grants = QueryGrants(context, rights, "", "") };
                        }
                    }
                }
                throw new Exception("Cannot find grant");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove grant");
                return new { success = false, message = message };
            }
        }
    }
}
