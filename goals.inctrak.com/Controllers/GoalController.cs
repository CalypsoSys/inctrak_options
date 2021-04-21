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
    public class GoalController : IncTrakController
    {
        public GoalController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryGoals(inctrak_goalsContext context, LoginRights rights, string searchString, string searchType)
        {
            string part1, part2, part3, part4, part5, part6;
            SearchType st = SearchParts(searchString, searchType, out part1, out part2, out part3, out part4, out part5, out part6);

            IQueryable<Models.Goalset> query;
            if (st == SearchType.None)
            {
                if (searchString == "_____" && searchType == "_____")
                    query = (from g in context.Goalset
                             where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                             select g).Take(10);
                else
                    query = from g in context.Goalset
                            where 1 == 2 && (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                            select g;
            }
            else if (st == SearchType.Any)
            {
                query = from g in context.Goalset
                        where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) && 
                        (
                            g.MemberFkNavigation.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) ||
                            (part2 != null && g.MemberFkNavigation.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part3 != null && g.MemberFkNavigation.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part4 != null && g.MemberFkNavigation.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part5 != null && g.MemberFkNavigation.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part6 != null && g.MemberFkNavigation.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select g;
            }
            else
            {
                query = from g in context.Goalset
                        where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) &&
                        (
                            g.MemberFkNavigation.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) &&
                            (part2 == null || g.MemberFkNavigation.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part3 == null || g.MemberFkNavigation.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part4 == null || g.MemberFkNavigation.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part5 == null || g.MemberFkNavigation.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part6 == null || g.MemberFkNavigation.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select g;
            }

            var existingGoalSet = query.Select(g => new GOALSET_UI(rights.GroupKeyCheck) { SetFromGoalSet = g, MEMBER_NAME = g.MemberFkNavigation.Name, TEAM_NAME = g.TeamFkNavigation.Name, SCHEDULE_NAME = g.ScheduleFkNavigation.Name }).ToArray();
            if (rights.IsAdmin)
            {
                var newGoalSet = new GOALSET_UI[1] { new GOALSET_UI(rights.GroupKeyCheck, new Models.Goalset() { GoalsetPk = Guid.Empty, GroupFk = rights.GroupKeyCheck }) { MEMBER_NAME = "<Create New Goals>", TEAM_NAME = "" } };
                return newGoalSet.Union(existingGoalSet).ToArray();
            }

            return existingGoalSet.ToArray();
        }

        [Route("api/company/goals/{searchString}/{searchType}/")]
        public object GetGoalSet(string searchString, string searchType)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryGoals(context, rights, searchString, searchType);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "goals");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/goal/{goalSetKey}/{uuidKey}")]
        public object GetGoal(Guid goalSetKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Models.Goalset goalSet;
                    if (goalSetKey == Guid.Empty)
                    {
                        goalSet = new Models.Goalset() { GroupFk = rights.GroupKeyCheck };
                    }
                    else
                    {
                        goalSet = (from s in context.Goalset
                                    where s.GoalsetPk == goalSetKey && (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).FirstOrDefault();
                    }

                    if (goalSet != null)
                    {
                        MEMBER_UI member = null;
                        if (goalSet.MemberFk != Guid.Empty)
                            member = (from p in context.Members
                                           where p.MemberPk == goalSet.MemberFk && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                           select p).ToArray().Select(p => new MEMBER_UI(rights.GroupKeyCheck) { SetFromMember = p }).FirstOrDefault();

                        var goals = (from g in context.Goals
                                       where g.GoalsetFk == goalSetKey && (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                       orderby g.ImportanceTypeFk descending
                                       select g).ToArray().Select(g => new GOAL_UI(rights.GroupKeyCheck) { SetFromGoal = g });

                        var teams = (from t in context.Teams
                                     where t.GroupFk == rights.GroupKey
                                     select t).ToArray().Select( t => new TEAM_UI(rights.GroupKeyCheck) { SetFromTeam = t });
                        var schedules = (from s in context.Schedules
                                       where s.GroupFk == rights.GroupKey
                                       select s).ToArray().Select(s=>new SCHEDULE_UI(rights.GroupKeyCheck) { SetFromSchedule = s });
                        var ratings = (from r in context.RatingType
                                         select r).ToArray().Select(r => new RATING_TYPE_UI(r) );
                        var importances = (from i in context.ImportanceType
                                           select i).ToArray().Select(i => new IMPORTANCE_TYPE_UI(i));

                        return new { success = true, GoalSet = new GOALSET_UI(rights.GroupKeyCheck, goalSet), Goals = goals, Member = member, Teams = teams, Schedules = schedules, Ratings = ratings, Importance = importances };
                    }
                    throw new Exception("Could not find goal");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "goal");
                return new { success = false, message = message };
            }

        }

        [Route("api/company/goal/")]
        [HttpPost]
        public ActionResult SaveGoal(SaveData<GOALSET_UI, GOAL_UI> saveGoalSet)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveGoalSet.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." } );
                    if (saveGoalSet.Key != saveGoalSet.Data.GOALSET_PK)
                        return Ok(new { success = false, message = "Invalid goal, keys do not match - thx" });

                    Models.Goalset goalSet;
                    if (saveGoalSet.Key == Guid.Empty)
                    {
                        goalSet = saveGoalSet.Data.GetGoalSet(rights.GroupKey);
                        foreach (GOAL_UI goal in saveGoalSet.Children)
                        {
                            goalSet.Goals.Add(goal.GetGoal(rights.GroupKey, goalSet.GoalsetPk));
                        }
                        context.Goalset.Add(goalSet);
                    }
                    else
                    {
                        goalSet = (from s in context.Goalset
                                             where s.GoalsetPk == saveGoalSet.Key && s.GroupFk == rights.GroupKey
                                     select s).FirstOrDefault();
                        if (goalSet == null)
                            return Ok(new { success = false, message = "Cannot find this goal - thx" });
                        saveGoalSet.Data.SetToGoalSet(goalSet, rights.GroupKey);

                        foreach(Goals goal in goalSet.Goals) //.Count, saveGoalSet.Children.Length); i++
                        {
                            var updatedPeriod = saveGoalSet.Children.SingleOrDefault(g => g.GOAL_PK == goal.GoalsPk);
                            if (updatedPeriod != null)
                                updatedPeriod.SetToGoal(goal, rights.GroupKey, goalSet.GoalsetPk);
                            else
                                context.Goals.Remove(goal);
                        }
                        foreach(GOAL_UI goal in saveGoalSet.Children)
                        {
                            bool addGoal = true;
                            if (goal.GOAL_PK != Guid.Empty)
                            {
                                var existingPeriod = goalSet.Goals.SingleOrDefault(g => g.GoalsPk == goal.GOAL_PK);
                                if (existingPeriod != null)
                                    addGoal = false;
                            }
                            if (addGoal)
                                goalSet.Goals.Add(goal.GetGoal(rights.GroupKey, goalSet.GoalsetPk));
                        }
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Goal saved.", key=goalSet.GoalsetPk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save goal");
                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/goal/{goalSetKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteGoal(Guid goalSetKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (goalSetKey != Guid.Empty)
                    {
                        Goalset goalSet = (from s in context.Goalset
                                             where s.GoalsetPk == goalSetKey && s.GroupFk == rights.GroupKey
                                             select s).FirstOrDefault();
                        if (goalSet != null)
                        {
                            context.Goalset.Remove(goalSet);
                            context.SaveChanges();
                            return new { success = true, GoalSet = QueryGoals(context, rights, "_____", "_____") };
                        }
                    }
                }
                throw new Exception("Cannot find goal");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove goal");
                return new { success = false, message = message };
            }
        }
    }
}