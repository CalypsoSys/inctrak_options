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
    public class PlanController : IncTrakController
    {
        public PlanController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryPlans(inctrakContext context, LoginRights rights)
        {
            IQueryable<Plans> query = from p in context.Plans
                                     where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select p;

            var existingPlans = query.ToArray().Select(p => new PLAN_UI(rights.GroupKeyCheck) { SetFromPlan = p, STOCK_CLASS_NAME = p.StockClassFkNavigation.Name, GRANTED_SHARES = (Decimal?)p.Grants.Sum(g => g.Shares) ?? 0 });
            if (rights.IsAdmin)
            {
                var newPlan = new PLAN_UI[1] { new PLAN_UI(rights.GroupKeyCheck, new Plans() { PlanPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Plan>" }) };

                return newPlan.Union(existingPlans).ToArray();
            }

            return existingPlans.ToArray();
        }

        [Route("api/company/plans/")]
        public object GetPlans()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryPlans(context, rights);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get plans");

                return new { success = false, message = message };
            }
        }

        [Route("api/company/plan/{planKey}/{uuidKey}")]
        public object GetPlan(Guid planKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Plans plan;
                    if (planKey == Guid.Empty)
                        plan = new Plans() { GroupFk = rights.GroupKeyCheck };
                    else
                        plan = (from p in context.Plans
                                    where p.PlanPk == planKey && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select p).FirstOrDefault();
                    if (plan != null)
                    {
                        var stockClasses = (from sc in context.StockClasses
                                            where sc.GroupFk == rights.GroupKey
                                            select sc).ToArray().Select(sc => new STOCK_CLASSES_UI(rights.GroupKeyCheck) { SetFromStockClass = sc });

                        return new { success = true, Plan = new PLAN_UI(rights.GroupKeyCheck, plan), StockClasses = stockClasses };
                    }
                }
                throw new Exception("Cannot find plan");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a plan");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/plan/")]
        [HttpPost]
        public ActionResult SavePlan(SaveData<PLAN_UI> savePlan)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, savePlan.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (savePlan.Key != savePlan.Data.PLAN_PK)
                        return Ok(new { success = false, message = "Invalid Plan, keys do not match - thx" });

                    Plans plan;
                    if (savePlan.Key == Guid.Empty)
                    {
                        plan = savePlan.Data.GetPlan(rights.GroupKey);
                        context.Plans.Add(plan);
                    }
                    else
                    {
                        plan = (from p in context.Plans
                                where p.PlanPk == savePlan.Key && p.GroupFk == rights.GroupKey
                                select p).FirstOrDefault();
                        if (plan == null)
                            return Ok(new { success = false, message = "Cannot find this plan - thx" });
                        savePlan.Data.SetToPlan(plan, rights.GroupKey);
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Plan saved.", key = plan.PlanPk  });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save a plan");
                return Ok(new { success = false, message = message });
            }
        }

        [Route("api/company/plan/{planKey}/{uuidKey}")]
        [HttpDelete]
        public object DeletePlan(Guid planKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (planKey != Guid.Empty)
                    {
                        Plans plan = (from p in context.Plans
                                     where p.PlanPk == planKey && p.GroupFk == rights.GroupKey
                                     select p).FirstOrDefault();
                        if (plan != null)
                        {
                            context.Plans.Remove(plan);
                            context.SaveChanges();
                            return new { success = true, Plans = QueryPlans(context, rights) };
                        }
                    }
                    throw new Exception("Plan not found");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove a plan");
                return new { success = false, message = message };
            }
        }
    }
}