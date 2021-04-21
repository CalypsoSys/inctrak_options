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
    public class ScheduleController : IncTrakController
    {
        public ScheduleController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object QuerySchedules(inctrakContext context, LoginRights rights)
        {
            IQueryable<Schedules> query = from s in context.Schedules
                                         where (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                         select s;

            var existingSchedule = query.ToArray().Select(s => new SCHEDULE_UI(rights.GroupKeyCheck) { SetFromSchedule = s, PERIODS = (Decimal?)s.Periods.Count ?? 0 });
            if (rights.IsAdmin)
            {
                var newSchedule = new SCHEDULE_UI[1] { new SCHEDULE_UI(rights.GroupKeyCheck, new Schedules() { SchedulePk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Schedule>" }) };
                return newSchedule.Union(existingSchedule).ToArray();
            }

            return existingSchedule.ToArray();
        }

        [Route("api/company/schedules/")]
        public object GetSchedules()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QuerySchedules(context, rights);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get schedules");

                return new { success = false, message = message };
            }
        }

        [Route("api/company/schedule/{scheduleKey}/{uuidKey}")]
        public object GetSchedule(Guid scheduleKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Schedules schedule;
                    if (scheduleKey == Guid.Empty)
                        schedule = new Schedules() { GroupFk = rights.GroupKeyCheck };
                    else
                        schedule = (from s in context.Schedules
                                    where s.SchedulePk == scheduleKey && (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                    select s).FirstOrDefault();
                    if (schedule != null)
                    {
                        var periods = (from p in context.Periods
                                    where p.ScheduleFk == scheduleKey && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                    orderby p.Order
                                    select p).ToArray().Select( p => new PERIOD_UI(rights.GroupKeyCheck) { SetFromPeriod = p });

                        var periodTypes = (from pt in context.PeriodTypes
                                       select new PERIOD_TYPES_UI() { PeriodType = pt }).ToArray();
                        var amountTypes = (from at in context.AmountTypes
                                           select new AMOUNT_TYPES_UI() { AmountType = at }).ToArray();

                        return new { success = true, Schedule = new SCHEDULE_UI(rights.GroupKeyCheck, schedule), Periods = periods, PeriodTypes = periodTypes, AmountTypes = amountTypes };
                    }
                }
                throw new Exception("Cannot find schedule");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a schedule");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/schedule/")]
        [HttpPost]
        public ActionResult SaveSchedule(SaveData<SCHEDULE_UI, PERIOD_UI> saveSchedule)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveSchedule.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });

                    if (saveSchedule.Key != saveSchedule.Data.SCHEDULE_PK)
                        return Ok(new { success = false, message = "Invalid schedule, keys do not match - thx" });
                    Schedules schedule;
                    if (saveSchedule.Key == Guid.Empty)
                    {
                        schedule = saveSchedule.Data.GetSchedule(rights.GroupKey);
                        foreach(PERIOD_UI period in saveSchedule.Children)
                        {
                            schedule.Periods.Add(period.GetPeriod(rights.GroupKey, schedule.SchedulePk));
                        }
                        context.Schedules.Add(schedule);
                    }
                    else
                    {
                        schedule = (from s in context.Schedules
                                             where s.SchedulePk == saveSchedule.Key && s.GroupFk == rights.GroupKey
                                     select s).FirstOrDefault();
                        if (schedule == null)
                            return Ok(new { success = false, message = "Cannot find this schedule - thx" });
                        saveSchedule.Data.SetToSchedule(schedule, rights.GroupKey);

                        for(int i=0;i<Math.Max(schedule.Periods.Count, saveSchedule.Children.Length);i++)
                        {
                            var existingPeriod = schedule.Periods.SingleOrDefault(p => p.Order == i);
                            var newPeriod = saveSchedule.Children.SingleOrDefault(p => p.ORDER == i);
                            if (existingPeriod != null && newPeriod != null)
                                newPeriod.SetToPeriod(existingPeriod, rights.GroupKey, schedule.SchedulePk);
                            else if (newPeriod == null)
                                context.Periods.Remove(existingPeriod);
                            else if (existingPeriod == null)
                                schedule.Periods.Add(newPeriod.GetPeriod(rights.GroupKey, schedule.SchedulePk));
                        }
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Schedule saved.", key = schedule.SchedulePk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save a schedule");

                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/schedule/{scheduleKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteSchedule(Guid scheduleKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (scheduleKey != Guid.Empty)
                    {
                        Schedules schedule = (from s in context.Schedules
                                             where s.SchedulePk == scheduleKey && s.GroupFk == rights.GroupKey
                                             select s).FirstOrDefault();
                        if (schedule != null)
                        {
                            context.Schedules.Remove(schedule);
                            context.SaveChanges();
                            return new { success = true, Schedules = QuerySchedules(context, rights) };
                        }
                    }
                }
                throw new Exception("Cannot find schedule");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove a schedule");
                return new { success = false, message = message };
            }
        }
    }
}