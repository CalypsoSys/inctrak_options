using IncTrak.GoalSetter.data;
using IncTrak.GoalSetter.Data;
using IncTrak.GoalSetter.Domain;
using IncTrak.GoalSetter.FeedbackModels;
using IncTrak.GoalSetter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Controllers
{
    [ApiController]
    public class DashboardController : IncTrakController
    {
        public DashboardController(IOptions<AppSettings> config) : base(config)
        {
        }

        [Route("api/company/summary/")]
        public object GetCompanySummary()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var departments = (from p in context.Departments
                                 where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                 select p).Count();
                    var teams = (from p in context.Teams
                                 where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                 select p).Count();
                    var members = (from p in context.Members
                                        where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                        select p).Count();
                    var schedules = (from s in context.Schedules
                                     where (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).Count();
                    var totalGoals = (from g in context.Goals
                                 where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                 select g).Count();
                    var currentGoals = (from g in context.Goals
                                 where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) &&
                                 g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate > DateTime.Now
                                 select g).Count();

                    var breakdown = (from p in context.Teams
                                              where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                              select new
                                              {
                                                  DEPARTMENT_NAME = p.DepartmentFkNavigation.Name,
                                                  TEAM_NAME = p.Name,
                                                  CURRENT_GOALS = p.Goalset.Where(g => g.ScheduleFkNavigation.EndDate > DateTime.Now).Sum(g => g.Goals.Count()),
                                                  TOTAL_GOALS = p.Goalset.Sum(g => g.Goals.Count()),
                                                  TOTAL_MEMBERS = p.Goalset.GroupBy(g=>g.MemberFk).Select(m => m.First()).Count()
                                              }).ToArray();


                    List<object> counts = new List<object>();
                    var perDep = from d in context.Departments
                                 where (d.GroupFk == rights.GroupKey || d.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                 select new { name = string.Format("{0} - Dept.", d.Name), count = d.Teams.Sum(t => t.Goalset.Sum(g => g.Goals.Count())) };
                    counts.AddRange(perDep.ToArray());
                    var perTeam = from t in context.Teams
                                 where (t.GroupFk == rights.GroupKey || t.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                 select new { name = string.Format("{0} - Team", t.Name), count = t.Goalset.Sum(g => g.Goals.Count()) };
                    counts.AddRange(perTeam.ToArray());
                    var perGoalRating = from g in context.Goals
                                  where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                  group g by g.RatingTypeFkNavigation into r
                                  select new { name = (r.Key != null ? r.Key.Name : "Rating Not Set"), count = r.Count() };
                    counts.AddRange(perGoalRating.ToArray());
                    var perGoalImportance = from g in context.Goals
                                        where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                        group g by g.ImportanceTypeFkNavigation into i
                                        select new { name = (i.Key != null ? i.Key.Name : "Importance Not Set"), count = i.Count() };
                    counts.AddRange(perGoalImportance.ToArray());

                    return new { success = true, Departments = departments, Teams = teams,
                                Members = members, Schedules = schedules,
                                TotalGoals = totalGoals, CurrentGoals = currentGoals,
                                Breakdown = breakdown, Counts = counts };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "company summary");

                return new { success = false, message = message };
            }
        }

        [Route("api/member/summary/")]
        public object GetMemberSummary()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var goalQuery = from g in context.Goals
                                 where g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && g.GoalsetFkNavigation.MemberFkNavigation.UserFk == rights.UserKey
                                 orderby g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate, g.ImportanceTypeFk descending
                                select new {
                                    DUE_DATE = g.GoalsetFkNavigation.ScheduleFkNavigation.EndDate,
                                    IMPORTANCE = g.ImportanceTypeFkNavigation.Name,
                                    RATING = g.RatingTypeFkNavigation.Name,
                                    GOAL = g.Description,
                                    SCHEDULE = g.GoalsetFkNavigation.ScheduleFkNavigation.Name,
                                    TEAM = g.GoalsetFkNavigation.TeamFkNavigation.Name,
                                    DEPARTMENT = g.GoalsetFkNavigation.TeamFkNavigation.DepartmentFkNavigation.Name
                                };

                    Dictionary<string, Dictionary<string, int>> overTime = new Dictionary<string, Dictionary<string, int>>();
                    Dictionary<string, int> totalRatings = new Dictionary<string, int>();
                    var ratings = (from r in context.RatingType
                                   select r.Name).ToList();
                    ratings.Add("Not Set");
                    foreach (var rating in ratings)
                    {
                        overTime.Add(rating, new Dictionary<string, int>());
                        totalRatings.Add(rating, 0);
                    }

                    List<object> goals = new List<object>();
                    foreach (var goal in goalQuery)
                    {
                        var ratingName = goal.RATING;
                        if (string.IsNullOrWhiteSpace(ratingName))
                            ratingName = "Not Set";

                        if (totalRatings.ContainsKey(ratingName))
                            totalRatings[ratingName] = totalRatings[ratingName] + 1;
                        else
                            totalRatings.Add(ratingName, 1);

                        if (goal.DUE_DATE > DateTime.Now)
                        {
                            goals.Add(goal);
                        }
                        else
                        {
                            Dictionary<string, int> ratingDict = new Dictionary<string, int>();
                            if (overTime.ContainsKey(ratingName) == false)
                                overTime.Add(ratingName, new Dictionary<string, int>());
                            string dueDate = goal.DUE_DATE.ToString("yyyy-MM-dd");
                            foreach (var rating in overTime.Keys)
                            {
                                ratingDict = overTime[rating];
                                int mod = 0;
                                if (rating == ratingName)
                                    mod = 1;

                                if (ratingDict.ContainsKey(dueDate))
                                    ratingDict[dueDate] = ratingDict[dueDate] + mod;
                                else
                                    ratingDict.Add(dueDate, mod);
                            }
                        }
                    }

                    return new { success = true, Goals = goals, OverTime = overTime };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "member summary");
                return new { success = false, message = message };
            }

        }
    }
}
