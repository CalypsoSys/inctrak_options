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
using IncTrak.GoalSetter.Domain;
using Microsoft.Extensions.Options;
using System.Text;

namespace IncTrak.GoalSetter.Controllers
{
    [ApiController]
    public class MemberViewController : IncTrakController
    {
        private static string _importanceKey = "importance_type_fk";
        private static string _ratingKey = "rating_type_fk";
        private static Dictionary<string, string> _goalColumnMap = new Dictionary<string, string> {
            { _importanceKey, "Importance" },
            { "description", "Description"  },
            { _ratingKey, "Rating" },
            { "manager_comments", "Manager Comments" },
            { "member_comments", "Member Comments" }
        };

        public MemberViewController(IOptions<AppSettings> config) : base(config)
        {
        }

        [Route("api/member/company_goals/")]
        public object GetCompanyGoals()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var schedules = from gs in context.Goalset
                                    where gs.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && gs.MemberFkNavigation.UserFk == rights.UserKey &&
                                        gs.ScheduleFkNavigation.EndDate > DateTime.Now
                                    select gs.ScheduleFk;
                    var goals = from g in context.Goals
                                where schedules.Contains(g.GoalsetFkNavigation.ScheduleFk)
                                select new
                                {
                                    DEPARTMENT = g.GoalsetFkNavigation.TeamFkNavigation.DepartmentFkNavigation.Name,
                                    TEAM = g.GoalsetFkNavigation.TeamFkNavigation.Name,
                                    SCHEDULE = g.GoalsetFkNavigation.ScheduleFkNavigation.Name,
                                    GOAL = g.Description,
                                    DUE_DATE = g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate
                                };
                    return goals.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member company");
                return new { success = false, message = message };
            }
        }

        [Route("api/member/department_goals/")]
        public object GetDepartmentGoals()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var departments = from gs in context.Goalset
                                  where gs.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && gs.MemberFkNavigation.UserFk == rights.UserKey &&
                                      gs.ScheduleFkNavigation.EndDate > DateTime.Now
                                  select gs.TeamFkNavigation.DepartmentFk;
                    var goals = from g in context.Goals
                                where departments.Contains(g.GoalsetFkNavigation.TeamFkNavigation.DepartmentFk) &&
                                g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate > DateTime.Now
                                select new
                                {
                                    DEPARTMENT = g.GoalsetFkNavigation.TeamFkNavigation.DepartmentFkNavigation.Name,
                                    TEAM = g.GoalsetFkNavigation.TeamFkNavigation.Name,
                                    SCHEDULE = g.GoalsetFkNavigation.ScheduleFkNavigation.Name,
                                    GOAL = g.Description,
                                    DUE_DATE = g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate
                                };
                    return goals.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member department");
                return new { success = false, message = message };
            }
        }

        [Route("api/member/team_goals/")]
        public object GetTeamGoals()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var teams = from gs in context.Goalset
                                    where gs.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && gs.MemberFkNavigation.UserFk == rights.UserKey &&
                                        gs.ScheduleFkNavigation.EndDate > DateTime.Now
                                    select gs.TeamFk;
                    var goals = from g in context.Goals
                                where teams.Contains(g.GoalsetFkNavigation.TeamFk) &&
                                g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate > DateTime.Now
                                select new
                                {
                                    DEPARTMENT = g.GoalsetFkNavigation.TeamFkNavigation.DepartmentFkNavigation.Name,
                                    TEAM = g.GoalsetFkNavigation.TeamFkNavigation.Name,
                                    SCHEDULE = g.GoalsetFkNavigation.ScheduleFkNavigation.Name,
                                    GOAL = g.Description,
                                    DUE_DATE = g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate
                                };
                    return goals.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member teams");
                return new { success = false, message = message };
            }
        }

        [Route("api/member/goals/")]
        public object GetGoals()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var goalSet = from gs in context.Goalset
                                    where gs.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && gs.MemberFkNavigation.UserFk == rights.UserKey
                                    orderby gs.ScheduleFkNavigation.EndDate descending
                                    select new {
                                        GOALSET_PK = gs.GoalsetPk,
                                        DEPARTMENT = gs.TeamFkNavigation.DepartmentFkNavigation.Name,
                                        TEAM = gs.TeamFkNavigation.Name,
                                        SCHEDULE = gs.ScheduleFkNavigation.Name,
                                        DUE_DATE = gs.ScheduleFkNavigation.EndDate
                                    };
                    return goalSet.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member goals");
                return new { success = false, message = message };
            }
        }

        [Route("api/member/goal/{goalKey}/{uuidKey}")]
        public object GetGoal(Guid goalKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var goalSet = (from gs in context.Goalset
                                 where gs.GoalsetPk == goalKey && gs.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && gs.MemberFkNavigation.UserFk == rights.UserKey
                                 select gs).FirstOrDefault();
                    if (goalSet != null)
                    {
                        var goals = from g in context.Goals
                                    where g.GoalsetFk == goalSet.GoalsetPk
                                    orderby g.ImportanceTypeFk descending
                                    select new
                                    {
                                        GOAL_PK = g.GoalsPk,
                                        IMPORTANCE = g.ImportanceTypeFkNavigation.Name,
                                        DESCRIPTION = g.Description,
                                        RATING = g.RatingTypeFkNavigation.Name,
                                        MANAGER_COMMENTS = g.ManagerComments,
                                        MEMBER_COMMENTS = g.MemberComments,
                                    };
                        return new { success = true, Goal = new {
                            DEPARTMENT = goalSet.TeamFkNavigation.DepartmentFkNavigation.Name,
                            TEAM = goalSet.TeamFkNavigation.Name,
                            SCHEDULE = goalSet.ScheduleFkNavigation.Name,
                            START_DATE = goalSet.ScheduleFkNavigation.StartDate,
                            END_DATE = goalSet.ScheduleFkNavigation.EndDate,
                            GOALS = goals.ToArray()
                        } };
                    }
                    throw new Exception("Could not find member goal");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member get goal");
                return new { success = false, message = message };
            }
        }

        private string GetChangedGoalValue(string key, Dictionary<string, string> data, Dictionary<string, string> priority, Dictionary<string, string> rating)
        {
            if (data != null && data.ContainsKey(key))
            {
                string value = data[key];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    if (key == _importanceKey && priority.ContainsKey(value))
                    {
                        return priority[value];
                    }
                    else if (key == _ratingKey && rating.ContainsKey(value))
                    {
                        return rating[value];
                    }
                }
                return value;
            }

            return "";
        }

        private string GetChangedGoal(Dictionary<string, string> oldData, Dictionary<string, string> newData, Dictionary<string, string> priority, Dictionary<string, string> rating)
        {
            StringBuilder output = new StringBuilder();
            string sep = string.Empty;
            foreach(var key in _goalColumnMap.Keys)
            {
                if (newData.ContainsKey(key))
                {
                    var value = newData[key];
                    if (string.IsNullOrWhiteSpace(value) == false)
                    {
                        if (key == _importanceKey && priority.ContainsKey(value))
                        {
                            value = priority[value];
                        }
                        else if (key == _ratingKey && rating.ContainsKey(value))
                        {
                            value = rating[value];
                        }
                    }

                    var oldValue = GetChangedGoalValue(key, oldData, priority, rating);
                    var newValue = GetChangedGoalValue(key, newData, priority, rating);
                    if (string.IsNullOrWhiteSpace(oldValue) != true || string.IsNullOrWhiteSpace(newValue) != true)
                    {
                        output.AppendFormat("{0}{1} [{2}] to [{3}]", sep, _goalColumnMap[key], oldValue, newValue);
                        if (sep == string.Empty)
                            sep = ", ";
                    }
                }
            }

            return output.ToString();
        }

        [Route("api/member/goal_history/{goalKey}/{uuidKey}")]
        public object GetGoalHistory(Guid goalKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var goalsInHistory = from la in context.LoggedActions
                                   where la.TableName == "goals" && la.GroupFk == rights.GroupKeyCheck && la.OriginalData["goalset_fk"] == goalKey.ToString()
                                   orderby la.Created
                                      select la;
                    var priority = context.ImportanceType.ToDictionary(i => i.ImportanceTypePk.ToString(), i => i.Name);
                    var rating = context.RatingType.ToDictionary(r => r.RatingTypePk.ToString(), i => i.Name);
                    var goalHistory = new List<object>();
                    foreach(var hist in goalsInHistory)
                    {
                        string action = "Unknown", changed = "";
                        if ( hist.Action == "I")
                        {
                            action = "Added";
                            changed = GetChangedGoal(null, hist.OriginalData, priority, rating);
                        }
                        else if ( hist.Action == "D")
                        {
                            action = "Deleted";
                            changed = GetChangedGoal(null, hist.OriginalData, priority, rating);
                        }
                        else if ( hist.Action == "U")
                        {
                            action = "Changed";
                            changed = GetChangedGoal(hist.OriginalData, hist.NewData, priority, rating);
                        }

                        goalHistory.Add(new
                        {
                            CREATED = hist.Created,
                            USER_NAME = hist.UserName,
                            ACTION = action,
                            CHANGED = changed
                        });
                    }
                    return new
                    {
                        success = true,
                        GoalHistory = goalHistory.ToArray()
                    };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member get goal");
                return new { success = false, message = message };
            }
        }

        [Route("api/member/goal/")]
        [HttpPost]
        public ActionResult SaveGoal(SaveData<string> saveMemberComment)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveMemberComment.UUID);
                    if (rights == null)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveMemberComment.Key == Guid.Empty)
                        return Ok(new { success = false, message = "Invalid goal key - thx" });

                    Models.Goals goal;
                    goal = (from g in context.Goals
                                where g.GoalsPk == saveMemberComment.Key && g.GroupFk == rights.GroupKeyCheck
                                && g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && g.GoalsetFkNavigation.MemberFkNavigation.UserFk == rights.UserKey
                            select g).FirstOrDefault();
                    if (goal == null)
                        return Ok(new { success = false, message = "Cannot find this goal - thx" });
                    goal.MemberComments = saveMemberComment.Data;

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Member Comment saved.", key = goal.GoalsetFk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save goal");
                return Ok(new { success = false, message = message });
            }
        }
    }
}