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
    public class TeamController : IncTrakController
    {
        public TeamController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryTeams(inctrak_goalsContext context, LoginRights rights)
        {
            IQueryable<Teams> query = from p in context.Teams
                                     where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select p;

            var existingTeams = query.Select(p => new TEAM_UI(rights.GroupKeyCheck) { SetFromTeam = p, DEPARTMENT_NAME = p.DepartmentFkNavigation.Name, CURRENT_GOALS = p.Goalset.Where(g => g.ScheduleFkNavigation.EndDate > DateTime.Now).Sum(g => g.Goals.Count()), TOTAL_GOALS = p.Goalset.Sum(g => g.Goals.Count()) }).ToArray();
            if (rights.IsAdmin)
            {
                var newTeam = new TEAM_UI[1] { new TEAM_UI(rights.GroupKeyCheck, new Teams() { TeamPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Team>" }) };

                return newTeam.Union(existingTeams).ToArray();
            }

            return existingTeams.ToArray();
        }

        [Route("api/company/teams/")]
        public object GetTeams()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryTeams(context, rights);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get teams");

                return new { success = false, message = message };
            }
        }

        [Route("api/company/team/{teamKey}/{uuidKey}")]
        public object GetTeam(Guid teamKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Teams team;
                    if (teamKey == Guid.Empty)
                        team = new Teams() { GroupFk = rights.GroupKeyCheck };
                    else
                        team = (from p in context.Teams
                                    where p.TeamPk == teamKey && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select p).FirstOrDefault();
                    if (team != null)
                    {
                        var departments = (from sc in context.Departments
                                            where sc.GroupFk == rights.GroupKey
                                            select sc).ToArray().Select(sc => new DEPARTMENTS_UI(rights.GroupKeyCheck) { SetFromDepartment = sc });

                        return new { success = true, Team = new TEAM_UI(rights.GroupKeyCheck, team), Departments = departments };
                    }
                }
                throw new Exception("Cannot find team");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a team");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/team/")]
        [HttpPost]
        public ActionResult SaveTeam(SaveData<TEAM_UI> saveTeam)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveTeam.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveTeam.Key != saveTeam.Data.TEAM_PK)
                        return Ok(new { success = false, message = "Invalid Team, keys do not match - thx" });

                    Teams team;
                    if (saveTeam.Key == Guid.Empty)
                    {
                        team = saveTeam.Data.GetTeam(rights.GroupKey);
                        context.Teams.Add(team);
                    }
                    else
                    {
                        team = (from p in context.Teams
                                where p.TeamPk == saveTeam.Key && p.GroupFk == rights.GroupKey
                                select p).FirstOrDefault();
                        if (team == null)
                            return Ok(new { success = false, message = "Cannot find this team, - thx" });
                        saveTeam.Data.SetToTeam(team, rights.GroupKey);
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Team saved.", key = team.TeamPk  });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save a team");
                return Ok(new { success = false, message = message });
            }
        }

        [Route("api/company/team/{teamKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteTeam(Guid teamKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (teamKey != Guid.Empty)
                    {
                        Teams team = (from p in context.Teams
                                     where p.TeamPk == teamKey && p.GroupFk == rights.GroupKey
                                     select p).FirstOrDefault();
                        if (team != null)
                        {
                            context.Teams.Remove(team);
                            context.SaveChanges();
                            return new { success = true, Teams = QueryTeams(context, rights) };
                        }
                    }
                    throw new Exception("Team not found");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove a team");
                return new { success = false, message = message };
            }
        }
    }
}