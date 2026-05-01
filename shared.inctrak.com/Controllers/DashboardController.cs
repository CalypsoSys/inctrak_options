using IncTrak.data;
using IncTrak.Data;
using IncTrak.Domain;
using IncTrak.FeedbackModels;
using IncTrak.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncTrak.Controllers
{
    [ApiController]
    public class DashboardController : IncTrakController
    {
        private static readonly PERIOD_TYPES_UI[] QuickPeriodTypes = new[]
        {
            new PERIOD_TYPES_UI(new Models.PeriodTypes { PeriodTypePk = 1, Name = "Years" }),
            new PERIOD_TYPES_UI(new Models.PeriodTypes { PeriodTypePk = 2, Name = "Months" }),
            new PERIOD_TYPES_UI(new Models.PeriodTypes { PeriodTypePk = 3, Name = "Weeks" }),
            new PERIOD_TYPES_UI(new Models.PeriodTypes { PeriodTypePk = 4, Name = "Days" })
        };

        private static readonly AMOUNT_TYPES_UI[] QuickAmountTypes = new[]
        {
            new AMOUNT_TYPES_UI(new Models.AmountTypes { AmountTypePk = 1, Name = "Shares" }),
            new AMOUNT_TYPES_UI(new Models.AmountTypes { AmountTypePk = 2, Name = "Percentage" })
        };

        private readonly IVestingPromptInterpreter _vestingPromptInterpreter;

        public DashboardController(IOptions<AppSettings> config, IVestingPromptInterpreter vestingPromptInterpreter) : base(config)
        {
            _vestingPromptInterpreter = vestingPromptInterpreter;
        }

        [Route("api/company/summary/")]
        public object GetCompanySummary()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var plans = (from p in context.Plans
                                 where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                 select new
                                 {
                                     NAME = p.Name,
                                     TOTAL_SHARES = p.TotalShares,
                                     GRANTED_SHARES = (Decimal?)p.Grants.Sum(g => g.Shares),
                                     GRANTS = (Decimal?)p.Grants.Count(),
                                     PARTICIPANTS = p.Grants.Select(g => g.ParticipantFk).Distinct().Count()
                                 }).ToArray();

                    var participants = (from p in context.Participants
                                        where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                        select p).Count();
                    var grants = (from g in context.Grants
                                  where (g.GroupFk == rights.GroupKey || g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                  select g).Count();
                    var schedules = (from s in context.Schedules
                                     where (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).Count();

                    List<object> counts = new List<object>();
                    decimal totalShares = 0;
                    decimal totalGranted = 0;
                    decimal remaining;
                    int order = 3;
                    foreach (var plan in plans)
                    {
                        remaining = plan.TOTAL_SHARES;
                        totalShares += plan.TOTAL_SHARES;
                        counts.Add(new { name = string.Format("{0} Shares", plan.NAME), count = plan.TOTAL_SHARES, order = order++ });
                        if (plan.GRANTED_SHARES.HasValue)
                        {
                            totalGranted += plan.GRANTED_SHARES.Value;
                            remaining -= plan.GRANTED_SHARES.Value;
                            counts.Add(new { name = string.Format("{0} Granted", plan.NAME), count = plan.GRANTED_SHARES, order = order++ });
                        }
                        counts.Add(new { name = string.Format("{0} Remaining", plan.NAME), count = remaining, order = order++ });
                    }
                    remaining = totalShares;
                    counts.Add(new { name = "Total Shares", count = totalShares, order = 0 });
                    if (totalGranted > 0)
                    {
                        remaining -= totalGranted;
                        counts.Add(new { name = "Total Granted", count = totalGranted, order = 1 });
                    }
                    counts.Add(new { name = "Total Remaining", count = remaining, order = 2 });

                    return new { success = true, Plans = plans, Participants = participants, Grants = grants, Schedules = schedules, Counts = counts };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "company summary");

                return new { success = false, message = message };
            }
        }

        [Route("api/optionee/summary/")]
        public object GetOptioneeSummary()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var grants = from g in context.Grants
                                 where g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && g.ParticipantFkNavigation.UserFk == rights.UserKey
                                 group g by g.PlanFk into gp
                                 select gp;
                    decimal totalGranted = 0;
                    decimal totalVested = 0;
                    decimal totalUnVested = 0;
                    DateTime maxDate = DateTime.MinValue;
                    DateTime minDate = DateTime.MaxValue;
                    Dictionary<string, Dictionary<string, decimal>> overTime = new Dictionary<string, Dictionary<string, decimal>>();
                    foreach (var grant in grants)
                    {
                        decimal grantedPlan = grant.Sum(g => g.Shares);
                        totalGranted += grantedPlan;

                        string planName = grant.First().PlanFkNavigation.Name;

                        Dictionary<string, decimal> vestedDict = new Dictionary<string, decimal>();
                        string vestName = string.Format("{0} - Vested", planName);
                        if (overTime.ContainsKey(vestName) == false)
                            overTime.Add(vestName, new Dictionary<string, decimal>());
                        vestedDict = overTime[vestName];

                        Dictionary<string, decimal> unVestedDict = new Dictionary<string, decimal>();
                        string unVestName = string.Format("{0} - Unvested", planName);
                        if (overTime.ContainsKey(unVestName) == false)
                            overTime.Add(unVestName, new Dictionary<string, decimal>());
                        unVestedDict = overTime[unVestName];
                        decimal vested = 0;
                        decimal unVested = grantedPlan;

                        List<VestingPeriod> merge = new List<VestingPeriod>();
                        foreach (var g in grant)
                        {
                            merge.AddRange(ScheduleCalc.GetVestedShares(g));
                        }
                        foreach (var period in merge.OrderBy(p => p.VestDate))
                        {
                            if (period.VestDate > maxDate)
                                maxDate = period.VestDate;
                            if (period.VestDate < minDate)
                                minDate = period.VestDate;
                            if (period.IsVested)
                                totalVested += period.Shares;
                            else
                                totalUnVested += period.Shares;

                            string vestDate = period.VestDate.ToString("yyyy-MM-dd");
                            vested += period.Shares;
                            unVested -= period.Shares;

                            if (vestedDict.ContainsKey(vestDate) == false)
                                vestedDict.Add(vestDate, vested);
                            else if (period.IsVested)
                                vestedDict[vestDate] = vestedDict[vestDate] + period.Shares;

                            if (unVestedDict.ContainsKey(vestDate) == false)
                                unVestedDict.Add(vestDate, unVested);
                            else if (period.IsVested == false)
                                unVestedDict[vestDate] = unVestedDict[vestDate] + period.Shares;
                        }
                    }

                    return new { success = true, OverTime = overTime, TotalGranted = totalGranted, TotalVested = totalVested, TotalUnVested = totalUnVested, MaxDate = maxDate.ToString("yyyy-MM-dd"), MinDate = minDate.ToString("yyyy-MM-dd") };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "optionee summary");
                return new { success = false, message = message };
            }

        }

        [Route("api/optionee/quick/")]
        public object GetQuickData()
        {
            try
            {
                return new
                {
                    success = true,
                    Grant = new GRANT_UI(Guid.Empty),
                    Periods = new PERIOD_UI[1] { new PERIOD_UI(Guid.Empty) },
                    PeriodTypes = QuickPeriodTypes,
                    AmountTypes = QuickAmountTypes
                };
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "quick data");
                return new { success = false, message = message };
            }
        }

        [Route("api/optionee/quick/interpret/")]
        [HttpPost]
        public ActionResult InterpretQuickVesting([FromBody] QuickVestingInterpretRequest request)
        {
            try
            {
                QuickVestingInterpretResult result = _vestingPromptInterpreter.Interpret(request);
                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    summary = result.Summary,
                    provider = result.Provider,
                    alternateProvider = result.AlternateProvider,
                    confidence = result.Confidence,
                    requiresAi = result.RequiresAi,
                    kind = result.Kind,
                    usedAi = result.UsedAi,
                    jsonRepairAttempted = result.JsonRepairAttempted,
                    warnings = result.Warnings,
                    missingFields = result.MissingFields,
                    assumptions = result.Assumptions,
                    sharesGranted = result.SharesGranted,
                    vestingStart = result.VestingStart,
                    Periods = result.Periods,
                    PeriodTypes = QuickPeriodTypes,
                    AmountTypes = QuickAmountTypes
                });
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "quick vesting interpret");
                return Ok(new { success = false, message = message, provider = "error", Periods = Array.Empty<PERIOD_UI>() });
            }
        }

        private void LogQuick(string message)
        {
            try
            {
                using (inctrak_feedbackContext context = new inctrak_feedbackContext(_options.Value))
                {
                    Feedback feedBack = new Feedback();
                    feedBack.Name = "Quick";
                    feedBack.MessageTypeFk = 9;
                    feedBack.EmailAddress = "quick@inctrak.com";
                    feedBack.Message = message;
                    feedBack.Subject = "Quick Vesting";
                    feedBack.Created = DateTime.Now;
                    feedBack.ClientData = GetClientInfo();
                    context.Feedback.Add(feedBack);
                    context.SaveChanges();
                }
            }
            catch (Exception excp)
            {
                IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "quick vesting logger");
            }
        }

        [Route("api/optionee/quick/")]
        [HttpPost]
        public ActionResult QuickVesting(SaveData<GRANT_UI, PERIOD_UI> saveSchedule)
        {
            try
            {
                if (saveSchedule?.Data == null || saveSchedule.Data.SHARES <= 0)
                {
                    return Ok(new { success = false, message = "Enter a Shares Granted value greater than zero before calculating vesting." });
                }

                if (saveSchedule.Children == null || saveSchedule.Children.Any() == false)
                {
                    return Ok(new { success = false, message = "Add at least one vesting period before calculating vesting." });
                }

                var periodTypes = QuickPeriodTypes;
                var amountTypes = QuickAmountTypes;
                Grants grant = saveSchedule.Data.GetGrant(Guid.Empty);
                grant.VestingScheduleFkNavigation = new Schedules();
                StringBuilder message = new StringBuilder();
                message.AppendFormat("D:{0} S:{1}\r\n", saveSchedule.Data.VESTING_START, saveSchedule.Data.SHARES);
                foreach (var prd in saveSchedule.Children)
                {
                    Periods period = prd.GetPeriod(Guid.Empty, Guid.Empty);
                    period.PeriodTypeFkNavigation = periodTypes.Where(p => p.PERIOD_TYPE_PK == period.PeriodTypeFk).First().GetPeriodType();
                    period.AmountTypeFkNavigation = amountTypes.Where(a => a.AMOUNT_TYPE_PK == period.AmountTypeFk).First().GetAmountType();
                    grant.VestingScheduleFkNavigation.Periods.Add(period);

                    message.AppendFormat("O:{0} A:{1} E:{2} I:{3} PA:{4} AT:{5} PT:{6}\r\n", period.Order, period.Amount, period.EvenOverN, period.Increments, period.PeriodAmount, period.AmountTypeFkNavigation, period.PeriodTypeFkNavigation);
                }

                var vestingSchedule = ScheduleCalc.GetVestedShares(grant);

                LogQuick(message.ToString());
                return Ok(new { success = true, message = "Quick saved.", Grant = saveSchedule.Data, Periods = saveSchedule.Children, PeriodTypes = periodTypes, AmountTypes = amountTypes, VestSchedule = vestingSchedule });
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "quick vesting");
                return Ok(new { success = false, message = message });
            }
        }
    }
}
