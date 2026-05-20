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
    public class TerminationController : IncTrakController
    {
        public TerminationController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object QueryTerminations(inctrakContext context, LoginRights rights)
        {
            IQueryable<Terminations> query = from s in context.Terminations
                                         where (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                         select s;

            var existingTermination = query.ToArray().Select(t => new TERMINATION_UI(rights.GroupKeyCheck) { SetFromTermination = t });
            if (rights.IsAdmin)
            {
                var newTermination = new TERMINATION_UI[1] { new TERMINATION_UI(rights.GroupKeyCheck, new Terminations { TerminationPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Termination Date>" }) };
                return newTermination.Union(existingTermination).ToArray();
            }

            return existingTermination.ToArray();
        }

        [Route("api/company/terminations/")]
        public object GetTerminations()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryTerminations(context, rights);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get terminations");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/termination/{terminationKey}")]
        public object GetTermination(Guid terminationKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Terminations termination;
                    if (terminationKey == Guid.Empty)
                        termination = new Terminations() { GroupFk = rights.GroupKeyCheck, SpecificDate=DateTime.Now, AbsoluteDate=DateTime.Now};
                    else
                        termination = (from s in context.Terminations
                                    where s.TerminationPk == terminationKey && (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).FirstOrDefault();
                    if (termination != null)
                    {
                        var termFromTypes = (from tf in context.TermFroms
                                             where tf.TermFromPk != (int)TERM_FROMS_UI.IDs.ABSOLUTE
                                       select new TERM_FROMS_UI() { TermFromType = tf }).ToArray();

                        return new { success = true, Termination = new TERMINATION_UI(rights.GroupKeyCheck, termination), TermFromType = termFromTypes };
                    }
                }
                throw new Exception("Cannot find termination");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a termination");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/termination/")]
        [HttpPost]
        public ActionResult SaveTermination(SaveData<TERMINATION_UI> saveTermination)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveTermination.Key != saveTermination.Data.TERMINATION_PK)
                        return Ok(new { success = false, message = "Invalid termination date, keys do not match - thx" });

                    Terminations termination;
                    if (saveTermination.Key == Guid.Empty)
                    {
                        termination = saveTermination.Data.GetTermination(rights.GroupKey);
                        context.Terminations.Add(termination);
                    }
                    else
                    {
                        termination = (from s in context.Terminations
                                             where s.TerminationPk == saveTermination.Key && s.GroupFk == rights.GroupKey
                                     select s).FirstOrDefault();
                        if (termination == null)
                            return Ok(new { success = false, message = "Cannot find this termination date - thx" });
                        saveTermination.Data.SetToTermination(termination, rights.GroupKey);
                    }

                    if (termination.IsAbsolute) {
                        termination.TermFromFk = (from tf in context.TermFroms
                                                        where tf.TermFromPk == (int)TERM_FROMS_UI.IDs.ABSOLUTE
                                                        select tf.TermFromPk).FirstOrDefault();
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Termination saved.", key = termination.TerminationPk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save termination");

                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/termination/{terminationKey}")]
        [HttpDelete]
        public object DeleteTermination(Guid terminationKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (terminationKey != Guid.Empty)
                    {
                        Terminations termination = (from s in context.Terminations
                                             where s.TerminationPk == terminationKey && s.GroupFk == rights.GroupKey
                                             select s).FirstOrDefault();
                        if (termination != null)
                        {
                            context.Terminations.Remove(termination);
                            context.SaveChanges();
                            return new { success = true, Terminations = QueryTerminations(context, rights) };
                        }
                    }
                }
                throw new Exception("Cannot find termination");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove termination");

                return new { success = false, message = message };
            }
        }
    }
}
