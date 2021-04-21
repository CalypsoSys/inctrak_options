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
    public class StockClassController : IncTrakController
    {
        public StockClassController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryStockClasses(inctrakContext context, LoginRights rights)
        {
            IQueryable<StockClasses> query = from sc in context.StockClasses
                                              where (sc.GroupFk == rights.GroupKey || sc.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select sc;

            var existingStokcClass = query.ToArray().Select(sc => new STOCK_CLASSES_UI(rights.GroupKeyCheck) { SetFromStockClass = sc });
            if (rights.IsAdmin)
            {
                var newStockClassn = new STOCK_CLASSES_UI[1] { new STOCK_CLASSES_UI(rights.GroupKeyCheck, new StockClasses() { StockClassPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Stock Class>" }) };

                return newStockClassn.Union(existingStokcClass).ToArray();
            }

            return existingStokcClass.ToArray();
        }

        [Route("api/company/stockclasses/")]
        public object GetStockClasses()
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryStockClasses(context, rights);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get stock class");

                return new { success = false, message = message };
            }
        }

        [Route("api/company/stockclass/{stockClassKey}/{uuidKey}")]
        public object GetStockClass(Guid stockClassKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    StockClasses stockClass;
                    if (stockClassKey == Guid.Empty)
                        stockClass = new StockClasses() { GroupFk = rights.GroupKeyCheck };
                    else
                        stockClass = (from sc in context.StockClasses
                                    where sc.StockClassPk == stockClassKey && (sc.GroupFk == rights.GroupKey || sc.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select sc).FirstOrDefault();
                    if (stockClass != null)
                        return new { success = true, StockClass = new STOCK_CLASSES_UI(rights.GroupKeyCheck, stockClass) };
                }
                throw new Exception("Cannot find stock class");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a stock class");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/stockclass/")]
        [HttpPost]
        public ActionResult SaveStockClass(SaveData<STOCK_CLASSES_UI> saveStockClass)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveStockClass.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveStockClass.Key != saveStockClass.Data.STOCK_CLASS_PK)
                        return Ok(new { success = false, message = "Invalid Stock Class, keys do not match - thx" });

                    StockClasses stockClass;
                    if (saveStockClass.Key == Guid.Empty)
                    {
                        stockClass = saveStockClass.Data.GetStockClass(rights.GroupKey);
                        context.StockClasses.Add(stockClass);
                    }
                    else
                    {
                        stockClass = (from sc in context.StockClasses
                                where sc.StockClassPk == saveStockClass.Key && sc.GroupFk == rights.GroupKey
                                select sc).FirstOrDefault();
                        if (stockClass == null)
                            return Ok(new { success = false, message = "Cannot find this stock class - thx" });
                        saveStockClass.Data.SetToStockClass(stockClass, rights.GroupKey);
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Stock Class saved.", key =stockClass.StockClassPk  });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save a stock class");
                return Ok(new { success = false, message = message });
            }
        }

        [Route("api/company/stockclass/{stockClassKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteStockClass(Guid stockClassKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (stockClassKey != Guid.Empty)
                    {
                        StockClasses stockClass = (from sc in context.StockClasses
                                     where sc.StockClassPk == stockClassKey && sc.GroupFk == rights.GroupKey
                                     select sc).FirstOrDefault();
                        if (stockClass != null)
                        {
                            context.StockClasses.Remove(stockClass);
                            context.SaveChanges();
                            return new { success = true, StockClasses = QueryStockClasses(context, rights) };
                        }
                    }
                    throw new Exception("Stock Class not found");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove a stock class");
                return new { success = false, message = message };
            }
        }
    }
}