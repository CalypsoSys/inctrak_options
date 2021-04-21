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
    public class StockHolderController : IncTrakController
    {
        public StockHolderController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryStockHolders(inctrakContext context, LoginRights rights, string searchString, string searchType)
        {
            string part1, part2, part3, part4, part5, part6;
            SearchType st = SearchParts(searchString, searchType, out part1, out part2, out part3, out part4, out part5, out part6);

            IQueryable<StockHolders> query;
            if (st == SearchType.None)
            {
                if (searchString == "_____" && searchType == "_____")
                    query = (from sh in context.StockHolders
                             where (sh.GroupFk == rights.GroupKey || sh.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                             select sh).Take(10);
                else
                    query = from sh in context.StockHolders
                            where 1 == 2 && (sh.GroupFk == rights.GroupKey || sh.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                            select sh;
            }
            else if (st == SearchType.Any)
            {
                query = from sh in context.StockHolders
                        where (sh.GroupFk == rights.GroupKey || sh.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) && 
                        (
                            sh.ParticipantFkNavigation.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) ||
                            (part2 != null && sh.ParticipantFkNavigation.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part3 != null && sh.ParticipantFkNavigation.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part4 != null && sh.ParticipantFkNavigation.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part5 != null && sh.ParticipantFkNavigation.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part6 != null && sh.ParticipantFkNavigation.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select sh;
            }
            else
            {
                query = from sh in context.StockHolders
                        where (sh.GroupFk == rights.GroupKey || sh.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) &&
                        (
                            sh.ParticipantFkNavigation.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) &&
                            (part2 == null || sh.ParticipantFkNavigation.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part3 == null || sh.ParticipantFkNavigation.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part4 == null || sh.ParticipantFkNavigation.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part5 == null || sh.ParticipantFkNavigation.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part6 == null || sh.ParticipantFkNavigation.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select sh;
            }

            var existingStockHolder = query.ToArray().Select(sh => new STOCK_HOLDER_UI(rights.GroupKeyCheck) { SetFromStockHolder =sh, PARTICIPANT_NAME = sh.ParticipantFkNavigation.Name, STOCK_CLASS_NAME = sh.StockClassFkNavigation.Name });
            if (rights.IsAdmin)
            {
                var newStockHolder = new STOCK_HOLDER_UI[1] { new STOCK_HOLDER_UI(rights.GroupKeyCheck, new StockHolders() { StockHolderPk = Guid.Empty, GroupFk = rights.GroupKeyCheck }) { PARTICIPANT_NAME = "<Create New Stock Holder>", STOCK_CLASS_NAME = "" } };
                return newStockHolder.Union(existingStockHolder).ToArray();
            }

            return existingStockHolder.ToArray();
        }

        [Route("api/company/stockholders/{searchString}/{searchType}/")]
        public object GetStockHolders(string searchString, string searchType)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryStockHolders(context, rights, searchString, searchType);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "stock holder");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/stockholder/{stockHolderKey}/{uuidKey}")]
        public object GetStockHolder(Guid stockHolderKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    StockHolders stockHolder;
                    if (stockHolderKey == Guid.Empty)
                    {
                        stockHolder = new StockHolders() {GroupFk = rights.GroupKeyCheck, DateOfSale = DateTime.Now };
                    }
                    else
                    {
                        stockHolder = (from sh in context.StockHolders
                                    where sh.StockHolderPk == stockHolderKey && (sh.GroupFk == rights.GroupKey || sh.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select sh).FirstOrDefault();
                    }

                    if (stockHolder != null)
                    {
                        PARTICIPANT_UI participant = null;
                        if (stockHolder.ParticipantFk != Guid.Empty)
                            participant = (from p in context.Participants
                                           where p.ParticipantPk == stockHolder.ParticipantFk && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                           select p).ToArray().Select(p => new PARTICIPANT_UI(rights.GroupKeyCheck) { SetFromParticipant = p }).FirstOrDefault();

                        var stockClasses = (from sc in context.StockClasses
                                     where sc.GroupFk == rights.GroupKey
                                     select sc).ToArray().Select( sc => new STOCK_CLASSES_UI(rights.GroupKeyCheck) { SetFromStockClass = sc });

                        return new { success = true, StockHolder = new STOCK_HOLDER_UI(rights.GroupKeyCheck, stockHolder), Participant = participant, StockClasses = stockClasses};
                    }
                    throw new Exception("Could not find Stock Holder");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "stock holder");
                return new { success = false, message = message };
            }

        }

        [Route("api/company/stockholder/")]
        [HttpPost]
        public ActionResult SaveStockHolder(SaveData<STOCK_HOLDER_UI> saveStockHolder)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveStockHolder.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." } );
                    if (saveStockHolder.Key != saveStockHolder.Data.STOCK_HOLDER_PK)
                        return Ok(new { success = false, message = "Invalid Stock Holder, keys do not match - thx" });

                    StockHolders stockHolder;
                    if (saveStockHolder.Key == Guid.Empty)
                    {
                        stockHolder = saveStockHolder.Data.GetStockHolder(rights.GroupKey);
                        context.StockHolders.Add(stockHolder);
                    }
                    else
                    {
                        stockHolder = (from sh in context.StockHolders
                                             where sh.STOCK_HOLDER_PK == saveStockHolder.Key && sh.GroupFk == rights.GroupKey
                                     select sh).FirstOrDefault();
                        if (stockHolder == null)
                            return Ok(new { success = false, message = "Cannot find this Stock Holder - thx" });
                        saveStockHolder.Data.SetToStockHolder(stockHolder, rights.GroupKey);
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Stock Holder saved.", key= stockHolder.STOCK_HOLDER_PK });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save stock holder");
                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/stockholder/{stockHolderKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteStockHolder(Guid stockHolderKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (stockHolderKey != Guid.Empty)
                    {
                        StockHolders stockHolder = (from sh in context.StockHolders
                                             where sh.STOCK_HOLDER_PK == stockHolderKey && sh.GroupFk == rights.GroupKey
                                             select sh).FirstOrDefault();
                        if (stockHolder != null)
                        {
                            context.StockHolders.Remove(stockHolder);
                            context.SaveChanges();
                            return new { success = true, StockHolders = QueryStockHolders(context, rights, "", "") };
                        }
                    }
                }
                throw new Exception("Cannot find Stock Holder");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove Stock Holder");
                return new { success = false, message = message };
            }
        }
    }
}