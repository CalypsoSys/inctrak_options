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
using IncTrak.Domain;
using Microsoft.Extensions.Options;

namespace IncTrak.Controllers
{
    [ApiController]
    public class OptioneeController : IncTrakController
    {
        public OptioneeController(IOptions<AppSettings> config) : base(config)
        {
        }

        [Route("api/participant/grants/")]
        public object GetGrants()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var grants = from g in context.Grants
                                    where g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && g.ParticipantFkNavigation.UserFk == rights.UserKey
                                    select new { GRANT_PK = g.GrantPk, PLAN_NAME = g.PlanFkNavigation.Name, VEST_NAME = g.VestingScheduleFkNavigation.Name, DATE_OF_GRANT = g.DateOfGrant, VESTING_START = g.VestingStart, OPTION_PRICE = g.OptionPrice, SHARES = g.Shares };
                    return grants.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "participant grants");
                return new { success = false, message = message };
            }
        }

        [Route("api/participant/grant/{grantKey}/{uuidKey}")]
        public object GetGrant(Guid grantKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var grant = (from g in context.Grants
                                 where g.GrantPk == grantKey && g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && g.ParticipantFkNavigation.UserFk == rights.UserKey
                                 select g).FirstOrDefault();
                    if (grant != null)
                    {
                        var vestingSchedule = ScheduleCalc.GetVestedShares(grant);
                        var vestingEnd = vestingSchedule.Last().VestDate;
                        return new { success = true, Grant = new { PLAN_NAME = grant.PlanFkNavigation.Name, VEST_NAME = grant.VestingScheduleFkNavigation.Name, DATE_OF_GRANT = grant.DateOfGrant, TerminationDate = ScheduleCalc.GetTerminationDate(grant), VESTING_START = grant.VestingStart, VestingEnd = vestingEnd, OPTION_PRICE = grant.OptionPrice, SHARES = grant.Shares }, VestSchedule = vestingSchedule };
                    }
                    throw new Exception("Could not find participant grant");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "participant get grant");
                return new { success = false, message = message };
            }
        }

        [Route("api/participant/summary/")]
        public object GetSummary()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var grants = from g in context.Grants
                                    where g.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && g.ParticipantFkNavigation.UserFk == rights.UserKey
                                    select g;
                    List<object> summary = new List<object>();
                    decimal totalGranted = 0;
                    decimal totalVested = 0;
                    foreach(var grant in grants)
                    {
                        var vest = ScheduleCalc.GetVestedShares(grant).Where(v=>v.IsVested).OrderBy(v=>v.Order).LastOrDefault();
                        if (vest == null)
                            vest = new VestingPeriod();
                        summary.Add(new { PLAN = grant.PlanFkNavigation.Name, GRANTED = grant.Shares, VESTED = vest.TotalShares, VEST_PCT = vest.TotalPercent, UNVESTED = (grant.Shares - vest.TotalShares), UNVEST_PCT = (100 - vest.TotalPercent) });
                        totalGranted += grant.Shares;
                        totalVested += vest.TotalShares;
                    }
                    decimal vestPct = totalVested / totalGranted * 100;
                    summary.Add(new { PLAN = "Total", GRANTED = totalGranted, VESTED = totalVested, VEST_PCT = vestPct, UNVESTED = (totalGranted - totalVested), UNVEST_PCT = (100 - vestPct) });
                    return summary.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "participant summary");
                return new { success = false, message = message };
            }
        }

        [Route("api/participant/stocks/")]
        public object GetStocks()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    var stocks = from sh in context.StockHolders
                                 where sh.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey) && sh.ParticipantFkNavigation.UserFk == rights.UserKey
                                 select sh;
                    List<object> summary = new List<object>();
                    foreach (var stock in stocks)
                    {
                        summary.Add(new { STOCK_CLASS_NAME = stock.StockClassFkNavigation.Name, DATE_OF_SALE = stock.DateOfSale, PRICE = stock.Price, SHARES = stock.Shares });
                    }
                    return summary.ToArray();
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "participant stock");
                return new { success = false, message = message };
            }
        }
    }
}